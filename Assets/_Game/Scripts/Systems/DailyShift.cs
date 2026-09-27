using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;

namespace Shift.Game
{
    public sealed class DailyPuzzle
    {
        public DateTime Date { get; }
        public string DateKey => DailyPool.Key(Date);
        public int SourceIndex { get; }
        public LevelData Level { get; }
        public int PoolVersion => DailyPool.Version;
        public DailyPuzzle(DateTime date,int index,LevelData level){Date=date.Date;SourceIndex=index;Level=level;}
    }
    public sealed class DailyPool
    {
        public const int Version=1;
        // Explicit immutable v1 curation: six Easy/Planning, six Advanced/Mastery/Finale.
        private static readonly int[] approachable={20,21,23,25,30,35};
        private static readonly int[] challenging={24,27,29,34,37,39};
        private readonly LevelData[] campaign;
        public DailyPool(LevelData[] levels)
        {
            campaign=(LevelData[])levels.Clone();
            foreach(int i in approachable.Concat(challenging))
                if(i>=campaign.Length||campaign[i]==null||!campaign[i].ValidatePlayable(out _)||!campaign[i].VerifiedOptimalMoveCount.HasValue)
                    throw new ArgumentException("Daily pool requires existing verified campaign references.");
        }
        public IReadOnlyList<int> SourceIndices => Array.AsReadOnly(approachable.Concat(challenging).ToArray());
        public static string Key(DateTime date)=>date.ToString("yyyy-MM-dd",CultureInfo.InvariantCulture);
        public static bool TryDate(string key,out DateTime date)=>DateTime.TryParseExact(key,"yyyy-MM-dd",CultureInfo.InvariantCulture,DateTimeStyles.None,out date);
        public DailyPuzzle For(DateTime date)
        {
            // Date ordinal avoids timezone/DST elapsed-hour arithmetic and negative modulo.
            long ordinal=date.Date.Ticks/TimeSpan.TicksPerDay;long cycle=ordinal/12;int slot=(int)(ordinal%12);
            var half=(int[])(slot%2==0?approachable:challenging).Clone();
            uint state=unchecked((uint)cycle*747796405u+2891336453u+(uint)(slot%2)*277803737u+(uint)Version);
            for(int i=half.Length-1;i>0;i--)
            {state=unchecked(state*1664525u+1013904223u);int j=(int)(state%(uint)(i+1));int t=half[i];half[i]=half[j];half[j]=t;}
            int source=half[slot/2];return new DailyPuzzle(date,source,campaign[source]);
        }
        public DailyPuzzle[] Archive(DateTime today)=>Enumerable.Range(0,7).Select(i=>For(today.Date.AddDays(-i))).ToArray();
    }
    [Serializable] public sealed class DailyRecord
    {
        public string dateKey,sourceId;
        public int sourceIndex,poolVersion;
        public bool completed,perfect;
        public DailyRecord Copy()=>(DailyRecord)MemberwiseClone();
    }
    public sealed class DailySaveService
    {
        [Serializable] private sealed class Store { public int version=1;public List<DailyRecord> records=new List<DailyRecord>(); }
        private readonly string key;
        private readonly Dictionary<string,DailyRecord> records=new Dictionary<string,DailyRecord>();
        public DailySaveService(string key="SHIFT.Daily.v1.History")
        {
            this.key=key;
            try
            {
                var raw=PlayerPrefs.GetString(key,"");if(string.IsNullOrEmpty(raw))return;
                var data=JsonUtility.FromJson<Store>(raw);if(data==null||data.version!=1||data.records==null)return;
                foreach(var r in data.records)
                {
                    if(r==null||!DailyPool.TryDate(r.dateKey,out _)||r.poolVersion<1||r.sourceIndex<0||r.sourceIndex>=40||string.IsNullOrEmpty(r.sourceId)||!r.completed)continue;
                    string id=Id(r.dateKey,r.poolVersion);
                    if(records.TryGetValue(id,out var prior)){if(prior.sourceId==r.sourceId&&prior.sourceIndex==r.sourceIndex)prior.perfect|=r.perfect;}
                    else records[id]=r.Copy();
                }
            }
            catch(ArgumentException){/* Invalid local metadata never resets campaign progress. */}
        }
        private static string Id(string date,int version)=>version+":"+date;
        public DailyRecord Get(DailyPuzzle puzzle)
        {
            if(records.TryGetValue(Id(puzzle.DateKey,puzzle.PoolVersion),out var r)&&r.sourceIndex==puzzle.SourceIndex&&r.sourceId==puzzle.Level.name)return r.Copy();
            return new DailyRecord{dateKey=puzzle.DateKey,poolVersion=puzzle.PoolVersion,sourceIndex=puzzle.SourceIndex,sourceId=puzzle.Level.name};
        }
        public bool Record(DailyPuzzle puzzle,AttemptMetrics result,out bool firstCompletion,out bool firstPerfect)
        {
            firstCompletion=firstPerfect=false;
            if(result==null||!result.completed||result.levelId!=puzzle.Level.name||result.levelIndex!=puzzle.SourceIndex)return false;
            var r=Get(puzzle);firstCompletion=!r.completed;
            bool perfect=result.perfectShift&&result.hasOptimal&&puzzle.Level.VerifiedOptimalMoveCount.HasValue&&result.verifiedOptimalMoves==puzzle.Level.VerifiedOptimalMoveCount;
            firstPerfect=perfect&&!r.perfect;r.completed=true;r.perfect|=perfect;records[Id(r.dateKey,r.poolVersion)]=r;
            if(firstCompletion||firstPerfect)
            {
                var data=new Store{records=records.Values.OrderBy(x=>x.dateKey,StringComparer.Ordinal).ThenBy(x=>x.poolVersion).ToList()};
                PlayerPrefs.SetString(key,JsonUtility.ToJson(data));PlayerPrefs.Save();
            }
            return true;
        }
    }
    public static class DailyText
    {
        public static string Title(HintLanguage l) => LocalizationCatalog.ForHint("daily.title", l);
        public static string Today(HintLanguage l) => LocalizationCatalog.ForHint("daily.today", l);
        public static string State(DailyRecord r, HintLanguage l) => LocalizationCatalog.ForHint(r.perfect ? "daily.perfect_state" : r.completed ? "daily.completed" : "daily.puzzle", l);
        public static string Archive(HintLanguage l) => LocalizationCatalog.ForHint("daily.archive", l);
        public static string Date(DateTime d, HintLanguage l) => d.ToString("d MMMM", CultureInfo.GetCultureInfo(l == HintLanguage.Turkish ? "tr-TR" : "en-US"));
        public static string Complete(bool perfect, HintLanguage l) => LocalizationCatalog.ForHint(perfect ? "daily.perfect" : "daily.complete", l);
        public static string Back(HintLanguage l) => LocalizationCatalog.ForHint("daily.back", l);
    }
}
