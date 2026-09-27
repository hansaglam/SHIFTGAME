using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using NUnit.Framework;
using Shift.Game.Editor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Shift.Game.Tests
{
    public sealed class HintContentTests
    {
        private static IEnumerable<int> Campaign => Enumerable.Range(1,40);
        [TestCaseSource(nameof(Campaign))]
        public void EachCampaignLevelHasThreeBilingualDistinctNonLeakingStages(int number)
        {
            var level=LevelValidation.AllLevels()[number-1];
            foreach(HintLanguage language in Enum.GetValues(typeof(HintLanguage)))
            {
                Assert.That(HintCatalog.AuthoredCount(level,language),Is.EqualTo(3),level.name);
                var stages=Enumerable.Range(1,3).Select(i=>HintCatalog.Get(level,i,language)).ToArray();
                Assert.That(stages.Distinct(StringComparer.OrdinalIgnoreCase).Count(),Is.EqualTo(3));
                foreach(var text in stages)
                {
                    Assert.That(string.IsNullOrWhiteSpace(text),Is.False);
                    Assert.That(text.Length,Is.LessThanOrEqualTo(110),text);
                    Assert.That(Regex.IsMatch(text,@"\d|\b[A-H][1-8]\b|\b(?:row|column)\s+\d",RegexOptions.IgnoreCase),Is.False,text);
                    Assert.That(Regex.IsMatch(text,@"BoardManager|LevelData|targetColor|reaction depth|snapshot|solver|fingerprint|koordinat",RegexOptions.IgnoreCase),Is.False,text);
                    Assert.That(Regex.IsMatch(text,@"(?:tap|click|press|dokun|tıkla).*(?:then|next|sonra|ardından).*(?:tap|click|press|dokun|tıkla)|(?:Red|Blue|Yellow|Green)\s*(?:,|→|->).*?(?:Red|Blue|Yellow|Green)\s*(?:,|→|->)",RegexOptions.IgnoreCase),Is.False,text);
                    Assert.That(text.IndexOf((char)0xFFFD),Is.EqualTo(-1),"Invalid replacement character: "+text);
                    // Mechanic and color references must exist in the actual puzzle, not just its old filename.
                    foreach(var mechanic in new[]{(PieceType.Gate,@"\bgate\b|\bgates\b|kapı"),(PieceType.Switch,@"\bswitch\b|düğme"),(PieceType.Rotator,@"\brotator\b|döndürücü"),(PieceType.PushBlock,@"\bbox\b|kutu")})
                        if(Regex.IsMatch(text,mechanic.Item2,RegexOptions.IgnoreCase))Assert.That(level.Placements.Any(p=>p.type==mechanic.Item1),Is.True,level.name+": "+text);
                    foreach(var color in new[]{(PieceColor.Red,@"\bred\b|\breds\b|kırmızı"),(PieceColor.Blue,@"\bblue\b|mavi"),(PieceColor.Yellow,@"\byellow\b|sarı"),(PieceColor.Green,@"\bgreen\b|yeşil")})
                        if(Regex.IsMatch(text,color.Item2,RegexOptions.IgnoreCase))Assert.That(level.Placements.Any(p=>p.type==PieceType.Normal&&p.color==color.Item1),Is.True,level.name+": "+text);
                }
                Assert.That(HintCatalog.Get(level,int.MinValue,language),Is.EqualTo(stages[0]));
                Assert.That(HintCatalog.Get(level,int.MaxValue,language),Is.EqualTo(stages[2]));
            }
            Assert.That(HintCatalog.Get(level,1),Is.EqualTo(HintCatalog.Get(level,1,HintLanguage.English)));
            Assert.That(LevelValidation.VerifySolution(level,out string error,out _),Is.True,error);
        }
        [TestCase(36,"turning point","dönüş noktası","Both colors","İki rengin")]
        [TestCase(37,"share a bay","aynı alanı","Red and Blue","Kırmızı ve Mavi")]
        [TestCase(38,"help before","çıkmadan önce yardım","other color","diğer rengin")]
        [TestCase(39,"same upward push","aynı yukarı itiş","Red and Blue","Kırmızı ve Mavi")]
        [TestCase(40,"different endings","farklı bitiş","one color leaves","Bir renk çıktıktan")]
        public void MultiColorCopyPreservesTheActualSharedDependency(int number,string dependencyEn,string dependencyTr,string objectiveEn,string objectiveTr)
        {
            var l=LevelValidation.AllLevels()[number-1];
            Assert.That(l.ResolvedTargetColors,Is.EqualTo(number==38?new[]{PieceColor.Red,PieceColor.Yellow}:new[]{PieceColor.Red,PieceColor.Blue}));
            string en=string.Join(" ",Enumerable.Range(1,3).Select(i=>HintCatalog.Get(l,i,HintLanguage.English)));
            string tr=string.Join(" ",Enumerable.Range(1,3).Select(i=>HintCatalog.Get(l,i,HintLanguage.Turkish)));
            Assert.That(en,Does.Contain(dependencyEn));Assert.That(en,Does.Contain(objectiveEn));
            Assert.That(tr,Does.Contain(dependencyTr));Assert.That(tr,Does.Contain(objectiveTr));
        }
        [Test] public void ExistingRepresentativeEnglishCopyIsPreservedExactly()
        {
            var levels=LevelValidation.AllLevels();
            foreach(var line in File.ReadAllLines(Path.Combine(Application.dataPath,"_Game/Tests/Editor/HintLegacyCopy.txt")))
            {var p=line.Split('|');Assert.That(HintCatalog.Get(levels.Single(l=>l.name==p[0]),int.Parse(p[1])),Is.EqualTo(p[2]));}
        }
        [Test] public void MissingAndNullMetadataKeepFallbackAndNeverConsumeInvalidHints()
        {
            var fixture=Object.Instantiate(LevelValidation.AllLevels()[0]);fixture.name="UncataloguedFixture";
            try
            {
                Assert.That(HintCatalog.AuthoredCount(fixture),Is.Zero);
                Assert.That(HintCatalog.Get(fixture,2),Is.EqualTo(fixture.Hint));
                foreach(HintLanguage language in Enum.GetValues(typeof(HintLanguage)))
                {
                    Assert.That(HintCatalog.AuthoredCount(null,language),Is.Zero);
                    Assert.That(HintCatalog.Get(null,1,language),Is.Not.Empty);
                    Assert.That(HintCatalog.Get(fixture,3,language),Is.Not.Empty);
                }
                var session=new HintSession();Assert.That(session.TryUse(null,out _),Is.False);Assert.That(session.Remaining,Is.EqualTo(3));
                Assert.That(HintCatalog.Get(fixture,1,(HintLanguage)999),Is.EqualTo(fixture.Hint));
            }
            finally{Object.DestroyImmediate(fixture);}
        }
        [Test] public void CatalogLanguageReadsDoNotChangeSessionAllowanceOrProgress()
        {
            var l=LevelValidation.AllLevels()[26];var session=new HintSession();
            for(int stage=1;stage<=3;stage++)
            {
                Assert.That(session.TryUse(l,out var text),Is.True);Assert.That(text,Is.EqualTo(HintCatalog.Get(l,stage)));
                Assert.That(HintCatalog.Get(l,stage,HintLanguage.Turkish),Is.Not.EqualTo(text));
                Assert.That(session.Stage(l),Is.EqualTo(stage));Assert.That(session.Remaining,Is.EqualTo(3-stage));
            }
            Assert.That(session.TryUse(l,out _),Is.False);session.GrantReward();
            Assert.That(session.TryUse(l,out var repeated),Is.False);Assert.That(repeated,Is.Null);
            Assert.That(session.Remaining,Is.EqualTo(1));Assert.That(session.Stage(l),Is.EqualTo(3));
        }
        [Test] public void AllGameplayUiUndoRewardsTelemetryAssetsAndCertificatesRemainByteIdentical()
        {
            using var sha=SHA256.Create();
            foreach(var line in File.ReadAllLines(Path.Combine(Application.dataPath,"_Game/Tests/Editor/HintContentProtectedFiles.txt")))
            {
                var p=line.Split('|');string hash=BitConverter.ToString(sha.ComputeHash(File.ReadAllBytes(Path.Combine(Application.dataPath,p[0])))).Replace("-","");
                Assert.That(hash,Is.EqualTo(p[1]),p[0]);
            }
        }
    }
}
