using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System;

public static class AssetCreator
{
    //public static Texture2D CopyTexture2DAsset(Texture2D texture2D, string path, bool filter, bool readable, bool format)
    //{
    //    var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GetAssetPath(texture2D));
    //    return texture;
    //}

    public static Texture2D GetTexture2DFromAssets(Texture2D texture2D, string path, bool filter = true, bool readable = true, bool format = true)
    {
        //string dirPath = "Assets/Sprites/Assessment2/";

        //if (!Directory.Exists(dirPath))
        //{
        //    Directory.CreateDirectory(dirPath);
        //}

        //string fullPath = $"{path}.bmp";

        AssetDatabase.CreateAsset(texture2D, path);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        var A = AssetImporter.GetAtPath(path) as TextureImporter;

        // TextureImporter A = (TextureImporter)AssetImporter.GetAtPath(path);

        var texSet = A.GetDefaultPlatformTextureSettings() as TextureImporterPlatformSettings;

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
        }
        else
        {
            texSet.format = TextureImporterFormat.RGBA32;
        }
        A.SetPlatformTextureSettings(texSet);

        string newPath = path + "1.bmp";

        AssetDatabase.CopyAsset(path, newPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Texture2D texture2D1 = AssetDatabase.LoadAssetAtPath<Texture2D>(newPath);

        texture2D1.Apply();

        return texture2D1;
    }


    public static Texture2D CreateTexture2D(Texture2D texture2D, string path, bool filter = true, bool readable = true, bool format = true)
    {
        var bytes = texture2D.EncodeToPNG();

        var timeStamp = DateTime.Now.ToString("yyyyMMddHHmmssfff");

        string dirPath = "Assets/Sprites/Assessment2/";

        if (!Directory.Exists(dirPath))
        {
            Directory.CreateDirectory(dirPath);
        }

        Debug.Log(timeStamp);

        string fullPath = $"{path}.bmp";

        File.WriteAllBytes(fullPath, bytes);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        TextureImporter A = (TextureImporter)AssetImporter.GetAtPath(fullPath);
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

        AssetDatabase.ImportAsset(fullPath, ImportAssetOptions.ForceUpdate);

        Texture2D texture2D1 = AssetDatabase.LoadAssetAtPath<Texture2D>(fullPath);

        texture2D1.Apply();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        return texture2D1;

        // Print the path of the created asset
        // Debug.Log(AssetDatabase.GetAssetPath(texture2D));
    }

    /*
    
        // Create a simple Texture2D asset

        // string pathFinal = path + "1.bmp";

        // Texture2D texture2D = new Texture2D(width, height);
        //bool copy = AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(texture2D), path);
        //Debug.Log(copy);

        //Release CachedFileHandles to avoid any I/O errors
        // AssetDatabase.ReleaseCachedFileHandles();

        // File.Delete(path);

        //  File.Replace(path, $"Assets/Sprites/Assessment2/replace.bmp", $"Assets/Sprites/Assessment2/backup.bmp");

    */

    public static Texture2D SaveTexture2D(Texture2D texture2D, string path, bool filter, bool readable, bool format)
    {
        var timeStamp = DateTime.Now.ToString("yyyyMMddHHmmssfff");

        var bytes = texture2D.EncodeToPNG();

        string dirPath = "Assets/Sprites/Assessment3/";

        if (!Directory.Exists(dirPath))
        {
            Directory.CreateDirectory(dirPath);
        }

        Debug.Log(timeStamp);

        string fullPath = $"{path}.bmp";

        File.WriteAllBytes(fullPath, bytes);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // AssetDatabase.CreateAsset(texture2D, fullPath);

        TextureImporter A = (TextureImporter)AssetImporter.GetAtPath(fullPath);
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

        AssetDatabase.ImportAsset(fullPath, ImportAssetOptions.ForceUpdate);

        File.WriteAllBytes(fullPath, bytes);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log(timeStamp);

        return (Texture2D)AssetDatabase.LoadAssetAtPath(fullPath, typeof(Texture2D));
    }
}
