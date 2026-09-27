using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Shift.Game
{
    // Read-only rendering of authored placements. No BoardManager, Piece behaviours,
    // Button callbacks, solutions, animations, audio, or gameplay state are executed.
    public sealed class OnboardingBoardPreview : MonoBehaviour
    {
        public LevelData Source { get; private set; }
        public void Build(LevelData level, Font font, Sprite circle, Sprite rounded, bool callouts, bool routingHero=false, bool chainCue=false)
        {
            Source = routingHero ? null : level;
            // A three-element illustration, not a new campaign level or executable puzzle.
            int width=routingHero?4:level.Width, height=routingHero?4:level.Height;
            IReadOnlyList<PiecePlacement> placements=routingHero?new[]
            {
                new PiecePlacement(PieceType.Normal,PieceColor.Red,Direction.Right,new GridPosition(0,1)),
                new PiecePlacement(PieceType.Direction,PieceColor.None,Direction.Up,new GridPosition(2,1)),
                new PiecePlacement(PieceType.Exit,PieceColor.Red,Direction.None,new GridPosition(2,3))
            }:level.Placements;
            var art = gameObject.AddComponent<BoardVisualResources>();
            var frame = VisualTheme.Surface("Preview Frame", transform, art.Surface(BoardSurface.Frame, new Color32(48,76,109,255)), Color.white, Vector2.zero, Vector2.one);
            frame.rectTransform.offsetMin = new Vector2(-12,-12); frame.rectTransform.offsetMax = new Vector2(12,12);
            VisualTheme.Shadow(frame, 8);
            VisualTheme.Surface("Preview Well", transform, rounded, new Color32(18,31,46,255), Vector2.zero, Vector2.one);
            var aspect = gameObject.AddComponent<AspectRatioFitter>();
            aspect.aspectMode = AspectRatioFitter.AspectMode.FitInParent; aspect.aspectRatio = (float)width / height;
            for (int y=0; y<height; y++) for (int x=0; x<width; x++)
            {
                var cell = VisualTheme.Surface("Preview Cell", transform, art.Surface(BoardSurface.Cell,new Color32(60,82,106,255)), Color.white,
                    new Vector2((float)x/width,(float)y/height), new Vector2((float)(x+1)/width,(float)(y+1)/height));
                cell.rectTransform.offsetMin = Vector2.one*3; cell.rectTransform.offsetMax = Vector2.one*-3;
            }
            for (int layer=0; layer<2; layer++) for (int i=0; i<placements.Count; i++)
            {
                var data = new BoardPiece(i,placements[i]);
                if (data.Movable != (layer==1)) continue;
                var cell = PlaceholderVisuals.Rect("Preview " + data.Type,transform,
                    new Vector2((float)data.Position.x/width,(float)data.Position.y/height),
                    new Vector2((float)(data.Position.x+1)/width,(float)(data.Position.y+1)/height));
                var shadow=VisualTheme.Surface("Shadow",cell,data.Type==PieceType.Normal?circle:rounded,new Color(0,0,0,.23f),new Vector2(.13f,.08f),new Vector2(.89f,.84f));
                shadow.type=data.Type==PieceType.Normal?Image.Type.Simple:Image.Type.Sliced;
                var body=PlaceholderVisuals.Rect("Body",cell,new Vector2(.12f,.12f),new Vector2(.88f,.88f)).gameObject.AddComponent<Image>();
                _ = new PieceAppearance(data,body,font,circle,rounded,art);
                if (callouts)
                {
                    int mark=data.Type==PieceType.Exit?3:data.Type==PieceType.Normal&&data.Color==PieceColor.Yellow?1:data.Type==PieceType.Normal&&data.Color==PieceColor.Red?2:0;
                    if(mark>0)
                    {
                        var badge=VisualTheme.Surface("Callout "+mark,cell,circle,IdentityStyle.Cream,new Vector2(.69f,.68f),new Vector2(.97f,.96f));
                        badge.type=Image.Type.Simple;
                        var text=PlaceholderVisuals.Label("Number",badge.transform,font,mark.ToString(),22,IdentityStyle.Navy,Vector2.zero,Vector2.one); text.fontStyle=FontStyle.Bold;
                    }
                }
            }
            if(chainCue)
            {
                // The Level 4 push chain goes Yellow -> box -> Red -> exit.
                // Static inter-cell arrows keep Reduced Motion and normal mode equally calm.
                var cue=PlaceholderVisuals.Rect("Chain Sequence",transform,Vector2.zero,Vector2.one);
                for(int i=1;i<=3;i++)
                {
                    float x=(float)i/width, y=2.5f/height;
                    BoardIcon.Create("Chain Segment "+i,cue,BoardIconShape.Arrow,new Color32(231,204,139,230),
                        new Vector2(x-.018f,y-.018f),new Vector2(x+.018f,y+.018f),Direction.Right);
                }
            }
            foreach(var graphic in GetComponentsInChildren<Graphic>()) graphic.raycastTarget=false;
            var group=gameObject.AddComponent<CanvasGroup>(); group.interactable=false; group.blocksRaycasts=false;
        }
    }
}
