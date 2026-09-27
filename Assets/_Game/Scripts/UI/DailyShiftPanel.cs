using System;
using UnityEngine;
using UnityEngine.UI;
namespace Shift.Game
{
    public sealed class DailyShiftPanel:MonoBehaviour
    {
        private HintLanguage Language => GameLanguageService.Shared.HintLanguage;
        private Text title,todayLabel,archiveTitle,closeLabel;
        private Button todayButton;
        private readonly Text[] labels=new Text[7];
        private readonly Button[] buttons=new Button[7];
        private DailyPuzzle[] dates;
        private DailyPuzzle todayPuzzle;
        public bool IsOpen=>gameObject.activeSelf;
        public void Build(Font font,Sprite rounded,GameFeelSettings settings,Action<DailyPuzzle,bool> select,Action close)
        {
            var shade=gameObject.AddComponent<Image>();shade.color=new Color(.055f,.13f,.23f,.88f);
            var card=VisualTheme.Surface("Daily Card",transform,rounded,IdentityStyle.Cream,new Vector2(.07f,.075f),new Vector2(.93f,.925f));IdentityStyle.Material(card);VisualTheme.Shadow(card,8);
            title=PlaceholderVisuals.Label("Daily Title",transform,font,"",46,VisualTheme.Ink,new Vector2(.1f,.82f),new Vector2(.9f,.89f));
            todayButton=ChapterSelect.CreateButton("Play Today",font,rounded,transform,new Vector2(.16f,.70f),new Vector2(.84f,.80f),()=>select(todayPuzzle,false),out todayLabel);
            archiveTitle=PlaceholderVisuals.Label("Archive Title",transform,font,"",30,VisualTheme.Ink,new Vector2(.14f,.635f),new Vector2(.86f,.68f));
            for(int i=0;i<7;i++)
            {
                int index=i;float y=.57f-i*.061f;
                buttons[i]=ChapterSelect.CreateButton("Daily Date "+i,font,rounded,transform,new Vector2(.16f,y),new Vector2(.84f,y+.050f),()=>select(dates[index],true),out labels[i]);labels[i].fontSize=26;
            }
            ChapterSelect.CreateButton("Close Daily",font,rounded,transform,new Vector2(.30f,.10f),new Vector2(.70f,.16f),close,out closeLabel);
            foreach(var motion in GetComponentsInChildren<PresentationMotion>())motion.Settings=settings;
            gameObject.SetActive(false);
        }
        public void Open(DailyPool pool,DailySaveService saves,DateTime today,HintLanguage language)
        {
            todayPuzzle=pool.For(today);dates=pool.Archive(today);LocalizedLabel.Bind(title, () => DailyText.Title(Language)); LocalizedLabel.Bind(archiveTitle, () => DailyText.Archive(Language));
            var record=saves.Get(todayPuzzle);LocalizedLabel.Bind(todayLabel, () => DailyText.Date(today,Language)+"\n"+DailyText.State(record,Language));todayLabel.fontSize=28;
            todayButton.targetGraphic.color=record.perfect?new Color32(181,137,48,255):IdentityStyle.Teal;
            for(int i=0;i<7;i++)
            {
                int index = i;
                var r=saves.Get(dates[i]);
                LocalizedLabel.Bind(labels[i], () => DailyText.Date(dates[index].Date,Language)+(index==0?" · "+DailyText.Today(Language):"")+"    "+
                    (r.perfect || r.completed ? DailyText.State(r, Language) : "—"));
                buttons[i].targetGraphic.color=r.perfect?new Color32(181,137,48,255):i==0?IdentityStyle.Teal:IdentityStyle.Navy;
            }
            LocalizedLabel.Bind(closeLabel, "common.back_title");
            transform.SetAsLastSibling();gameObject.SetActive(true);
        }
        public void Close()=>gameObject.SetActive(false);
    }
}
