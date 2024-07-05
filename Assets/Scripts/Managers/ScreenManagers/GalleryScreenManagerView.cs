using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GalleryScreenManagerView : MonoBehaviour
{
    [SerializeField] private Image mainImage;
    [SerializeField] private Button rightButton;
    [SerializeField] private Button leftButton;
    [SerializeField] private Button closeButton;
    
    private List<GalleryData> galleryData = new();
    private List<Sprite> sprites = new();
    private int currentIndex;

    public void InitGallery(string placeName, int index)
    {
        galleryData = FileExtensions.GetJsonData<GalleryData>(PathConstants.GalleryData(placeName));
        foreach (var gallery in galleryData)
        {
            if (gallery.Index != index)
            {
                continue;
            }
            
            ShowGallery(gallery, placeName);
            break;
        }

        closeButton.onClick.AddListener(() => Destroy(gameObject));
        rightButton.onClick.AddListener(() => ShiftSprite(1));
        leftButton.onClick.AddListener(() => ShiftSprite(-1));
    }

    private void ShowGallery(GalleryData gallery, string placeName)
    {
        foreach (var imagePath in gallery.ImagesPath)
        {
            var sprite = FileExtensions.GetLocalImage($"{PathConstants.GalleryPath(placeName)}/{imagePath}");
            sprites.Add(sprite);
        }

        mainImage.sprite = sprites[currentIndex];
    }

    private void ShiftSprite(int shift)
    {
        if (currentIndex + shift >= sprites.Count)
        {
            return;
        }
        
        if (currentIndex + shift < 0)
        {
            return;
        }
        
        currentIndex += shift;
        mainImage.sprite = sprites[currentIndex];
    }
}