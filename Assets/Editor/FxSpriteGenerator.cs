using System;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class FxSpriteGenerator
{
    private const string Folder = "Assets/Sprites/Fx";

    [MenuItem("Tools/Old Tech/Generate FX Sprites")]
    public static void Generate()
    {
        Directory.CreateDirectory(Folder);

        Write("Soft Circle", 128, d =>
        {
            float a = Mathf.Clamp01(1f - d);
            return a * a;
        });

        Write("Ring", 256, d =>
        {
            float distance = Mathf.Abs(d - 0.86f);
            return 1f - Mathf.SmoothStep(0f, 0.12f, distance);
        });

        Write("Rounded Box", 64, (x, y) =>
        {
            const float radius = 14f;
            const float half = 32f;
            Vector2 q = new(Mathf.Max(Mathf.Abs(x - half + 0.5f) - (half - radius), 0f), Mathf.Max(Mathf.Abs(y - half + 0.5f) - (half - radius), 0f));
            return Mathf.Clamp01(radius - q.magnitude + 0.5f);
        }, new Vector4(20f, 20f, 20f, 20f));

        Write("White", 4, (x, y) => 1f, Vector4.zero);

        AssetDatabase.Refresh();
    }

    private static void Write(string name, int size, Func<float, float> radialAlpha)
    {
        float half = size * 0.5f;
        Write(name, size, (x, y) =>
        {
            float dx = (x + 0.5f - half) / half;
            float dy = (y + 0.5f - half) / half;
            return radialAlpha(Mathf.Sqrt(dx * dx + dy * dy));
        }, Vector4.zero);
    }

    private static void Write(string name, int size, Func<int, int, float> alpha, Vector4 border)
    {
        Texture2D texture = new(size, size, TextureFormat.RGBA32, false);
        Color32[] pixels = new Color32[size * size];
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                pixels[y * size + x] = new Color32(255, 255, 255, (byte)Mathf.RoundToInt(Mathf.Clamp01(alpha(x, y)) * 255f));
            }
        }

        texture.SetPixels32(pixels);
        texture.Apply();

        string path = $"{Folder}/{name}.png";
        File.WriteAllBytes(path, texture.EncodeToPNG());
        UnityEngine.Object.DestroyImmediate(texture);

        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
        TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(path);
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.mipmapEnabled = false;
        importer.wrapMode = TextureWrapMode.Clamp;
        importer.alphaIsTransparency = true;
        importer.spriteBorder = border;
        importer.SaveAndReimport();
    }
}
