#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Shift.Game.Editor
{
    /// <summary>
    /// Automatic, non-destructive import settings ONLY for MoraPremium textures.
    /// Keeps sliced premium panels from being stretched like placeholder sprites.
    /// </summary>
    public sealed class MoraPremiumAssetImporter : AssetPostprocessor
    {
        private void OnPreprocessTexture()
        {
            if (!assetPath.StartsWith("Assets/_Game/Resources/MoraPremium/"))
                return;
            var importer = (TextureImporter)assetImporter;
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.isReadable = false;
            importer.filterMode = FilterMode.Bilinear;
            importer.textureCompression = TextureImporterCompression.CompressedHQ;
            bool characterOrHabitat = assetPath.Contains("MoraStage") ||
                assetPath.Contains("HabitatStage");
            importer.maxTextureSize = characterOrHabitat ? 1024 : 512;
            string file = System.IO.Path.GetFileNameWithoutExtension(assetPath);
            importer.spriteBorder = file.StartsWith("Frame")
                ? new Vector4(34f, 32f, 34f, 32f) : Vector4.zero;
        }
    }
}
#endif
