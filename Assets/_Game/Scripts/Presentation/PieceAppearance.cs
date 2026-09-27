using UnityEngine;
using UnityEngine.UI;

namespace Shift.Game
{
    // A view-only composition. Gate state and direction are supplied by action playback.
    public sealed class PieceAppearance
    {
        private BoardIcon arrow, arrowShadow;
        private GameObject barrier;
        private Image threshold, flash;
        public bool GateShownOpen { get; private set; }
        public static Color PieceTint(PieceColor color) => color switch
        {
            PieceColor.Red => new Color32(246,65,80,255), PieceColor.Blue => new Color32(38,143,248,255),
            PieceColor.Yellow => new Color32(255,200,51,255), PieceColor.Green => new Color32(77,196,79,255),
            _ => new Color32(149,176,193,255)
        };
        public PieceAppearance(BoardPiece data,Image root,Font font,Sprite circle,Sprite rounded,BoardVisualResources art)
        {
            var body=root.rectTransform;
            Image Surface(string name,Transform parent,Color color,Vector2 min,Vector2 max,BoardSurface style=BoardSurface.Raised)
            { return VisualTheme.Surface(name,parent,art.Surface(style,color),Color.white,min,max); }
            root.color=Color.white; root.type=data.Type==PieceType.Normal?Image.Type.Simple:Image.Type.Sliced;
            Color tint=data.Type switch
            {
                PieceType.Normal => PieceTint(data.Color), PieceType.Direction => new Color32(97,126,158,255),
                PieceType.Rotator => new Color32(123,100,171,255), PieceType.PushBlock => new Color32(191,134,69,255),
                PieceType.Switch => new Color32(97,73,43,255), PieceType.Gate => new Color32(41,57,70,255),
                PieceType.Exit => data.Color==PieceColor.None?new Color32(51,190,142,255):PieceTint(data.Color),
                _ => new Color32(46,63,79,255)
            };
            root.sprite=art.Surface(data.Type==PieceType.Normal?BoardSurface.Disc:BoardSurface.Raised,tint);
            if(data.Type==PieceType.Normal || data.Type==PieceType.Direction)
            {
                Color ink=data.Type==PieceType.Normal?new Color32(22,42,65,255):new Color32(248,252,255,255);
                arrowShadow=BoardIcon.Create("Arrow Relief",body,BoardIconShape.Arrow,data.Type==PieceType.Normal?new Color(1,1,1,.22f):new Color(0,0,0,.27f),new Vector2(.16f,.14f),new Vector2(.84f,.82f),data.Direction);
                arrow=BoardIcon.Create("Direction Icon",body,BoardIconShape.Arrow,ink,new Vector2(.16f,.17f),new Vector2(.84f,.85f),data.Direction);
            }
            else if(data.Type==PieceType.Rotator)
            {
                BoardIcon.Create("Rotation Relief",body,BoardIconShape.Clockwise,new Color(0,0,0,.24f),new Vector2(.09f,.07f),new Vector2(.91f,.89f));
                BoardIcon.Create("Clockwise Icon",body,BoardIconShape.Clockwise,new Color32(248,244,255,255),new Vector2(.09f,.10f),new Vector2(.91f,.92f));
            }
            else if(data.Type==PieceType.PushBlock)
            {
                var inset=Surface("Block Inset",body,new Color32(125,79,43,255),new Vector2(.13f,.13f),new Vector2(.87f,.87f),BoardSurface.Well);
                BoardIcon.Create("Block Brace Shadow",inset.transform,BoardIconShape.Crate,new Color32(80,48,29,255),new Vector2(.04f,.01f),new Vector2(.96f,.93f));
                BoardIcon.Create("Block Braces",inset.transform,BoardIconShape.Crate,new Color32(222,168,94,255),new Vector2(.04f,.05f),new Vector2(.96f,.97f));
                foreach(var p in new[]{new Vector2(.1f,.1f),new Vector2(.9f,.1f),new Vector2(.1f,.9f),new Vector2(.9f,.9f)})
                    VisualTheme.Surface("Block Rivet",body,circle,new Color32(107,76,47,255),p-Vector2.one*.025f,p+Vector2.one*.025f).type=Image.Type.Simple;
            }
            else if(data.Type==PieceType.Exit)
            {
                Surface("Exit Inner Light",body,Color.Lerp(tint,Color.white,.5f),new Vector2(.06f,.06f),new Vector2(.94f,.94f));
                Surface("Exit Well",body,Color.Lerp(tint,new Color32(12,23,36,255),.78f),new Vector2(.115f,.115f),new Vector2(.885f,.885f),BoardSurface.Well);
                var label=PlaceholderVisuals.Label("Symbol",body,font,"EXIT",34,new Color32(255,248,244,255),new Vector2(.13f,.20f),new Vector2(.87f,.80f));
                LocalizedLabel.Bind(label, "tile.exit"); label.fontStyle=FontStyle.Bold;
                VisualTheme.Surface("Exit Threshold",body,rounded,Color.Lerp(tint,Color.white,.38f),new Vector2(.27f,.14f),new Vector2(.73f,.17f));
            }
            else if(data.Type==PieceType.Switch)
            {
                Surface("Pad Recess",body,new Color32(55,47,37,255),new Vector2(.07f,.07f),new Vector2(.93f,.93f),BoardSurface.Well);
                var pad=Surface("Raised Pad",body,new Color32(228,174,77,255),new Vector2(.16f,.19f),new Vector2(.84f,.84f));
                Channel(pad.transform,data.Channel,new Color32(255,245,202,255),new Vector2(.06f,.08f),new Vector2(.94f,.92f));
            }
            else if(data.Type==PieceType.Gate)
            {
                Surface("Gate Track",body,new Color32(24,38,50,255),new Vector2(.13f,.09f),new Vector2(.87f,.76f),BoardSurface.Well);
                var metal=new Color32(182,151,95,255);
                Surface("Gate Left Post",body,metal,new Vector2(.04f,.08f),new Vector2(.16f,.78f));
                Surface("Gate Right Post",body,metal,new Vector2(.84f,.08f),new Vector2(.96f,.78f));
                var header=Surface("Gate Channel Header",body,new Color32(196,157,90,255),new Vector2(.08f,.76f),new Vector2(.92f,.97f));
                Channel(header.transform,data.Channel,new Color32(255,248,214,255),new Vector2(.10f,.04f),new Vector2(.90f,.96f));
                var bars=PlaceholderVisuals.Rect("Closed Barrier",body,new Vector2(.17f,.15f),new Vector2(.83f,.73f)); barrier=bars.gameObject;
                for(int i=0;i<3;i++) Surface("Barrier Slat",bars,metal,new Vector2(.05f+i*.34f,0),new Vector2(.22f+i*.34f,1));
                Surface("Barrier Crossbar",bars,new Color32(202,174,120,255),new Vector2(0,.39f),new Vector2(1,.55f));
                threshold=VisualTheme.Surface("Open Passage",body,rounded,new Color32(118,188,163,255),new Vector2(.25f,.10f),new Vector2(.75f,.14f));
                ShowGate(data.GateOpen);
            }
            flash=VisualTheme.Surface("Surface Feedback",body,data.Type==PieceType.Normal?circle:rounded,Color.clear,Vector2.zero,Vector2.one);
            flash.type=data.Type==PieceType.Normal?Image.Type.Simple:Image.Type.Sliced;
        }
        public static BoardIcon Channel(Transform parent,int channel,Color color,Vector2 min,Vector2 max)
            => BoardIcon.Create("Channel Symbol",parent,channel==1?BoardIconShape.Diamond:BoardIconShape.DoubleDiamond,color,min,max);
        public void Direction(Direction value)
        { if(arrow!=null) { arrow.Configure(BoardIconShape.Arrow,value); arrowShadow.Configure(BoardIconShape.Arrow,value); } }
        public void ShowGate(bool open)
        { GateShownOpen=open; if(barrier!=null) barrier.SetActive(!open); if(threshold!=null) threshold.gameObject.SetActive(open); }
        public void Highlight(float amount) { if(flash!=null) flash.color=new Color(1,1,1,amount); }
    }
}
