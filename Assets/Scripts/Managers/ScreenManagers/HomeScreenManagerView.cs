using System.Collections.Generic;
using Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HomeScreenManagerView : MonoBehaviour
{
    [SerializeField] private NewsBlockView newsPrefab;
    [SerializeField] private RectTransform content;
    [SerializeField] private Transform gridParent;
    [SerializeField] private Image headHome;
    [SerializeField] private TextMeshProUGUI regionButtonText;
    [SerializeField] private NewScreenView newScreenPrefab;
    public List<GameObject> newBlocks = new();

    private const string Icon = "<sprite index=0>";
    private const float NewsHeight = 490;
    private GameObject currentNew;
    private string currentRegion;

    public void ShowScreen(string regionKey)
    {
        content.anchoredPosition = Vector2.zero;
        if (currentRegion == regionKey)
        {
            return;
        }

        currentRegion = regionKey;
        regionButtonText.text = $"{Icon}{ScreenManagerView.Instance.CurrentRegionTitle}";
        headHome.sprite = FileExtensions.GetLocalImage(PathConstants.HomeHead());
        SetNewsData();
    }

    public void ClearOpenNew()
    {
        if (currentNew == null)
        {
            return;
        }
        
        Destroy(currentNew.gameObject);
        currentNew = null;
    }
    
    private void SetNewsData()
    {
        ClearNews();
        var newsJsonData = FileExtensions.GetJsonData<NewData>(PathConstants.HomeData());
        foreach (var newsData in newsJsonData)
        {
            AddNewsBlock(newsData);
        }
    }
    
    private void AddNewsBlock(NewData data)
    {
        var item = Instantiate(newsPrefab, gridParent);
        newBlocks.Add(item.gameObject);
        content.sizeDelta = new Vector2(0, content.sizeDelta.y + NewsHeight);

        item.Data.text = data.Time;
        item.Title.text = data.Title;
        item.NewsText.text = data.ShortText;
        item.Image.sprite = FileExtensions.GetLocalImage($"{PathConstants.HomePath()}/{data.ShortImagePath}");
        item.OpenNewButton.onClick.AddListener(() =>
        {
            var newScreen = Instantiate(newScreenPrefab);
            newScreen.Data.text = data.Time;
            newScreen.Title.text = data.Title;
            newScreen.Info.text = data.Description;
            newScreen.Image.sprite = FileExtensions.GetLocalImage($"{PathConstants.HomePath()}/{data.HeadImagePath}");
            currentNew = newScreen.gameObject;
        });
    }

    private void ClearNews()
    {
        foreach (var block in newBlocks)
        {
            Destroy(block);
        }
    }
}