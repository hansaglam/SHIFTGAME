using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
namespace Shift.Game.Editor
{
    public static class CampaignCertification
    {
        public static readonly int[] Missing = {1,2,3,4,5,6,8,11,12,13,14,16,17,19};
        [Serializable] public sealed class Proof
        {
            public int number, knownLength, minimum, states, shorterNodes;
            public string level, fingerprint, error;
            public bool complete;
            public List<GridPosition> shortest;
        }
        [Serializable] public sealed class Report { public List<Proof> proofs = new List<Proof>(); }
        public static Proof Prove(LevelData level, int number, int cap = 200000)
        {
            var proof = new Proof { number=number, level=level.name, knownLength=level.KnownSolution.Count, fingerprint=VerifiedOptimality.Fingerprint(level) };
            try
            {
                var result = new PuzzleBatchAnalysis(level, cap).Analyze();
                if (result.minimum < 1) throw new InvalidOperationException("No positive-length solution proven.");
                int nodes=0;
                if (DesignValidation.CanWin(level,new List<GridPosition>(),result.minimum-1,ref nodes)) throw new InvalidOperationException("Shorter solution found by independent bounded replay.");
                if (VerifiedOptimality.Fingerprint(level)!=proof.fingerprint) throw new InvalidOperationException("Puzzle changed during proof.");
                proof.minimum=result.minimum;proof.states=result.states;proof.shorterNodes=nodes;proof.shortest=result.shortest;proof.complete=true;
            }
            catch (Exception e) { proof.error=e.Message;proof.complete=false; }
            return proof;
        }
        public static void Export()
        {
            var report=new Report();var levels=LevelValidation.AllLevels();
            foreach (int number in Missing) report.proofs.Add(Prove(levels[number-1],number));
            var directory=Path.Combine(Application.dataPath,"../Validation/certification40");Directory.CreateDirectory(directory);
            File.WriteAllText(Path.Combine(directory,"proofs.json"),JsonUtility.ToJson(report,true));
            Debug.Log("Campaign certification proof export complete.");
        }
    }
}
