using UnityEngine;
using UnityEngine.UI;

namespace Shift.Game
{
    // Cover-crop, never stretch; adapts to portrait aspect and safe-area changes.
    public sealed class ScenicBackdrop : RawImage
    {
        protected override void OnRectTransformDimensionsChange() { base.OnRectTransformDimensionsChange(); Fit(); }
        public void Initialize(Texture2D art) { texture=art; raycastTarget=false; Fit(); }
        private void Fit()
        {
            if(texture==null || rectTransform.rect.height<=0) return;
            float aspect=rectTransform.rect.width/rectTransform.rect.height, artAspect=(float)texture.width/texture.height;
            float width=Mathf.Min(1,aspect/artAspect),height=Mathf.Min(1,artAspect/aspect);
            uvRect=new Rect((1-width)*.5f,(1-height)*.5f,width,height);
        }
    }
}
