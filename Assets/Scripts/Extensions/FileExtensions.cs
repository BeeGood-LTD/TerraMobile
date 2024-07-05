using System.Collections.Generic;
using System.IO;
using Data;
using Newtonsoft.Json;
using UnityEngine;

public static class FileExtensions
{
    public static async void SetJsonData(string filePath, List<RegionData> localRegions)
    {
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }

        var serializedData = JsonConvert.SerializeObject(localRegions);
        await using var stream = File.OpenWrite(filePath);
        await using var writer = new StreamWriter(stream);
        await writer.WriteAsync(serializedData);
    }

    public static List<T> GetJsonData<T>(string filePath)
    {
        var source = new StreamReader(filePath);
        var fileContents = source.ReadToEnd();
        source.Close();

        var data = JsonConvert.DeserializeObject<List<T>>(fileContents);
        return data;
    }

    public static Sprite GetLocalImage(string filePath, float pixelsPerUnit = 100.0f)
    {
        Texture2D spriteTexture = GetTexture(filePath);

        if (spriteTexture == null)
        {
            Debug.LogError($"It was not possible to take an image from '{filePath}'");
            return null;
        }

        Sprite newSprite = Sprite.Create(spriteTexture, new Rect(0, 0, spriteTexture.width, spriteTexture.height),
            new Vector2(0, 0), pixelsPerUnit);

        return newSprite;
    }

    public static Texture2D GetTexture(string filePath)
    {
        Texture2D tex2D;
        byte[] fileData;

        if (File.Exists(filePath))
        {
            fileData = File.ReadAllBytesAsync(filePath).Result;
            tex2D = new Texture2D(2, 2);
            if (tex2D.LoadImage(fileData))
            {
                return tex2D;
            }
        }

        return null;
    }
}