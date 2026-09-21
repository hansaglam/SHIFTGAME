using System.Collections.Generic;
using UnityEngine;

namespace Shift.Game
{
    public enum BoardSurface { Disc, Raised, Cell, Frame, Well }

    // Small, shared runtime textures owned by one board. Generated only during construction.
    // No imported art, custom shader, per-frame texture work or global lifetime leaks.
    [ExecuteAlways]
    public sealed class BoardVisualResources : MonoBehaviour
    {
        private readonly Dictionary<string, Sprite> sprites = new Dictionary<string, Sprite>();
        public int SurfaceCount => sprites.Count;
        public Sprite Surface(BoardSurface style, Color tint)
        {
            string key = style + ColorUtility.ToHtmlStringRGBA(tint);
            if (sprites.TryGetValue(key, out var existing)) return existing;
            int size = style == BoardSurface.Disc ? 192 : 128;
            var texture = new Texture2D(size,size,TextureFormat.RGBA32,false)
            { name = "SHIFT generated " + key, filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Clamp, hideFlags = HideFlags.DontSave };
            var pixels = new Color32[size*size];
            for(int y=0;y<size;y++) for(int x=0;x<size;x++)
            {
                float px = (x+.5f)/size*2-1, py = (y+.5f)/size*2-1;
                float radius = Mathf.Sqrt(px*px+py*py);
                float distance;
                if(style==BoardSurface.Disc) distance = radius-.96f;
                else
                {
                    var q = new Vector2(Mathf.Abs(px),Mathf.Abs(py))-Vector2.one*.68f;
                    distance = new Vector2(Mathf.Max(q.x,0),Mathf.Max(q.y,0)).magnitude + Mathf.Min(Mathf.Max(q.x,q.y),0)-.28f;
                }
                float edge = -distance;
                float alpha = Mathf.Clamp01(.5f-distance*size*.5f);
                float light = Mathf.Clamp01(.5f + (-px+py)*.30f);
                Color c;
                if(style==BoardSurface.Disc)
                {
                    if(edge<.045f) c=Color.Lerp(new Color(.025f,.045f,.075f),tint,.25f);
                    else if(edge<.10f) c=Color.Lerp(tint*.42f,Color.Lerp(tint,Color.white,.34f),light);
                    else if(edge<.145f) c=tint*.62f;
                    else
                    {
                        float dome=Mathf.Clamp01(1-radius*radius);
                        c=Color.Lerp(tint*.74f,tint,Mathf.Clamp01(.34f+.54f*light+.28f*dome));
                        float gleam=Mathf.Exp(-((px+.30f)*(px+.30f)/.10f+(py-.43f)*(py-.43f)/.018f));
                        c=Color.Lerp(c,Color.white,.42f*gleam);
                    }
                }
                else
                {
                    bool recess=style==BoardSurface.Cell || style==BoardSurface.Well;
                    if(edge<.028f) c=tint*.38f;
                    else if(edge<.075f) c=Color.Lerp(tint*.55f,Color.Lerp(tint,Color.white,style==BoardSurface.Frame?.24f:.18f),light);
                    else if(edge<.12f) c=Color.Lerp(tint*.73f,tint*1.05f,light);
                    else c=Color.Lerp(tint*(recess?.88f:.82f),tint,Mathf.Clamp01(.5f+py*.28f-px*.14f));
                }
                c.a=alpha; pixels[y*size+x]=c;
            }
            texture.SetPixels32(pixels); texture.Apply(false,true);
            var sprite=Sprite.Create(texture,new Rect(0,0,size,size),new Vector2(.5f,.5f),100,0,SpriteMeshType.FullRect,
                style==BoardSurface.Disc?Vector4.zero:Vector4.one*24);
            sprite.name=texture.name; sprite.hideFlags=HideFlags.DontSave; sprites.Add(key,sprite); return sprite;
        }
        private void OnDestroy()
        {
            foreach(var sprite in sprites.Values)
            {
                if(sprite==null) continue;
                if(Application.isPlaying) { Destroy(sprite.texture); Destroy(sprite); }
                else { DestroyImmediate(sprite.texture); DestroyImmediate(sprite); }
            }
            sprites.Clear();
        }
    }
}
