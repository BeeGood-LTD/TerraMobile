using System;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class PlaceData
{
    [NonSerialized] public Sprite Sprite;

    public string Name;
    public string Localization;
    public string Info;
    public string Description;
    public string ImagePath;
    public string VideoPath;
    public float MarkX;
    public float MarkY;
}