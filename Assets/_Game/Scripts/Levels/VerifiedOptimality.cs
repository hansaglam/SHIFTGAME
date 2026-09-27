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
            // Preserve all legacy fingerprints; explicit objective overrides must bind the proof.
            var targets = level.ResolvedTargetColors;
            if (targets.Count != 1 || targets[0] != level.TargetColor)
            {
                Add(-360); Add(targets.Count);
                foreach (PieceColor color in System.Enum.GetValues(typeof(PieceColor)))
                    if (level.IsTargetColor(color)) Add((int)color);
            }
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
                "bcbb7b7b859aeae46f95515bb260d5a9a828bdc85425d6bf3c0a859c04912338" => 6, // Level 36; Batch36Tests exhaustive graph
                "ed61dd9a06b2c7168320c110eba59fa5ff6ff0140b0ad79f32fc65074c5a4dc6" => 6, // Level 37; Batch36Tests exhaustive graph
                "3e2327cc3c64050827f36e9453d24d62048fdd8aee898070684f8b4be31ee1a7" => 7, // Level 38; Batch36Tests exhaustive graph
                "29c61895aac08fcea307f02395c68ad4393558710f382d2c25ff8dfbdc076b88" => 5, // Level 39; Batch36Tests exhaustive graph
                "8e1575d58b5d5ae9925ea04bb1af60bc673acd1f4aff37db93f31b889e40ca5d" => 7, // Level 40; Batch36Tests exhaustive graph
                "f9ee98c455ba937fba2ef1a8d1914acc8a2bfaa37e7d294c458b16876b09cb59" => 7, // Level 38; Batch36Tests exhaustive graph
                "bf6268d7821c416f68af9762934543a17a36d09ec182f1fbc5ee3d8e148cbfd8" => 1, // Level 1; CampaignCertificationTests exhaustive proof
                "acc4bdb364d90f586b03aa3e67d29c215544540aa8addf0886badf8475ca3e05" => 3, // Level 2; CampaignCertificationTests exhaustive proof
                "ffb47718ee466b8a82d7bb4f87933d24c34275e75186f4af10ca2d427bf94f7a" => 3, // Level 3; CampaignCertificationTests exhaustive proof
                "2be4e3c182901696e85365867dd603750f5bc849d02f011f24923e57566b3149" => 1, // Level 4; CampaignCertificationTests exhaustive proof
                "19b4d2d46834393893d57b93657423f33a2b67e17ba72a1cb88fa3481998f0af" => 2, // Level 5; CampaignCertificationTests exhaustive proof
                "247ac4f044e6809624b8ec5803ef1575ddeb6625376cc6cd7f18a5c7dc2b3f1f" => 2, // Level 6; CampaignCertificationTests exhaustive proof
                "d04d8f4c455fe1918d862f69c614cde4135ef02ac5fcd7d1a7dc154664e3d1a5" => 2, // Level 8; CampaignCertificationTests exhaustive proof
                "763bb645012b69660121e0332ac867ea90316865017bd0580387bd84b7e6be17" => 4, // Level 11; CampaignCertificationTests exhaustive proof
                "89a1642cf0bebc8877e6b4180c77894e29616db06f8c86e6fa5159a27ca3d4e7" => 3, // Level 12; CampaignCertificationTests exhaustive proof
                "22d6a9ec7c0ac43c4ead66f901a14f97deff5fd8d482c8076a8e9239f8689e0a" => 2, // Level 13; CampaignCertificationTests exhaustive proof
                "fbaf334e1ba7a7413397540a924e887cd88367d91af6a4d821d64bd03c3fca92" => 4, // Level 14; CampaignCertificationTests exhaustive proof
                "d97fb41ffb2a4a239f26460faafc73d8275c839f69069a0d859504fee14bdb13" => 5, // Level 16; CampaignCertificationTests exhaustive proof
                "ecf105273e754b75d4e42ac88cfc06e7731e7fdee9d960c1a999052d243588c7" => 5, // Level 17; CampaignCertificationTests exhaustive proof
                "cfebae29399926003b05ddc5df632d9f9eb62acc5b48ae86afd7183e527a91d8" => 4, // Level 19; CampaignCertificationTests exhaustive proof
                _ => null
            };
    }
}
