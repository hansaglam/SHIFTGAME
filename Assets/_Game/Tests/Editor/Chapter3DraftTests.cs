using NUnit.Framework;
using Shift.Game.Editor;

namespace Shift.Game.Tests
{
    public sealed class Chapter3DraftTests
    {
        [Test]
        public void AllTenDraftLevelsHaveValidAndOptimalRecordedSolutions()
        {
            for (int n = 41; n <= 50; n++)
            {
                var level = Chapter3DraftAudit.Get(n);
                Assert.That(level, Is.Not.Null, "Missing Level " + n);
                Assert.That(level.Width, Is.InRange(4, 6));
                Assert.That(level.Height, Is.InRange(4, 6));
                Assert.That(LevelValidation.VerifySolution(level, out string error, out int depth),
                    Is.True, level.name + ": " + error);
                Assert.That(level.KnownSolutionLength, Is.GreaterThanOrEqualTo(5));
                Assert.That(depth, Is.GreaterThanOrEqualTo(4));
                var proof = CampaignCertification.Prove(level, n);
                Assert.That(proof.complete, Is.True, level.name + ": " + proof.error);
                Assert.That(proof.minimum, Is.EqualTo(level.KnownSolutionLength),
                    level.name + " has a shorter solution than the recorded path");
            }
        }
    }
}
