using System;
using UnityEngine;
using UnityEngine.UI;

namespace Shift.Game
{
    public sealed class FirstLaunchOnboardingPanel : MonoBehaviour
    {
        private FirstLaunchOnboardingService service;
        private Font font;
        private Sprite circle, rounded;
        private LevelData[] levels;
        private GameFeelSettings feel;
        private Action done, feedback;
        private Func<bool> nativeBusy;
        private RectTransform card, content;
        private CanvasGroup visibility, pageOpacity;
        private Text primaryLabel, secondaryLabel;
        private readonly Image[] dots = new Image[4];
        private float fade;
        public int PageIndex => service.PageIndex;
        public bool WaitingForNative => nativeBusy?.Invoke() ?? false;
        public void Build(FirstLaunchOnboardingService state, Font font, Sprite circle, Sprite rounded,
            LevelData[] levels, GameFeelSettings feel, Func<bool> nativeBusy, Action done, Action feedback)
        {
            service=state; this.font=font; this.circle=circle; this.rounded=rounded;
            this.levels=levels; this.feel=feel; this.nativeBusy=nativeBusy; this.done=done; this.feedback=feedback;
            visibility=gameObject.AddComponent<CanvasGroup>();
            Wordmark.Build(transform,font);
            var surface=VisualTheme.Surface("Onboarding Card",transform,rounded,IdentityStyle.Cream,new Vector2(.07f,.055f),new Vector2(.93f,.885f));
            IdentityStyle.Material(surface); VisualTheme.Shadow(surface,8); card=surface.rectTransform;
            var secondary=ChapterSelect.CreateButton("Onboarding Secondary",font,rounded,card,new Vector2(.065f,.06f),new Vector2(.40f,.135f),Secondary,out secondaryLabel);
            var primary=ChapterSelect.CreateButton("Onboarding Primary",font,rounded,card,new Vector2(.43f,.06f),new Vector2(.935f,.135f),Primary,out primaryLabel);
            primary.targetGraphic.color=IdentityStyle.Teal;
            secondary.targetGraphic.color=new Color32(35,57,80,255);
            secondaryLabel.color=new Color32(216,228,234,255);
            secondaryLabel.resizeTextMaxSize=30;
            foreach(var button in new[]{primary,secondary}) button.GetComponent<PresentationMotion>().Settings=feel;
            primaryLabel.fontStyle=FontStyle.Bold; primaryLabel.fontSize=34;
            LocalizedLabel.Bind(primaryLabel,()=>GameLanguageService.Shared.Text("onboarding."+(PageIndex==0?"start":PageIndex==3?"play":"continue")));
            LocalizedLabel.Bind(secondaryLabel,()=>GameLanguageService.Shared.Text("onboarding."+(PageIndex==0?"skip":"back")));
            for(int i=0;i<4;i++)
            {
                var dot=PlaceholderVisuals.Rect("Page Indicator "+i,card,new Vector2(.44f+i*.04f,.026f),new Vector2(.44f+i*.04f,.026f));
                dot.sizeDelta=new Vector2(12,12); dots[i]=dot.gameObject.AddComponent<Image>(); dots[i].sprite=circle; dots[i].raycastTarget=false;
            }
            Present();
        }
        private void Present()
        {
            if(WaitingForNative) { visibility.alpha=0; visibility.interactable=false; visibility.blocksRaycasts=false; return; }
            if(service.Begin()) RenderPage();
            visibility.alpha=1; visibility.interactable=true; visibility.blocksRaycasts=true;
        }
        private void Update()
        {
            if(service==null || service.Completed) return;
            Present();
            if(pageOpacity!=null) { fade=Mathf.Min(1,fade+Time.unscaledDeltaTime/.14f); pageOpacity.alpha=feel.reducedMotion?1:Mathf.Lerp(.65f,1,fade); }
        }
        public void Primary()
        {
            if(WaitingForNative || service.Completed) return;
            if(PageIndex==3) { service.Finish(false,done); feedback?.Invoke(); return; }
            if(service.Next()) { feedback?.Invoke(); RenderPage(); }
        }
        public void Secondary()
        {
            if(WaitingForNative || service.Completed) return;
            if(PageIndex==0) { service.Finish(true,done); feedback?.Invoke(); return; }
            if(service.Back()) { feedback?.Invoke(); RenderPage(); }
        }
        private Text Copy(string name,string key,int size,Vector2 min,Vector2 max,bool bold=false)
        {
            var text=PlaceholderVisuals.Label(name,content,font,"",size,new Color32(27,52,69,255),min,max);
            text.fontStyle=bold?FontStyle.Bold:FontStyle.Normal; text.lineSpacing=1.12f;
            LocalizedLabel.Bind(text,"onboarding."+key); return text;
        }
        private void Row(string key,int index,int count,bool gold=false,string number=null)
        {
            float top=PageIndex==1?.35f:count==2?.29f:.33f, height=PageIndex==1?.045f:.054f, y=top-index*(PageIndex==1?.054f:.062f);
            var row=VisualTheme.Surface("Support Row",content,rounded,gold?new Color32(239,204,119,150):new Color32(218,215,191,75),new Vector2(.065f,y-height),new Vector2(.935f,y));
            float left=.09f;
            if(number!=null)
            {
                var label=PlaceholderVisuals.Label("Callout Number",row.transform,font,number,26,IdentityStyle.Navy,new Vector2(.02f,.08f),new Vector2(.10f,.92f)); label.fontStyle=FontStyle.Bold;
                left=.18f;
            }
            var text=Copy("Support",key,28,new Vector2(left,y-height+.003f),new Vector2(.91f,y-.003f));
            text.alignment=number==null?TextAnchor.MiddleCenter:TextAnchor.MiddleLeft;
        }
        private void RenderPage()
        {
            if(content!=null) { content.gameObject.SetActive(false); Destroy(content.gameObject); }
            content=PlaceholderVisuals.Rect("Onboarding Page "+PageIndex,card,Vector2.zero,Vector2.one);
            content.SetAsFirstSibling(); pageOpacity=content.gameObject.AddComponent<CanvasGroup>(); pageOpacity.blocksRaycasts=false;
            fade=0; pageOpacity.alpha=feel.reducedMotion?1:.65f;
            string prefix=new[]{"welcome","how","chain","ready"}[PageIndex];
            Copy("Onboarding Title",prefix+".title",48,new Vector2(.065f,.878f),new Vector2(.935f,.951f),true);
            Copy("Onboarding Subtitle",prefix+".subtitle",30,new Vector2(.08f,.774f),new Vector2(.92f,.869f));
            var area=PlaceholderVisuals.Rect("Preview Area",content,new Vector2(.13f,.369f),new Vector2(.87f,.744f));
            var preview=PlaceholderVisuals.Rect("Authentic Board Preview",area,Vector2.zero,Vector2.one).gameObject.AddComponent<OnboardingBoardPreview>();
            int index=PageIndex==3?0:PageIndex==2?3:9;
            if(levels!=null && levels.Length>index && levels[index]!=null) preview.Build(levels[index],font,circle,rounded,PageIndex==1,PageIndex==3,PageIndex==2);
            if(PageIndex==0) { Row("welcome.levels",0,3); Row("welcome.daily",1,3); Row("welcome.perfect",2,3); }
            else if(PageIndex==1)
            {
                Row("how.move",0,3,false,"1"); Row("how.direction",1,3,false,"2"); Row("how.exit",2,3,false,"3");
                Copy("Tip","how.tip",24,new Vector2(.08f,.139f),new Vector2(.92f,.186f));
            }
            else if(PageIndex==2) { Row("chain.reaction",0,3); Row("chain.perfect",1,3,true); Row("chain.help",2,3); }
            else { Row("ready.campaign",0,2); Row("ready.daily",1,2); }
            for(int i=0;i<4;i++) dots[i].color=i==PageIndex?IdentityStyle.Teal:new Color32(178,194,197,255);
            LocalizedLabel.Bind(primaryLabel,()=>GameLanguageService.Shared.Text("onboarding."+(PageIndex==0?"start":PageIndex==3?"play":"continue")));
            LocalizedLabel.Bind(secondaryLabel,()=>GameLanguageService.Shared.Text("onboarding."+(PageIndex==0?"skip":"back")));
        }
    }
}
