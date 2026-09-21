using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Shift.Game.Editor
{
    // Exact finite-horizon enumeration. Every transition is replayed by the authoritative model.
    // A valid action spends a move; blocked/cancelled taps are excluded, never counted as choices.
    public sealed class PuzzleBatchAnalysis
    {
        [Serializable] public sealed class Outcome
        {
            public int minimum = -1, states, meaningfulDecisions, efficiencyDecisions, finalDepth, maxDepth, routeMaxDepth;
            public long solutionPaths;
            public double randomSolveProbability;
            public double optimalDiscoveryProbability;
            public List<Opening> openings = new List<Opening>();
            public List<Decision> decisions = new List<Decision>();
            public List<GridPosition> shortest = new List<GridPosition>();
        }
        [Serializable] public sealed class Opening
        { public GridPosition tap; public long winningPaths; public double probability, optimalProbability; public int depth, minimumRemaining; }
        [Serializable] public sealed class Decision
        { public int step, legal, viable, optimal; public GridPosition intended; }
        private sealed class Node
        {
            public int minimum = -1;
            public long paths;
            public double probability, optimalProbability;
            public List<GridPosition> shortest = new List<GridPosition>();
            public List<Opening> edges = new List<Opening>();
        }
        private readonly LevelData level;
        private readonly int limit;
        private readonly Dictionary<string, Node> cache = new Dictionary<string, Node>();
        private int maxDepth;
        public PuzzleBatchAnalysis(LevelData level, int nodeLimit = 200000) { this.level=level;limit=nodeLimit; }
        public Outcome Analyze(IReadOnlyList<GridPosition> route = null)
        {
            var root=Visit(new List<GridPosition>());
            var result=new Outcome {minimum=root.minimum,solutionPaths=root.paths,randomSolveProbability=root.probability,
                openings=root.edges,shortest=root.shortest,optimalDiscoveryProbability=root.optimalProbability};
            var path=new List<GridPosition>();
            foreach(var tap in route ?? root.shortest)
            {
                var node=Visit(path); int viable=node.edges.Count(e=>e.winningPaths>0);
                int optimal=node.edges.Count(e=>e.minimumRemaining>=0 && e.minimumRemaining+1==node.minimum);
                result.decisions.Add(new Decision {step=path.Count+1,legal=node.edges.Count,viable=viable,optimal=optimal,intended=tap});
                if(node.edges.Where(e=>e.minimumRemaining>=0).Select(e=>e.minimumRemaining).Distinct().Count()>1) result.efficiencyDecisions++;
                // Conservative decision density: at least one viable and one budget-losing committed action.
                if(viable>0 && viable<node.edges.Count) result.meaningfulDecisions++;
                if(!node.edges.Any(e=>e.tap==tap)) throw new InvalidOperationException("Route contains a non-committing tap.");
                path.Add(tap);
                result.routeMaxDepth=Math.Max(result.routeMaxDepth,Replay(path).ReactionDepth);
            }
            var final=Replay(path);
            if(root.minimum>=0 && final.State!=GameState.Won) throw new InvalidOperationException("Analyzed route is not a solution.");
            result.finalDepth=final.ReactionDepth; result.states=cache.Count; result.maxDepth=maxDepth; return result;
        }
        public long WinningContinuations(IEnumerable<GridPosition> prefix) => Visit(new List<GridPosition>(prefix)).paths;
        private BoardManager Replay(List<GridPosition> path)
        {
            var b=new BoardManager(); b.Load(level);
            foreach(var tap in path)
            {
                if(!b.RequestMove(tap) || b.ReactionDepth==0 || b.LastReactionWasCancelled)
                    throw new InvalidOperationException("Invalid search prefix: "+tap);
                b.CompleteResolution();
            }
            return b;
        }
        private Node Visit(List<GridPosition> path)
        {
            var b=Replay(path); var key=new StringBuilder().Append(b.MovesRemaining).Append('|');
            foreach(var p in b.Pieces) key.Append(p.Id).Append(':').Append(p.Active?1:0).Append(',').Append(p.Position.x).Append(',')
                .Append(p.Position.y).Append(',').Append((int)p.Direction).Append(',').Append(p.GateOpen?1:0).Append(';');
            if(cache.TryGetValue(key.ToString(),out var existing)) return existing;
            if(cache.Count>=limit) throw new InvalidOperationException("State bound reached; no optimum/probability certificate may be claimed.");
            var node=new Node(); cache.Add(key.ToString(),node);
            if(b.State==GameState.Won) { node.minimum=0;node.paths=1;node.probability=1;node.optimalProbability=1;return node; }
            if(b.State!=GameState.Playing) return node;
            foreach(var piece in b.Pieces)
            {
                if(!piece.Active || piece.Type!=PieceType.Normal) continue;
                var branch=Replay(path);var tap=piece.Position;
                if(!branch.RequestMove(tap) || branch.ReactionDepth==0 || branch.LastReactionWasCancelled) continue;
                int depth=branch.ReactionDepth;
                maxDepth=Math.Max(maxDepth,depth);
                path.Add(tap);var child=Visit(path);path.RemoveAt(path.Count-1);
                node.edges.Add(new Opening {tap=tap,winningPaths=child.paths,probability=child.probability,depth=depth,minimumRemaining=child.minimum,optimalProbability=child.optimalProbability});
                checked { node.paths+=child.paths; }
                node.probability+=child.probability;
                if(child.minimum>=0 && (node.minimum<0 || child.minimum+1<node.minimum))
                { node.minimum=child.minimum+1;node.shortest=new List<GridPosition>{tap};node.shortest.AddRange(child.shortest); }
            }
            if(node.edges.Count>0)
            {
                node.probability/=node.edges.Count;
                node.optimalProbability=node.edges.Where(e=>e.minimumRemaining>=0 && e.minimumRemaining+1==node.minimum).Sum(e=>e.optimalProbability)/node.edges.Count;
            }
            return node;
        }
    }
}
