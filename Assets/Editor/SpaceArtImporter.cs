using UnityEditor;
using UnityEngine;

/// <summary>
/// Import settings for the pixel-art assets under Assets/Art.
/// Runs only the first time a texture is imported (importSettingsMissing), so any later
/// manual tweak in the Inspector or Sprite Editor is kept.
/// </summary>
public class SpaceArtImporter : AssetPostprocessor
{
    const string ArtRoot = "Assets/Art/";
    const float PixelsPerUnit = 100f;

    void OnPreprocessTexture()
    {
        if (!assetPath.StartsWith(ArtRoot) || !assetImporter.importSettingsMissing) return;

        var importer = (TextureImporter)assetImporter;
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = PixelsPerUnit;
        importer.filterMode = FilterMode.Point;                      // keep pixel edges sharp
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.mipmapEnabled = false;
        importer.alphaIsTransparency = true;

        // 9-slice borders for stretchable UI (left, bottom, right, top), in pixels.
        string file = System.IO.Path.GetFileNameWithoutExtension(assetPath);
        if (file.StartsWith("button")) importer.spriteBorder = new Vector4(28, 16, 28, 16);
        else if (file == "panel") importer.spriteBorder = new Vector4(56, 56, 56, 56);
    }
}
