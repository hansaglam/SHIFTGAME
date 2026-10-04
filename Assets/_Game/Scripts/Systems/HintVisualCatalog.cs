using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
namespace Shift.Game
{
    public sealed class HintVisualTarget
    {
        public readonly int Stage;
        public readonly GridPosition[] Cells;
        public readonly bool ExactMove;
        public bool Relationship => Stage == 2 && Cells.Length > 1;
        public HintVisualTarget(int stage, GridPosition[] cells, bool exactMove = false)
        { Stage = stage; Cells = cells; ExactMove = exactMove; }
    }
    // Authored presentation metadata; IDs refer to immutable placement order, never UI coordinates.
    public static class HintVisualCatalog
    {
        public sealed class Entry
        {
            public readonly string Fingerprint;
            private readonly int[][] stages;
            public Entry(string fingerprint, int[] attention, int[] relationship, int[] conceptual)
            { Fingerprint = fingerprint; stages = new[]{attention, relationship, conceptual}; }
            public IReadOnlyList<int> Targets(int stage) => stages[stage - 1];
        }
        private static readonly Dictionary<string, Entry> entries = new Dictionary<string, Entry> {
            { "Level01_FirstTap", new Entry("1757c1ec2375691885502a70b3f6d64c1a72675c275f635fef5e7da7db2a1ec4", new[]{0}, new[]{0,1}, new[]{0}) },
            { "Level02_FindTheExit", new Entry("1ab92d75cc598ee24ce9d970fe4c94a31cbd4bdc2a93c787bb33e895cb3fa59d", new[]{0}, new[]{0,1}, new[]{0}) },
            { "Level03_ClearTheBox", new Entry("fa0d80e3465f4177e6e7231536e7ccdf5cb99d4d595cfcb2afe8a969662b4ce3", new[]{1}, new[]{0,1}, new[]{1,2}) },
            { "Level04_FirstChain", new Entry("7a0286c97ee93f6c6c0ea629f6c8489baf7adeabd3246202fc626cdc07e1cc5c", new[]{1}, new[]{0,1,2}, new[]{0,2}) },
            { "Level05_BlockedIsFree", new Entry("972c405c502007bdb21fa998f9e835eba16f2988fefe7b673ece08b8972761f2", new[]{0}, new[]{2,3}, new[]{2}) },
            { "Level06_TurnTheCorner", new Entry("4c9b45cf3aed413eefb7896b439efbf784911b66ef017882b82ea8b79e6f607e", new[]{1}, new[]{0,1}, new[]{1}) },
            { "Level07_MakeRoom", new Entry("1a31aab3345b59d4ad5d3ddbb5b7dbc09d839c0ada1d3dac18a12eecbf64efef", new[]{2}, new[]{0,1}, new[]{1}) },
            { "Level08_Clockwise", new Entry("bed5a9b554d81f1cec96556e51a5490845561c777ba7576dfa3f11558d015068", new[]{1}, new[]{0,1}, new[]{1}) },
            { "Level09_PushAndTurn", new Entry("403a5f4fa290651c4bba8ddcbe2ee9ced0d462639b7840110465a25a99fc9193", new[]{2}, new[]{1,4}, new[]{2,4}) },
            { "Level10_SetTheChain", new Entry("65e4f1f4b8356972b308378e179dda27003446bdb5ebc0e22bb55d717b779a18", new[]{1}, new[]{2,3}, new[]{3,4}) },
            { "Level11_OpenTheLane", new Entry("7e7ec91af2624c6c45a41d6231bc38faadc9d9b3bc569148b9937f9d25b27543", new[]{1}, new[]{0,1}, new[]{1}) },
            { "Level12_CrossingOrder", new Entry("4b011d2c6622315a025c129a436dbac416c8acf5b8dd37395a07069383cd1423", new[]{1}, new[]{1,2}, new[]{1,2}) },
            { "Level13_SecondAct", new Entry("82406981d082efbbde169bae8f103a8c8ea3efa78f6d54782791a16549f826f2", new[]{1}, new[]{1,2}, new[]{2,3}) },
            { "Level14_TwoCorners", new Entry("3778b4e24d81d264f4c3ddb97c6eb0ee90f281cc867e996dfb647fd9000f335d", new[]{1}, new[]{1,2}, new[]{2,3}) },
            { "Level15_TheLongSetup", new Entry("ad680f856393695f24eae038fc93643a5c36db4ab6c7d1d6220a3643dc093f52", new[]{1}, new[]{2,4}, new[]{4,5}) },
            { "Level16_TransformAhead", new Entry("52ea982e180b908d8012035a4523b5ff032f4c128d3e71e100879962acd90732", new[]{1}, new[]{1,3}, new[]{3,4}) },
            { "Level17_SharedCorridor", new Entry("40d7f5c5e3a5ed99e0d132661894aad237925d0443f015a391185e8388c6e2b4", new[]{1}, new[]{1,2}, new[]{1}) },
            { "Level18_ClearTriggerFinish", new Entry("e60a9c792e65103b578e6b741e84478034c8f5c71d110a789eb5838a893b4265", new[]{5}, new[]{5,6}, new[]{4,5}) },
            { "Level19_SharedDelivery", new Entry("8b3a28f36cb480fb0d2423741c8d0788cdfc7ab4607e89d766b9fa2fc0f2591b", new[]{1}, new[]{2,3}, new[]{5,6}) },
            { "Level20_TheFinalShift", new Entry("77b05bdc70511369f8ac98f1115c93ea344cfc7d5dcadaebb1407767c490fab5", new[]{4}, new[]{4,7}, new[]{5,6}) },
            { "Level21_FirstSwitch", new Entry("fb2ec0c7b716fb8a6a203a22073ac7c286d0c87a2bbae0283c0d33d8f693d26f", new[]{1}, new[]{5,1}, new[]{5,1}) },
            { "Level22_OpenForRed", new Entry("7a16a427ba79f9fede0daadb715499651a6b30283a6e03f7aba44968aa1e0e20", new[]{1}, new[]{2,4}, new[]{1,2}) },
            { "Level23_ToggleTwice", new Entry("c9b008457d917db8fc30a6bdf503116e3dd09cf0cdc58a47b009832f1a54e5c7", new[]{1}, new[]{1,3}, new[]{6,7}) },
            { "Level24_ClosedLane", new Entry("23ec0d38f5ec97c5ba4b58f53caaeed7655b909eb608be69f1ef81701ad1af09", new[]{2}, new[]{3,4}, new[]{0,1}) },
            { "Level25_BeforeYouGo", new Entry("28ae6ed2eba436fa791e72a5e2c33f0b73307d2ab83f560aa2f83e3cfecba5d0", new[]{0}, new[]{4,10}, new[]{3,10}) },
            { "Level26_ThreeEntries", new Entry("80a313f11d5ebecfc5fc0ffadc5278b00b1334ab7df4d30574ef8f776e8d1597", new[]{1}, new[]{2,3}, new[]{1,2}) },
            { "Level27_BoxDoesNotPress", new Entry("4cb5ec45172e005fa6e5e64d5da87f677a1f1e66c4e4a5224b2bbe8bf02a8e2e", new[]{1}, new[]{1,4}, new[]{1,5}) },
            { "Level28_TurnThrough", new Entry("6eafec5835a24629ae6652531bf308715aca4a91dfea9e9253853674f8221794", new[]{3}, new[]{0,3}, new[]{5,8}) },
            { "Level29_ClockworkGate", new Entry("7f0761fa9ba43dda7b8d36f255145885646872fff2d28b3d7f5e0fd4323b66ee", new[]{0}, new[]{0,1,2}, new[]{0,1}) },
            { "Level30_ClearThenOpen", new Entry("08e0fd6385e0e4b6048a6e6dcf7ed706275f9a2a0ccd797cf8f330b3e8794eb2", new[]{4}, new[]{4,6}, new[]{2,6}) },
            { "Level31_OnePadTwoGates", new Entry("c9ef730422da1f301484be84f004b6e1d6fd4468bc2a05253d1f51f1104b4886", new[]{2}, new[]{2,1}, new[]{3,1}) },
            { "Level32_OppositeStates", new Entry("5e52f465d99e9fabdce6885d9cc5bdcecc81b92e87ab3e55abc5aca183ae71ad", new[]{1}, new[]{1,4}, new[]{7,4}) },
            { "Level33_SharedDeliveryGate", new Entry("a8b2240fcd79bc95151a33b8ca89a7458c0f97e16e21dd24ce52ba0cc7fe5278", new[]{0}, new[]{0,1}, new[]{0,1}) },
            { "Level34_ClearTheCorridor", new Entry("705ba8d16c1644ba6b0f7a0cc09686cb4dae951f0e5800e31a174e6e990a7968", new[]{3}, new[]{0,3}, new[]{5,7}) },
            { "Level35_TwoChannels", new Entry("e1c99f3641c4f4a462f20836d64272e107fbd18047fda338d22c4170271d1dbc", new[]{4}, new[]{4,6}, new[]{4,6}) },
            { "Level36_CrossBeforeClosing", new Entry("b7d2012367c012e24b860242eba52f74cdd39c95c1c280dd68ba41698af397d7", new[]{4}, new[]{0,4,1}, new[]{3,4}) },
            { "Level37_OpenTheSecondPad", new Entry("8664c13f5b87c6d1b52a6cb98849b8cc9d7ea6ebae7debf9872332c121685d52", new[]{0}, new[]{0,1}, new[]{0,5}) },
            { "Level38_LiftAndTurn", new Entry("d44c8b17480a46c7571edb0bd18ce666fce308f926185fbd48d3354b0ab0db6e", new[]{3}, new[]{3,5}, new[]{3,2}) },
            { "Level39_PrepareToggleRedirect", new Entry("4bafaba5eacaa08bdbcb1d4215f4132c81bd5192c7d88ef08a5e4dfbb10ca9ee", new[]{0}, new[]{0,1}, new[]{0,3}) },
            { "Level40_TheStateOfShift", new Entry("e9d16f5e3c59a4b509922658ce60a699eeda217bbd14c6dba0eb4ead45ad0218", new[]{13}, new[]{13,15}, new[]{1,9}) },
        };
        public static Entry Find(LevelData level) => level != null && entries.TryGetValue(level.name, out var e) && e.Fingerprint == Fingerprint(level) ? e : null;
        public static string Fingerprint(LevelData level)
        {
            var s = new StringBuilder(); s.Append(level.Width).Append(',').Append(level.Height).Append(',').Append(level.MoveLimit).Append(';');
            foreach (var c in level.ResolvedTargetColors) s.Append((int)c).Append(','); s.Append(';');
            foreach (var p in level.Placements) s.Append((int)p.type).Append(',').Append((int)p.color).Append(',').Append((int)p.direction).Append(',').Append(p.position.x).Append(',').Append(p.position.y).Append(',').Append(p.channel).Append(',').Append(p.initialOpen ? 1 : 0).Append(';');
            using var sha = SHA256.Create(); return BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(s.ToString()))).Replace("-", "").ToLowerInvariant();
        }
    }
}
