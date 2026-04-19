using UnityEngine;
using UnityEditor;
using System.IO;

[InitializeOnLoad]
public class GenerateDottedTexture
{
    static GenerateDottedTexture()
    {
        EditorApplication.delayCall += GenerateTexture;
    }

    private static void GenerateTexture()
    {
        string dir = "Assets/Textures";
        if (!AssetDatabase.IsValidFolder(dir))
        {
            AssetDatabase.CreateFolder("Assets", "Textures");
        }

        string texPath = dir + "/DottedLine.png";
        
        // Only generate if it doesn't already exist
        if (!File.Exists(texPath))
        {
            // Create a 2x1 texture (Half solid, half transparent)
            Texture2D tex = new Texture2D(2, 1, TextureFormat.RGBA32, false);
            
            // Left pixel solid white (the dash)
            tex.SetPixel(0, 0, Color.white);
            
            // Right pixel completely transparent (the gap)
            tex.SetPixel(1, 0, Color.clear);
            
            // Apply changes
            tex.Apply();

            // Save as PNG
            byte[] bytes = tex.EncodeToPNG();
            File.WriteAllBytes(texPath, bytes);
            
            // Force Unity to import the new image
            AssetDatabase.Refresh();
            
            // Ensure the texture imports as Point Filtered so it stays crisp instead of blurry!
            TextureImporter importer = AssetImporter.GetAtPath(texPath) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Default;
                importer.npotScale = TextureImporterNPOTScale.None;
                importer.filterMode = FilterMode.Point;
                importer.wrapMode = TextureWrapMode.Repeat;
                importer.SaveAndReimport();
            }

            Debug.Log("Successfully generated a Dotted-Line Texture at: " + texPath);
        }
    }
}
