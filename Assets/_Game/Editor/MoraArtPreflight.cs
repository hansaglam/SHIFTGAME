using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Shift.Game.Editor
{
    /// <summary>
    /// Non-destructive art QA for the Living Seed character.
    /// Checks source import contracts without editing textures or gameplay.
    /// </summary>
    public static class MoraArtPreflight
    {
        private const string Root = "Assets/_Game/Resources/Mora";
        private static readonly string[] Expressions = { "Idle", "Curious", "Happy" };
        private static readonly string[] Stages = { "S1", "S2", "S3", "S4" };

        [MenuItem("SHIFT/MORA/Validate Living Seed Sprite Production")]
        public static void ValidateLivingSeedSprites()
        {
            int errors = 0, warnings = 0, ready = 0;
            var details = new StringBuilder();
            Vector2? referencePivot = null;
            Vector2Int? referenceDimensions = null;

            // Stage I Idle is the minimum meaningful master-art deliverable;
            // the remaining states are required for a full vertical slice.
            foreach (var stage in Stages)
            {
                foreach (string expression in Expressions)
                {
                    string name = "MoraSeed_" + stage + "_" + expression;
                    string path = Root + "/" + name + ".png";
                    Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                    var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                    var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                    bool critical = stage == "S1" && expression == "Idle";

                    if (sprite == null || importer == null || tex == null)
                    {
                        if (critical) { errors++; details.AppendLine("ERROR: Missing primary MORA master: " + path); }
                        else { warnings++; details.AppendLine("PENDING: " + name + " (not yet delivered)"); }
                        continue;
                    }

                    ready++;
                    if (importer.textureType != TextureImporterType.Sprite ||
                        importer.spriteImportMode != SpriteImportMode.Single)
                    {
                        errors++;
                        details.AppendLine("ERROR: " + name + " must be Sprite (2D and UI), Single.");
                    }
                    if (!importer.DoesSourceTextureHaveAlpha() || !importer.alphaIsTransparency)
                    {
                        errors++;
                        details.AppendLine("ERROR: " + name + " needs actual transparent pixels and Alpha Is Transparency.");
                    }
                    if (tex.width < 1024 || tex.height < 1024)
                    {
                        warnings++;
                        details.AppendLine("WARNING: " + name + " is below 1024 px source resolution.");
                    }
                    if (tex.width != tex.height)
                    {
                        warnings++;
                        details.AppendLine("WARNING: " + name + " uses non-square source dimensions.");
                    }
                    if (importer.mipmapEnabled)
                    {
                        warnings++;
                        details.AppendLine("WARNING: " + name + " has mipmaps enabled for a UI sprite.");
                    }
                    if (importer.textureCompression != TextureImporterCompression.Uncompressed)
                    {
                        warnings++;
                        details.AppendLine("WARNING: " + name + " is compressed; inspect with no compression while approving art.");
                    }

                    Vector2 size = sprite.rect.size;
                    var pivot = new Vector2(sprite.pivot.x / size.x, sprite.pivot.y / size.y);
                    if (!referencePivot.HasValue)
                    {
                        referencePivot = pivot;
                        referenceDimensions = new Vector2Int(tex.width, tex.height);
                    }
                    else
                    {
                        if (Vector2.Distance(referencePivot.Value, pivot) > .015f)
                        {
                            warnings++;
                            details.AppendLine("WARNING: " + name + " pivot differs from Stage I master.");
                        }
                        if (referenceDimensions.Value.x != tex.width || referenceDimensions.Value.y != tex.height)
                        {
                            warnings++;
                            details.AppendLine("WARNING: " + name + " pixel canvas differs from Stage I master.");
                        }
                    }
                }
            }

            string summary = "MORA art preflight: " + ready + "/12 source sprites present; "
                + errors + " errors; " + warnings + " warnings/pending.\\n"
                + "This is not a visual approval and does not verify expressions or silhouettes.\\n\\n"
                + details.ToString();
            if (errors > 0) Debug.LogError(summary);
            else if (warnings > 0) Debug.LogWarning(summary);
            else Debug.Log(summary);
            EditorUtility.DisplayDialog("MORA Living Seed Asset Preflight",
                "Present: " + ready + "/12\\nErrors: " + errors + "\\nPending/warnings: " + warnings +
                "\\nFull details are in the Console. No files were modified.", "OK");
        }
    }
}
