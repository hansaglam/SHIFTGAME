using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using NUnit.Framework;
using Shift.Game.Editor;
using UnityEngine;
using Object=UnityEngine.Object;
namespace Shift.Game.Tests
{
    public sealed class CampaignCertificationTests
    {
        [TestCase(1,1,"bf6268d7821c416f68af9762934543a17a36d09ec182f1fbc5ee3d8e148cbfd8")]
        [TestCase(2,3,"acc4bdb364d90f586b03aa3e67d29c215544540aa8addf0886badf8475ca3e05")]
        [TestCase(3,3,"ffb47718ee466b8a82d7bb4f87933d24c34275e75186f4af10ca2d427bf94f7a")]
        [TestCase(4,1,"2be4e3c182901696e85365867dd603750f5bc849d02f011f24923e57566b3149")]
        [TestCase(5,2,"19b4d2d46834393893d57b93657423f33a2b67e17ba72a1cb88fa3481998f0af")]
        [TestCase(6,2,"247ac4f044e6809624b8ec5803ef1575ddeb6625376cc6cd7f18a5c7dc2b3f1f")]
        [TestCase(8,2,"d04d8f4c455fe1918d862f69c614cde4135ef02ac5fcd7d1a7dc154664e3d1a5")]
        [TestCase(11,4,"763bb645012b69660121e0332ac867ea90316865017bd0580387bd84b7e6be17")]
        [TestCase(12,3,"89a1642cf0bebc8877e6b4180c77894e29616db06f8c86e6fa5159a27ca3d4e7")]
        [TestCase(13,2,"22d6a9ec7c0ac43c4ead66f901a14f97deff5fd8d482c8076a8e9239f8689e0a")]
        [TestCase(14,4,"fbaf334e1ba7a7413397540a924e887cd88367d91af6a4d821d64bd03c3fca92")]
        [TestCase(16,5,"d97fb41ffb2a4a239f26460faafc73d8275c839f69069a0d859504fee14bdb13")]
        [TestCase(17,5,"ecf105273e754b75d4e42ac88cfc06e7731e7fdee9d960c1a999052d243588c7")]
        [TestCase(19,4,"cfebae29399926003b05ddc5df632d9f9eb62acc5b48ae86afd7183e527a91d8")]
        public void NewCertificatesHaveExhaustiveProofAndMatchingFingerprint(int number,int minimum,string fingerprint)
        {
            var level=LevelValidation.AllLevels()[number-1];var proof=CampaignCertification.Prove(level,number);
            Assert.That(proof.complete,Is.True,proof.error);Assert.That(proof.minimum,Is.EqualTo(minimum));
            Assert.That(proof.fingerprint,Is.EqualTo(fingerprint));Assert.That(level.VerifiedOptimalMoveCount,Is.EqualTo(minimum));
            Assert.That(proof.knownLength,Is.EqualTo(minimum));Assert.That(proof.shortest.Count,Is.EqualTo(minimum));
            Assert.That(proof.states,Is.LessThan(200000));Assert.That(proof.shorterNodes,Is.LessThanOrEqualTo(200000));
            Assert.That(LevelValidation.VerifySolution(level,out string error,out _),Is.True,error);
        }
        [Test] public void SearchCapOverflowCannotReturnACompletedProof()
        {
            var level=LevelValidation.AllLevels()[0];var proof=CampaignCertification.Prove(level,1,1);
            Assert.That(proof.complete,Is.False);Assert.That(proof.error,Does.Contain("bound reached"));
            Assert.That(proof.shortest,Is.Null);Assert.That(proof.minimum,Is.Zero);
            Assert.Throws<InvalidOperationException>(()=>new PuzzleBatchAnalysis(level,1).Analyze());
        }
        [Test] public void AllFortyCanEarnPerfectAndAllFortyFingerprintMutationsInvalidateCertificates()
        {
            var levels=LevelValidation.AllLevels();Assert.That(levels.Count(x=>x.VerifiedOptimalMoveCount.HasValue),Is.EqualTo(40));
            foreach(var level in levels)
            {
                var board=new BoardManager();board.Load(level);var telemetry=new TelemetryTracker(null,()=>0);
                telemetry.StartLevel(level,Array.IndexOf(levels,level),level.VerifiedOptimalMoveCount);
                foreach(var tap in level.KnownSolution){Assert.That(board.RequestMove(tap),Is.True);telemetry.AcceptedTap(board.ReactionDepth,board.MovesRemaining,board.LastReactionWasCancelled);board.CompleteResolution();}
                Assert.That(board.State,Is.EqualTo(GameState.Won),level.name);telemetry.Finish(true);Assert.That(telemetry.Result.perfectShift,Is.True,level.name);
                var clone=Object.Instantiate(level);
                try
                {
                    Assert.That(VerifiedOptimality.Fingerprint(clone),Is.EqualTo(VerifiedOptimality.Fingerprint(level)));
                    Assert.That(clone.VerifiedOptimalMoveCount,Is.EqualTo(level.VerifiedOptimalMoveCount));
                    clone.Configure(level.Width,level.Height,level.MoveLimit+100,level.TargetColor,level.Placements);clone.ConfigureTargetColors(level.ResolvedTargetColors);
                    Assert.That(clone.VerifiedOptimalMoveCount,Is.Null,level.name);
                }
                finally{Object.DestroyImmediate(clone);}
            }
        }
        [Test] public void OldPerfectFlagsAndCompletedProgressSurviveWithoutRetroactiveAwards()
        {
            const string prefix="SHIFT.Tests.Certification40.";var saves=new SaveService(prefix);saves.Reset();
            try
            {
                saves.Save(new ProgressData(39,0,39));var flags=Enumerable.Repeat('0',40).ToArray();
                foreach(int i in new[]{6,8,9,14,17,19,20,39})flags[i]='1';
                PlayerPrefs.SetString(prefix+"Mastery.v1.Perfect",new string(flags));
                var levels=LevelValidation.AllLevels();var progress=new LevelProgression(40,saves,levels);
                Assert.That(progress.HighestCompleted,Is.EqualTo(39));Assert.That(progress.HighestUnlocked,Is.EqualTo(39));Assert.That(progress.Current,Is.Zero);
                for(int i=0;i<40;i++)Assert.That(progress.IsPerfect(i),Is.EqualTo(flags[i]=='1'));
                foreach(int n in CampaignCertification.Missing)Assert.That(progress.Mastery(n-1),Is.EqualTo(LevelMasteryState.Completed));
                Assert.That(progress.ChapterSummary(0).Perfect,Is.EqualTo(6));Assert.That(progress.ChapterSummary(0).Eligible,Is.EqualTo(20));Assert.That(progress.ChapterSummary(0).Mastered,Is.False);
                var b=new BoardManager();b.Load(levels[0]);var t=new TelemetryTracker(null,()=>0);t.StartLevel(levels[0],0,levels[0].VerifiedOptimalMoveCount);
                foreach(var tap in levels[0].KnownSolution){b.RequestMove(tap);t.AcceptedTap(b.ReactionDepth,b.MovesRemaining,false);b.CompleteResolution();}
                t.Finish(b.State==GameState.Won);progress.Complete(0);Assert.That(progress.RecordPerfect(0,t.Result),Is.True);
                progress=new LevelProgression(40,saves,levels);Assert.That(progress.IsPerfect(0),Is.True);Assert.That(progress.IsPerfect(39),Is.True);Assert.That(progress.ChapterSummary(0).Perfect,Is.EqualTo(7));
            }
            finally{saves.Reset();}
        }
        [Test] public void AllPuzzleAssetsAndGameplaySourcesAreByteIdenticalTo306Baseline()
        {
            using var sha=SHA256.Create();
            foreach(var line in File.ReadAllLines(Path.Combine(Application.dataPath,"_Game/Tests/Editor/CertificationProtectedFiles.txt")))
            {var p=line.Split('|');Assert.That(BitConverter.ToString(sha.ComputeHash(File.ReadAllBytes(Path.Combine(Application.dataPath,p[0])))).Replace("-",""),Is.EqualTo(p[1]),p[0]);}
        }
    }
}
