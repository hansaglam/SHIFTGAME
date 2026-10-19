using System;
using System.Collections.Generic;
using UnityEngine;

namespace Shift.Game
{
    public enum MechanicTopic { Push, Direction, Exits, Rotator, Gates, Channels, Objectives }

    // Content discovery, never a solution lookup or a filename/index convention.
    public static class MechanicGuideCatalog
    {
        public static readonly MechanicTopic[] Topics = (MechanicTopic[])Enum.GetValues(typeof(MechanicTopic));
        public static string Key(MechanicTopic topic) => "guide." + topic.ToString().ToLowerInvariant();
        public static bool Contains(LevelData level, MechanicTopic topic)
        {
            if (level == null) return false;
            bool box=false, direction=false, exit=false, rotator=false, sw=false, gate=false;
            var channels=new HashSet<int>();
            foreach(var p in level.Placements)
            {
                box |= p.type==PieceType.PushBlock; direction |= p.type==PieceType.Direction;
                exit |= p.type==PieceType.Exit && p.color!=PieceColor.None;
                rotator |= p.type==PieceType.Rotator; sw |= p.type==PieceType.Switch; gate |= p.type==PieceType.Gate;
                if(p.type==PieceType.Gate) channels.Add(p.channel);
            }
            switch(topic)
            {
                case MechanicTopic.Push: return box;
                case MechanicTopic.Direction: return direction;
                case MechanicTopic.Exits: return exit;
                case MechanicTopic.Rotator: return rotator;
                case MechanicTopic.Gates: return sw && gate;
                case MechanicTopic.Channels: return sw && channels.Count>1;
                case MechanicTopic.Objectives: return level.ResolvedTargetColors.Count>1;
                default: return false;
            }
        }
        // Independent symbolic samples. Never use the active level, authored hints or certified routes.
        public static PiecePlacement[] Example(MechanicTopic topic)
        {
            PiecePlacement P(PieceType t, PieceColor c=PieceColor.None, Direction d=Direction.None, int channel=0, bool open=false)
                => new PiecePlacement(t,c,d,new GridPosition(0,0),channel,open);
            switch(topic)
            {
                case MechanicTopic.Push: return new[]{P(PieceType.Normal,PieceColor.Yellow,Direction.Right),P(PieceType.PushBlock),P(PieceType.Normal,PieceColor.Red,Direction.Up)};
                case MechanicTopic.Direction: return new[]{P(PieceType.Normal,PieceColor.Blue,Direction.Right),P(PieceType.Direction,PieceColor.None,Direction.Up),P(PieceType.Normal,PieceColor.Blue,Direction.Up)};
                case MechanicTopic.Exits: return new[]{P(PieceType.Normal,PieceColor.Red,Direction.Right),P(PieceType.Exit,PieceColor.Red),P(PieceType.Exit,PieceColor.Blue)};
                case MechanicTopic.Rotator: return new[]{P(PieceType.Normal,PieceColor.Red,Direction.Up),P(PieceType.Rotator),P(PieceType.Normal,PieceColor.Red,Direction.Right)};
                case MechanicTopic.Gates: return new[]{P(PieceType.Switch,channel:1),P(PieceType.Gate,channel:1),P(PieceType.Gate,channel:1,open:true)};
                case MechanicTopic.Channels: return new[]{P(PieceType.Switch,channel:1),P(PieceType.Gate,channel:1),P(PieceType.Switch,channel:2),P(PieceType.Gate,channel:2)};
                default: return new[]{P(PieceType.Normal,PieceColor.Red,Direction.Up),P(PieceType.Normal,PieceColor.Blue,Direction.Right),P(PieceType.Exit,PieceColor.Red),P(PieceType.Exit,PieceColor.Blue)};
            }
        }
    }

    // All guide flags live in one independent key. No progression/settings/allowance writes.
    public sealed class MechanicGuideState
    {
        public const string SaveKey="SHIFT.MechanicGuide.v1";
        [Serializable] private sealed class Flags { public int presented, opened, suppressed; }
        private readonly string key;
        private readonly Flags flags;
        public MechanicGuideState(string key=SaveKey, LevelData[] campaign=null, int highestCompleted=-1)
        {
            this.key=key;
            try { flags=JsonUtility.FromJson<Flags>(PlayerPrefs.GetString(key,"")) ?? new Flags(); }
            catch(ArgumentException) { flags=new Flags(); }
            // Completed content suppresses retrospective notices, without claiming it was read.
            int before=flags.suppressed;
            if(campaign!=null) for(int i=0;i<campaign.Length && i<=highestCompleted;i++)
                foreach(var topic in MechanicGuideCatalog.Topics)
                    if(MechanicGuideCatalog.Contains(campaign[i],topic)) flags.suppressed |= Bit(topic);
            if(before!=flags.suppressed) Save();
        }
        private static int Bit(MechanicTopic topic) => 1 << (int)topic;
        public bool Presented(MechanicTopic topic) => (flags.presented & Bit(topic))!=0;
        public bool Opened(MechanicTopic topic) => (flags.opened & Bit(topic))!=0;
        public List<MechanicTopic> Pending(LevelData level)
        {
            var result=new List<MechanicTopic>();
            foreach(var topic in MechanicGuideCatalog.Topics)
                if(((flags.presented|flags.opened|flags.suppressed)&Bit(topic))==0 && MechanicGuideCatalog.Contains(level,topic)) result.Add(topic);
            return result;
        }
        public void Present(IEnumerable<MechanicTopic> topics) { foreach(var topic in topics) flags.presented|=Bit(topic); Save(); }
        public void Open(MechanicTopic topic) { flags.opened|=Bit(topic); Save(); }
        private void Save() { PlayerPrefs.SetString(key,JsonUtility.ToJson(flags)); PlayerPrefs.Save(); }
    }
}
