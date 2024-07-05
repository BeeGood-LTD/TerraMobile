using System;
using System.Collections.Generic;
using UnityEngine;

public class PlacesScreenManagerView : MonoBehaviour
{
    public List<PlaceData> Places { get; private set; } = new();
    
    [SerializeField] private PlaceInfoScreenView infoScreenPrefab;
    [SerializeField] private RectTransform contentRect;
    [SerializeField] private float distanceValue;
    [SerializeField] private float slideValue;
    [SerializeField] private List<PlaceBlockView> cards;
    
    private const float MaxScale = 1.2f;
    private const float MinScale = 1f;
    private string currentRegion;

    public void ShowScreen(string regionKey)
    {
        if (currentRegion == regionKey)
        {
            return;
        }

        currentRegion = regionKey;
        InitCardData();
    }
    
    private void InitCardData()
    {
        Places.Clear();
        var placesJsonData = FileExtensions.GetJsonData<PlaceData>(PathConstants.PlacesData());
        foreach (var placesData in placesJsonData)
        {
            placesData.Sprite = FileExtensions.GetLocalImage($"{PathConstants.PlaceLocationPath(placesData.Name)}/{placesData.ImagePath}");
            Places.Add(placesData);
        }
        
        var cardsCount = Places.Count;
        SetCardValues(cardsCount - 1, 0, cardsCount > 1 ? 1 : 0);
    }

    private void OnSlideLeft()
    {
        var leftIndex = cards[1].Index;
        var middleIndex = cards[2].Index;
        var rightIndex = cards[2].Index + 1;
        
        if (rightIndex == Places.Count)
        {
            rightIndex = 0;
        }

        SetCardValues(leftIndex, middleIndex, rightIndex);
    }
    
    private void OnSlideRight()
    {
        var leftIndex = cards[0].Index - 1;
        var middleIndex = cards[0].Index;
        var rightIndex = cards[1].Index;
        
        if (leftIndex == -1)
        {
            leftIndex = Places.Count - 1;
        }

        SetCardValues(leftIndex, middleIndex, rightIndex);
    }

    private void SetCardValues(int leftIndex, int middleIndex, int rightIndex)
    {
        SetValues(0, leftIndex);
        SetValues(1, middleIndex);
        SetValues(2, rightIndex);
    }
    
    private void SetValues(int index, int dataIndex)
    {
        cards[index].Index = dataIndex;
        cards[index].Image.sprite = Places[dataIndex].Sprite;
        cards[index].TitleText.text = Places[dataIndex].Localization;
        cards[index].InfoText.text = Places[dataIndex].Info;
        
        cards[index].Button.onClick.RemoveAllListeners();
        
        //PROMO
        if (Places[dataIndex].Name is "Promo1" or "Promo2")
        {
            return;
        }
        
        cards[index].Button.onClick.AddListener(() =>
        {
            var infoScreen = Instantiate(infoScreenPrefab);
            infoScreen.UpdateData(
                Places[dataIndex].Name,
                Places[dataIndex].Localization, 
                Places[dataIndex].Info, 
                Places[dataIndex].Description, 
                Places[dataIndex].VideoPath);
        });
    }
    
    private void TrySlideCards()
    {
        var distance = 0f;
        var isSlide = false;
        var leftOffset = contentRect.offsetMin.x;
        var rightOffset = -contentRect.offsetMax.x;
            
        if (leftOffset < -slideValue)
        {
            isSlide = true;
            distance = distanceValue;
            OnSlideLeft();
        }
            
        if (rightOffset < -slideValue)
        {
            isSlide = true;
            distance = -distanceValue;
            OnSlideRight();
        }

        if (isSlide == false)
        {
            return;
        }
            
        contentRect.offsetMin = new Vector2(distance + leftOffset, 0);
        contentRect.offsetMax = new Vector2(distance - rightOffset, 0);
    }

    private void CardsScaleControl()
    {
        OnScaleChange(0, MaxScale, MinScale, contentRect.offsetMin.x);
        OnScaleChange(1, MinScale, MaxScale, Math.Abs(contentRect.offsetMin.x));
        OnScaleChange(2, MaxScale, MinScale, -contentRect.offsetMax.x);
    }

    private void OnScaleChange(int index, float max, float min, float offset)
    {
        var range = MathExtensions.ValueToRange(distanceValue, 0f, offset <= 0f ? 0f : offset);
        var value = MathExtensions.RangeToValue(max, min, range);
        cards[index].transform.localScale = Vector3.one * value;
    }
    
    private void Update()
    {
        if (Input.GetMouseButtonUp(0))
        {
            TrySlideCards();
        }

        CardsScaleControl();
    }
}
