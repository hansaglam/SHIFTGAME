using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Shift.Game
{
    public sealed class MechanicGuidePanel : MonoBehaviour
    {
        private Font font; private Sprite circle,rounded;
        private GameFeelSettings feel;
        private MechanicGuideState state;
        private Action close;
        private Transform content;
        private readonly Dictionary<CanvasGroup,bool> inputOwners=new Dictionary<CanvasGroup,bool>();
        private void TakeInput()
        {
            if(UnityEngine.EventSystems.EventSystem.current!=null)
                UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
            if(inputOwners.Count>0)return;
            foreach(Transform sibling in transform.parent)
            {
                if(sibling==transform)continue;
                var group=sibling.GetComponent<CanvasGroup>();
                if(group==null)group=sibling.gameObject.AddComponent<CanvasGroup>();
                inputOwners.Add(group,group.interactable);group.interactable=false;
            }
        }
        private void OnDisable()
        {
            foreach(var owner in inputOwners)if(owner.Key!=null)owner.Key.interactable=owner.Value;
            inputOwners.Clear();
        }
        public bool IsOpen => gameObject.activeSelf;
        public void Build(Font font,Sprite circle,Sprite rounded,GameFeelSettings feel,MechanicGuideState state,Action close)
        {
            this.font=font;this.circle=circle;this.rounded=rounded;this.feel=feel;this.state=state;this.close=close;
            gameObject.AddComponent<Image>().color=new Color(.055f,.13f,.23f,.88f);
            IdentityStyle.Modal(transform,rounded,"Guide Card");
            content=PlaceholderVisuals.Rect("Guide Content",transform,new Vector2(.1f,.11f),new Vector2(.9f,.89f));
            gameObject.SetActive(false);
        }
        private void Clear()
        {
            foreach(Transform child in content) { child.gameObject.SetActive(false); Destroy(child.gameObject); }
        }
        private Text Label(string name,string key,float low,float high,int size)
        {
            var label=PlaceholderVisuals.Label(name,content,font,"",size,IdentityStyle.Navy,new Vector2(.04f,low),new Vector2(.96f,high));
            label.resizeTextForBestFit=true;label.resizeTextMinSize=24;label.resizeTextMaxSize=size;
            LocalizedLabel.Bind(label,key);return label;
        }
        private void Button(string name,string key,Vector2 min,Vector2 max,Action action,bool primary=false)
        {
            var button=ChapterSelect.CreateButton(name,font,rounded,content,min,max,action,out var label);
            if(primary)button.GetComponent<Image>().color=new Color32(24,146,162,255);
            label.resizeTextForBestFit=true;label.resizeTextMinSize=24;label.resizeTextMaxSize=32;
            LocalizedLabel.Bind(label,key);button.GetComponent<PresentationMotion>().Settings=feel;
        }
        public void OpenIndex()
        {
            TakeInput();transform.SetAsLastSibling();gameObject.SetActive(true);Clear();
            LayoutCard(false);
            Label("Guide Title","guide.title",.88f,1,48).fontStyle=FontStyle.Bold;
            for(int i=0;i<MechanicGuideCatalog.Topics.Length;i++)
            {
                var topic=MechanicGuideCatalog.Topics[i];float y=.76f-i*.097f;
                Button("Guide "+topic,MechanicGuideCatalog.Key(topic)+".name",new Vector2(.04f,y),new Vector2(.96f,y+.08f),()=>OpenTopic(topic));
            }
            Button("Close Guide","common.back",new Vector2(.2f,.01f),new Vector2(.8f,.09f),close);
        }
        public void OpenTopic(MechanicTopic topic)
        {
            TakeInput();transform.SetAsLastSibling();gameObject.SetActive(true);Clear();state.Open(topic);
            LayoutCard(true);
            string key=MechanicGuideCatalog.Key(topic);
            Label("Mechanic Title",key+".name",.88f,1,44).fontStyle=FontStyle.Bold;
            var sample=PlaceholderVisuals.Rect("Generic Example",content,new Vector2(.04f,.61f),new Vector2(.96f,.86f));
            var art=sample.gameObject.AddComponent<BoardVisualResources>();
            var pieces=MechanicGuideCatalog.Example(topic);
            for(int i=0;i<pieces.Length;i++)
            {
                float x=(float)i/pieces.Length;
                var slot=PlaceholderVisuals.Rect("Example Slot",sample,new Vector2(x,0),new Vector2(x+1f/pieces.Length,1));
                var square=PlaceholderVisuals.Rect("Example Symbol",slot,new Vector2(.05f,.05f),new Vector2(.95f,.95f));
                var aspect=square.gameObject.AddComponent<AspectRatioFitter>();aspect.aspectMode=AspectRatioFitter.AspectMode.FitInParent;aspect.aspectRatio=1;
                var body=square.gameObject.AddComponent<Image>();
                _=new PieceAppearance(new BoardPiece(i,pieces[i]),body,font,circle,rounded,art);
            }
            foreach(var graphic in sample.GetComponentsInChildren<Graphic>())graphic.raycastTarget=false;
            Label("Example Caption",key+".example",.53f,.60f,28);
            var rules=Label("Mechanic Explanation",key+".body",.14f,.50f,32);
            rules.alignment=TextAnchor.UpperLeft;rules.lineSpacing=1.12f;
            Button("All Rules","guide.title",new Vector2(.01f,.01f),new Vector2(.49f,.10f),OpenIndex);
            Button("Dismiss Guide","guide.close",new Vector2(.51f,.01f),new Vector2(.99f,.10f),close,true);
        }
        private void LayoutCard(bool detail)
        {
            var body=(RectTransform)content;
            body.anchorMin=new Vector2(.1f,detail?.15f:.11f);
            body.anchorMax=new Vector2(.9f,detail?.90f:.89f);
        }
        public void Close() => gameObject.SetActive(false);
    }

    public sealed class MechanicNotice : MonoBehaviour
    {
        private Text title;
        private MechanicTopic topic;
        public bool IsVisible => gameObject.activeSelf;
        public void Build(Font font,Sprite rounded,GameFeelSettings feel,Action learn)
        {
            var image=gameObject.AddComponent<Image>();image.sprite=rounded;image.type=Image.Type.Sliced;image.color=IdentityStyle.Cream;IdentityStyle.Material(image);
            title=PlaceholderVisuals.Label("New Mechanic",transform,font,"",30,IdentityStyle.Navy,new Vector2(.03f,.51f),new Vector2(.97f,.97f));
            title.fontStyle=FontStyle.Bold;title.resizeTextForBestFit=true;title.resizeTextMinSize=24;title.resizeTextMaxSize=30;
            LocalizedLabel.Bind(title,()=>GameLanguageService.Shared.Text("guide.notice",GameLanguageService.Shared.Text(MechanicGuideCatalog.Key(topic)+".name")));
            var a=ChapterSelect.CreateButton("Learn Mechanic",font,rounded,transform,new Vector2(.04f,.06f),new Vector2(.49f,.49f),learn,out var learnLabel);
            a.GetComponent<Image>().color=new Color32(24,146,162,255);LocalizedLabel.Bind(learnLabel,"guide.learn");
            var b=ChapterSelect.CreateButton("Dismiss Mechanic",font,rounded,transform,new Vector2(.51f,.06f),new Vector2(.96f,.49f),Dismiss,out var dismissLabel);
            LocalizedLabel.Bind(dismissLabel,"guide.dismiss");
            foreach(var motion in GetComponentsInChildren<PresentationMotion>())motion.Settings=feel;
            gameObject.SetActive(false);
        }
        public void Show(MechanicTopic discovered) { topic=discovered;gameObject.SetActive(true);
            LocalizedLabel.Bind(title,()=>GameLanguageService.Shared.Text("guide.notice",GameLanguageService.Shared.Text(MechanicGuideCatalog.Key(topic)+".name"))); }
        public void Dismiss() => gameObject.SetActive(false);
    }
}
