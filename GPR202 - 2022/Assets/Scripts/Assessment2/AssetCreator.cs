using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System;

public static class AssetCreator
{
    public static Texture2D CreateTexture2D(Texture2D texture2D, string path, bool filter, bool readable, bool format)
    {
        // Create a simple Texture2D asset

        // Texture2D texture2D = new Texture2D(width, height);
        //bool copy = AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(texture2D), path);
        //Debug.Log(copy);

        File.WriteAllBytes(path, texture2D.EncodeToPNG());

        AssetDatabase.Refresh();

        TextureImporter A = (TextureImporter)AssetImporter.GetAtPath(path);
        TextureImporterPlatformSettings texSet = A.GetDefaultPlatformTextureSettings();

        // DEBUGGING
        if (filter)
            A.filterMode = FilterMode.Point;
        else
            A.filterMode = FilterMode.Bilinear;
        if (readable)
            A.isReadable = true;
        else
            A.isReadable = false;
        if (format)
        {
            texSet.format = TextureImporterFormat.RGB24;
            A.SetPlatformTextureSettings(texSet);
        }
        else
        {
            texSet.format = TextureImporterFormat.RGBA32;
            A.SetPlatformTextureSettings(texSet);
        }
        // A.textureCompression = TextureImporterCompression.Uncompressed;

        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

        AssetDatabase.Refresh();

        Texture2D texture2D1 = AssetDatabase.LoadAssetAtPath<Texture2D>(path);

        texture2D1.Apply();

        return texture2D;


        // Print the path of the created asset
        // Debug.Log(AssetDatabase.GetAssetPath(texture2D));
    }
}
