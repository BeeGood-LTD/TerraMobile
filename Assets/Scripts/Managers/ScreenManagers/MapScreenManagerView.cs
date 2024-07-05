using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapScreenManagerView : MonoBehaviour
{
    public static MapScreenManagerView Instance { get; private set; }

    [SerializeField] private Transform TopAnchorCachedTransform;
    [SerializeField] private Transform BottomAnchorCachedTransform;
    [SerializeField] private RectTransform mapTransform;
    [SerializeField] private Image mapImage;
    [SerializeField] private LocationMarkView locMarkButton;
    [SerializeField] private Button scaleButton;
    [field:SerializeField] public LocationBlockView locationBlock { get; private set; }
    [SerializeField] private LocationInfoScreenView locationInfoScreen;
    [SerializeField] private GalleryScreenManagerView galleryScreen;
    [SerializeField] private List<MarkIcons> markIcons;
    [SerializeField] private Sprite regionIcon;

    private LocationInfoScreenView cachedLocationInfo;
    private List<PlaceData> cachedPlaces = new();
    private List<LocationMarkView> mapMarks = new();
    private Vector3 startDelta;
    private Vector3 startPosition;
    private Vector3 mouseResultDelta;
    private string currentRegion;
    private bool isScale;

    private void Awake()
    {
        Instance = this;
    }

    public void ShowScreen(string regionKey, List<PlaceData> places)
    {
        if (currentRegion == regionKey)
        {
            return;
        }

        cachedPlaces.Clear();
        cachedPlaces = places;
        currentRegion = regionKey;
        InitRegionMap();
    }

    public void InitRegionMap()
    {
        mapTransform.localPosition = new Vector3(0, 180f, 0);
        mapTransform.localScale = Vector3.one * 1.2f;
        mouseResultDelta = mapTransform.position;
        var mapData = FileExtensions.GetJsonData<RegionMapData>(PathConstants.MapData())[0];

        mapImage.sprite = FileExtensions.GetLocalImage($"{PathConstants.MapPath()}/{mapData.ImagePath}");
        TopAnchorCachedTransform.localPosition = new Vector3(mapData.TopPointX, mapData.TopPointY, 0);
        BottomAnchorCachedTransform.localPosition = new Vector3(mapData.BottomPointX, mapData.BottomPointY, 0);
        
        foreach (var mark in mapMarks)
        {
            Destroy(mark.gameObject);
        }
        
        mapMarks.Clear();
        
        foreach (var place in cachedPlaces)
        {
            var mark = Instantiate(locMarkButton, mapTransform);
            mark.transform.localPosition = new Vector3(place.MarkX, place.MarkY, 0);
            mark.Image.sprite = regionIcon;
            mark.Text.text = place.Localization;
            mark.Text.fontSize = 20f;
            
            //PROMO
            if (place.Name is not ("Promo1" or "Promo2"))
            {
                mark.Button.onClick.AddListener(() => InitPlaceMap(place.Name));
            }
            
            mapMarks.Add(mark);
        }
        
        scaleButton.gameObject.SetActive(false);
    }

    private void ScaleMap(string placeName)
    {
        var placeMap = FileExtensions.GetJsonData<RegionMapData>(PathConstants.MapLocationData(placeName));
        var placeData = isScale ? placeMap[1] : placeMap[0];
        var scale = isScale ? 4.5f : 3f;
        TopAnchorCachedTransform.localPosition = new Vector3(placeData.TopPointX, placeData.TopPointY, 0);
        BottomAnchorCachedTransform.localPosition = new Vector3(placeData.BottomPointX, placeData.BottomPointY, 0);
        mapTransform.localScale = Vector3.one * scale;
        isScale = !isScale;

        foreach (var mark in mapMarks)
        {
            mark.transform.localScale = Vector3.one * (isScale ? 0.6f : 0.4f);
        }
        
        startDelta = Input.mousePosition;
        startPosition = mapTransform.position;
        var mouseDelta = Input.mousePosition - startDelta;
            
        var mouseResultDeltaX = startPosition.x + mouseDelta.x / 2;
        var mouseResultDeltaY = startPosition.y + mouseDelta.y / 2;
            
        CheckBorder(ref mouseResultDeltaX, ref mouseResultDeltaY);
            
        mouseResultDelta = new Vector3(mouseResultDeltaX, mouseResultDeltaY, startPosition.z);
    }
    
    public void InitPlaceMap(string placeName)
    {
        isScale = false;
        mapTransform.localPosition = Vector3.left * -700f;
        mouseResultDelta = mapTransform.position;
        
        var placeMapData = FileExtensions.GetJsonData<RegionMapData>(PathConstants.MapLocationData(placeName))[0];
        TopAnchorCachedTransform.localPosition = new Vector3(placeMapData.TopPointX, placeMapData.TopPointY, 0);
        BottomAnchorCachedTransform.localPosition = new Vector3(placeMapData.BottomPointX, placeMapData.BottomPointY, 0);
        mapImage.sprite = FileExtensions.GetLocalImage($"{PathConstants.MapLocationPath(placeName)}/{placeMapData.ImagePath}");
        
        foreach (var mark in mapMarks)
        {
            Destroy(mark.gameObject);
        }
        
        mapMarks.Clear();
        
        var locationManager = LocationsManagerView.Instance;
        locationManager.UpdateLocationData(placeName);
        foreach (var location in locationManager.Locations)
        {
            var mark = Instantiate(locMarkButton, mapTransform);

            if (location.History == "...")
            {
                mark.RectTransform.sizeDelta = new Vector2 (50f, 50f);
            }
            
            mark.Image.sprite = markIcons[location.MarkIndex].Icon;
            mark.Icon = markIcons[location.MarkIndex].Icon;
            mark.Text.text = location.Localization;
            mark.transform.localPosition = new Vector3(location.MarkX, location.MarkY, 0);
            mark.Button.onClick.AddListener(() =>
            {
                locationBlock.gameObject.SetActive(true);
                locationBlock.Title.text = location.Localization;
                locationBlock.Info.text = location.Info;
                locationBlock.Description.text = location.Description;
                locationBlock.Group.spacing = 0f;

                ClearIconMarks();
                mark.Image.sprite = markIcons[location.MarkIndex].Active;
                locationBlock.OpenInfoButton.onClick.RemoveAllListeners();

                if (location.History == "...")
                {
                    locationBlock.OpenInfoButton.gameObject.SetActive(false);
                    return;
                }

                locationBlock.OpenInfoButton.gameObject.SetActive(true);
                locationBlock.OpenInfoButton.onClick.AddListener(() =>
                {
                    cachedLocationInfo = Instantiate(locationInfoScreen);
                    cachedLocationInfo.Title.text = location.Localization;
                    cachedLocationInfo.Info.text = location.Info;
                    cachedLocationInfo.Description.text = location.ShortHistory;
                    
                    cachedLocationInfo.Head.sprite = FileExtensions.GetLocalImage($"{PathConstants.PlaceLocationPath(placeName)}/{location.HeadPath}");
                    cachedLocationInfo.Media.sprite = FileExtensions.GetLocalImage($"{PathConstants.PlaceLocationPath(placeName)}/{location.MediaPath}");
                    
                    cachedLocationInfo.Close.onClick.AddListener(() =>
                    {
                        Destroy(cachedLocationInfo.gameObject);
                        cachedLocationInfo = null;
                    });
                    cachedLocationInfo.OpenInfo.onClick.AddListener(() =>
                    {
                        cachedLocationInfo.Description.text = location.History;
                        cachedLocationInfo.OpenInfo.gameObject.SetActive(false);
                        cachedLocationInfo.group.GetComponent<VerticalLayoutGroup>().spacing -= 1; //force update
                    });
                    cachedLocationInfo.Tour.onClick.AddListener(() =>
                    {
                        var tourManager = TourManagerView.Instance;
                        tourManager.UpdateTourData(placeName);
                        tourManager.SetSkyboxTexture(tourManager.GetTourData(location.Index), true);
                        ScreenManagerView.Instance.MapHudOnTour(cachedLocationInfo.gameObject);
                    });
                    cachedLocationInfo.Gallery.onClick.AddListener(() =>
                    {
                        var gallery = Instantiate(galleryScreen);
                        gallery.InitGallery(placeName, location.Index);
                    });
                });
            });
            mapMarks.Add(mark);
        }
        
        ScaleMap(placeName);
        scaleButton.onClick.RemoveAllListeners();
        scaleButton.onClick.AddListener(() => ScaleMap(placeName));
        scaleButton.gameObject.SetActive(true);
    }

    public void ClearIconMarks()
    {
        foreach (var mark in mapMarks)
        {
            mark.Image.sprite = mark.Icon;
        }
    }
    
    private void Update()
    {
        if (locationBlock.gameObject.activeSelf)
        {
            locationBlock.Group.spacing = Mathf.Lerp(locationBlock.Group.spacing, 40f, 10 * Time.deltaTime);
        }
        
        if (cachedLocationInfo != null)
        {
            return;
        }
        
        if (Input.GetMouseButtonDown(0))
        {
            startDelta = Input.mousePosition;
            startPosition = mapTransform.position;
        }

        if (Input.GetMouseButton(0))
        {
            var mouseDelta = Input.mousePosition - startDelta;
            
            var mouseResultDeltaX = startPosition.x + mouseDelta.x;
            var mouseResultDeltaY = startPosition.y + mouseDelta.y;
            
            CheckBorder(ref mouseResultDeltaX, ref mouseResultDeltaY);
            
            mouseResultDelta = new Vector3(mouseResultDeltaX, mouseResultDeltaY, startPosition.z);
        }
        
        mapTransform.position = Vector3.Slerp(mapTransform.position, mouseResultDelta, 6f * Time.deltaTime);
    }
    
    private void CheckBorder(ref float deltaX, ref float deltaY)
    {
        if (deltaX <= BottomAnchorCachedTransform.position.x)
        {
            deltaX = BottomAnchorCachedTransform.position.x;
        }

        if (deltaX >= TopAnchorCachedTransform.position.x)
        {
            deltaX = TopAnchorCachedTransform.position.x;
        }

        if (deltaY <= BottomAnchorCachedTransform.position.y)
        {
            deltaY = BottomAnchorCachedTransform.position.y;
        }

        if (deltaY >= TopAnchorCachedTransform.position.y)
        {
            deltaY = TopAnchorCachedTransform.position.y;
        }
    }
}

[Serializable]
public class MarkIcons
{
    public Sprite Active;
    public Sprite Icon;
}