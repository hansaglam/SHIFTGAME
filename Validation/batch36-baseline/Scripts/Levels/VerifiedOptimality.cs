using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace Shift.Game
{
    public static class VerifiedOptimality
    {
        // Proofs: SprintLevelTests, ChapterLevelTests and LevelDesignTests bounded exhaustive searches.
        // Fingerprints bind certificates to exact puzzle data, not names or recorded routes.
        public static string Fingerprint(LevelData level)
        {
            bool stateful = false;
            foreach (var p in level.Placements)
                if (p.type == PieceType.Switch || p.type == PieceType.Gate) stateful = true;
            var text = new StringBuilder();
            void Add(int value) => text.Append(value.ToString(CultureInfo.InvariantCulture)).Append(',');
            Add(level.Width); Add(level.Height); Add(level.MoveLimit); Add((int)level.TargetColor);
            foreach (var p in level.Placements)
            { Add((int)p.type); Add((int)p.color); Add((int)p.direction); Add(p.position.x); Add(p.position.y); if (stateful) { Add(p.channel); Add(p.initialOpen ? 1 : 0); } }
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(text.ToString()));
            var key = new StringBuilder(64);
            foreach (byte b in bytes) key.Append(b.ToString("x2", CultureInfo.InvariantCulture));
            return key.ToString();
        }
        public static int? Get(LevelData level) => Fingerprint(level) switch
            {
                "c54ad4bbe3bea8d6c5524e573b31bc932a9a7a614d0188cc52c2d626a044c923" => 3, // Level 7
                "6b97ccad6d8019f132d426fff3e3edf090c85cdcc7eb57a310f09f90aba8708a" => 2, // Level 9
                "b8df582c1f28621f718812e3d8b693f705134e40d261c64e5de27f89f14693fa" => 4, // Level 10
                "c1d32e2906d92e30684f00e5cf2ee9cecf2f6830b5710246dd33214fcd80cd65" => 5, // Level 15
                "ad2786efbb4cd32c4b5d7e7863f074acc34881095f9eda3f279ad691f455aa42" => 6, // Level 18
                "1c549ddf877c60c8ddb80cd9c184f730668c1e6e69e3bca998e8afec1f08222d" => 6, // Level 20
                "fb73cee904f63db731e5f343944c305555fe273e0c6385955df477332d798819" => 3, // Level 21; Batch21Tests exhaustive graph
                "cf44606a27f3acc1adda538254db7c49e61fed3550c6a19b9581540969fd61b3" => 4, // Level 22
                "ed06e6ae8f3ff52d8d85f73c92e11959cbb0e51b2bd68bbe71a6a6c35de63b74" => 5, // Level 23
                "19f0543c07aa27b5205f0e56bd5595b713b964df1398d477a3b5a17c3c5d761d" => 5, // Level 24
                "a4a8e0a64460f72551914ddda2559a84789b0ee08a3397883aa9624ce0b7cce6" => 6, // Level 25
                "7d9964028bf1fe1f8657cf837a93fca3a6eec9186a88bdee77474341ab5e3110" => 5, // Level 30; 25 search nodes
                "502582fa6502dddfc407559567db9588a4c411e8aa77d46d08f022b7fe881bc3" => 5, // Level 34; 23 search nodes
                "140f70bf39286329fe3ea49618a2fdff3292edd840e705c04a2dfb137409c09c" => 6, // Level 37; 37 search nodes
                "505c8f07720ce7b18ca458168ce8b61d797ac2cf044158448385133fe8097705" => 4, // Level 39; 16 search nodes
                "efd5d59b3bbb2c44fd3c92b60d2f07df7697d1701783faf7d67649c8e5ca1b23" => 6, // Level 40; 106 search nodes
                "fd88e0219163ed9f1845d5d12da56634dc38be83d2ff3eaa11e8cb69091e473a" => 4, // Level 26; Batch26Tests exhaustive graph
                "de9dd5d8dd6ff56a34c90df691dd4c94637dbe30bfa862e01a771a0728f92fe9" => 5, // Level 27; Batch26Tests exhaustive graph
                "19790755fcdb21b76c721fa8566da7743e5c67650c8c08d383f65bb18c507eb3" => 5, // Level 28; Batch26Tests exhaustive graph
                "21a6b3177c010caae6bb0a60067308856802be355d82fca5ea5682c2d99625b3" => 5, // Level 29; Batch26Tests exhaustive graph
                "39d5ce53c64e076218c0f7e64d1a74d0217698ceb5eba5ac8c00639b99ed7572" => 7, // Level 30; Batch26Tests exhaustive graph
                "744cb83d92c36abbadc7a3e4ae7566189711456d60c1773c4a82cb7655023387" => 4, // Level 26 V2; Refinement26Tests
                "c7a665dfa8a9a35682f0abec70fadfb3ba0a9de0660e923dae86ca418da932dd" => 5, // Level 27 V2; Refinement26Tests
                "4603039100919d67e3ad2e2917ce00cd0797291147640ebc17f4d1007a426c7c" => 7, // Level 30 V2; Refinement26Tests
                "d9d9edcd513649fca8a8eef743be282b5aef31063c4658b4daf3b92d8bd82265" => 6, // Level 31; Batch31Tests exhaustive graph
                "8c310620b64b6de7938cf540f44785c3de8800db4878127d7c6f3e93d3c907ba" => 6, // Level 32; Batch31Tests exhaustive graph
                "51518d5ddc9e781896de5e1bf54c69680abb4164a4c56a7fcd6ddb9092e37018" => 6, // Level 33; Batch31Tests exhaustive graph
                "ce2dfd41bbe2e1f45bc29d392769de3296c2512297389fd14c99330d08ebf5bc" => 5, // Level 34; Batch31Tests exhaustive graph
                "a4014867b8f7864d57f1c514fd4a590f845a299179d7ce14e6449fa6c2dd4a48" => 8, // Level 35; Batch31Tests exhaustive graph
                _ => null
            };
    }
}
