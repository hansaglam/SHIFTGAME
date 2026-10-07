using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Shift.Game.Editor;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object = UnityEngine.Object;
namespace Shift.Game.Tests
{
 public sealed class AndroidPrebuildFinalTests
 {
  private const string Prefix="SHIFT.Tests.AndroidPrebuild.";
  private GameObject root;
  private sealed class Privacy : IPrivacyChoices,IRewardedAdService
  {
   public bool Required,Error;public int Shows,Ads;
   public bool IsPrivacyOptionsRequired=>Error?throw new InvalidOperationException():Required;
   public bool IsBusy=>false;public bool IsRewardedAdAvailable=>false;
   public void ShowPrivacyOptions(Action<bool> done){Shows++;done(true);}
   public void ShowRewardedAd(RewardReason reason,Action<bool> done){Ads++;done(false);}
  }
  [TearDown]public void Cleanup(){if(root!=null)Object.DestroyImmediate(root);new SettingsService(Prefix).Reset();}
  [Test]public void AndroidIdentityIsFinal()
  {
   Assert.That(PlayerSettings.GetApplicationIdentifier(NamedBuildTarget.Android),Is.EqualTo("com.ethemsincar.shift"));
   Assert.That(PlayerSettings.companyName,Is.EqualTo("Ethem Sincar"));Assert.That(PlayerSettings.productName,Is.EqualTo("SHIFT"));
  }
  [Test]public void ExplicitTargetAndProtectedAndroidCapabilities()
  {
   Assert.That((int)PlayerSettings.Android.targetSdkVersion,Is.EqualTo(36));Assert.That((int)PlayerSettings.Android.minSdkVersion,Is.EqualTo(25));
   Assert.That(PlayerSettings.GetScriptingBackend(NamedBuildTarget.Android),Is.EqualTo(ScriptingImplementation.IL2CPP));
   Assert.That(PlayerSettings.Android.targetArchitectures,Is.EqualTo(AndroidArchitecture.ARM64));
   Assert.That(PlayerSettings.defaultInterfaceOrientation,Is.EqualTo(UIOrientation.Portrait));Assert.That(PlayerSettings.Android.useCustomKeystore,Is.False);
  }
  private static object RequestConfiguration() => typeof(GoogleRewardedAdClient).GetMethod("CreateRequestConfiguration").Invoke(null,null);
  private static object Member(object instance,string name)
  {var type=instance.GetType();return type.GetProperty(name)?.GetValue(instance)??type.GetField(name)?.GetValue(instance);}
  [Test]public void ProductionRequestConfigurationCapsContentAtT()
  {
   var rating=Member(RequestConfiguration(),"MaxAdContentRating");Assert.That(rating,Is.Not.Null);
   var type=rating.GetType();var expected=type.GetField("T",BindingFlags.Public|BindingFlags.Static)?.GetValue(null)??type.GetProperty("T",BindingFlags.Public|BindingFlags.Static)?.GetValue(null);
   Assert.That(expected,Is.Not.Null);Assert.That(Member(rating,"Value"),Is.EqualTo(Member(expected,"Value")));Assert.That(Member(rating,"Value"),Is.EqualTo("T"));
  }
  [Test]public void RequestConfigurationKeepsAgeUnspecifiedAndNoTestDevices()
  {
   var config=RequestConfiguration();var defaults=Activator.CreateInstance(config.GetType());
   foreach(var name in new[]{"AgeRestrictedTreatment","TagForChildDirectedTreatment","TagForUnderAgeOfConsent"})
   {
    var type=config.GetType();var property=type.GetProperty(name);var field=type.GetField(name);
    object Value(object instance)=>property!=null?property.GetValue(instance):field?.GetValue(instance);
    Assert.That(Value(config),Is.EqualTo(Value(defaults)),name);
    Assert.That(Value(config)?.ToString(),Is.Not.EqualTo("True"),name);
   }
   Assert.That(Member(config,"TestDeviceIds") as System.Collections.ICollection,Is.Empty);
  }
  [TestCase("Required")][TestCase("NotRequired")][TestCase("Unknown")][TestCase("Error")][TestCase("Unavailable")]
  public void PolicyAlwaysVisibleAndInvokesExactUrlIndependently(string state)
  {
   root=new GameObject("Settings",typeof(RectTransform));var panel=root.AddComponent<SettingsPanel>();
   var provider=state=="Unavailable"?null:new Privacy{Required=state=="Required",Error=state=="Error"};string url=null;int calls=0;
   panel.Build(Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"),null,new SettingsService(Prefix),new GameFeelSettings(),null,()=>{},()=>provider,value=>{calls++;url=value;});panel.Open();
   var button=panel.transform.Find("Privacy Policy").GetComponent<Button>();Assert.That(button.gameObject.activeSelf,Is.True);Assert.That(button.interactable,Is.True);
   button.onClick.Invoke();Assert.That(calls,Is.EqualTo(1));Assert.That(url,Is.EqualTo("https://hansaglam.github.io/shift-legal/privacy-policy/"));
   Assert.That(panel.transform.Find("Privacy Choices").gameObject.activeSelf,Is.EqualTo(state=="Required"));
   if(provider!=null){Assert.That(provider.Shows,Is.Zero);Assert.That(provider.Ads,Is.Zero);}
  }
  [TestCase(GameLanguage.English,"Privacy Policy")][TestCase(GameLanguage.Turkish,"Gizlilik Politikası")]
  public void PolicyLabelsAreLocalized(GameLanguage language,string expected)
  {Assert.That(LocalizationCatalog.Format("settings.privacy_policy",language),Is.EqualTo(expected));}
  [UnityTest]public IEnumerator SettingsLanguagesAndConsentStatesFitAndCapture()
  {
   EditorSceneManager.OpenScene(PrototypeSetup.ScenePath);ProgressionPresentationTests.IsolateSave(Prefix);
   yield return new EnterPlayMode();yield return new WaitForSecondsRealtime(.35f);
   var game=Object.FindFirstObjectByType<PrototypeGame>();var provider=new Privacy();game.RewardedAds=provider;
   Directory.CreateDirectory(Path.Combine(Application.dataPath,"../Validation/android-prebuild-final"));
   int n=0;
   foreach(var language in new[]{HintLanguage.English,HintLanguage.Turkish})foreach(bool required in new[]{false,true})
   {
    provider.Required=required;game.DailyLanguage=language;game.OpenSettings();yield return new WaitForSecondsRealtime(.15f);Canvas.ForceUpdateCanvases();
    var panel=Object.FindFirstObjectByType<SettingsPanel>();var buttons=panel.GetComponentsInChildren<Button>();Assert.That(buttons.Length,Is.EqualTo(required?7:6));
    Assert.That(panel.transform.Find("Privacy Policy").GetComponentInChildren<Text>().text,Is.EqualTo(language==HintLanguage.English?"Privacy Policy":"Gizlilik Politikası"));
    var rects=buttons.Select(b=>(RectTransform)b.transform).OrderBy(r=>r.anchorMin.y).ToArray();
    for(int i=0;i<rects.Length;i++)
    {
     Assert.That(rects[i].anchorMin.y,Is.GreaterThanOrEqualTo(.12f));Assert.That(rects[i].anchorMax.y,Is.LessThan(.78f));
     if(i>0)Assert.That(rects[i-1].anchorMax.y,Is.LessThan(rects[i].anchorMin.y),"Settings rows overlap");
    }
    SprintPresentationTests.Capture("android-prebuild-final/"+(++n).ToString("00")+"-"+(language==HintLanguage.English?"en":"tr")+"-"+(required?"required":"not-required")+".png");game.CloseSettings();
   }
   Assert.That(provider.Shows,Is.Zero);Assert.That(provider.Ads,Is.Zero);yield return new ExitPlayMode();
  }
 }
}
