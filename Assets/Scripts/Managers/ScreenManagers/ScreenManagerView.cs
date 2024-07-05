using BeeGood.Managers.ARManagers;
using UnityEngine;
using UnityEngine.UI;

public class ScreenManagerView : MonoBehaviour
{
    [SerializeField] private GameObject mainCamera;
    [SerializeField] private GameObject arCamera;

    [SerializeField] private Button homeButton;
    [SerializeField] private Button regionButton;
    [SerializeField] private Button placesButton;
    [SerializeField] private Button mapButton;
    [SerializeField] private Button arButton;

    [Space] [SerializeField] private GameObject homeActive;
    [SerializeField] private GameObject placesActive;
    [SerializeField] private GameObject mapActive;
    [SerializeField] private GameObject arActive;
    [SerializeField] private GameObject fade;
    [SerializeField] private GameObject hud;
    [SerializeField] private GameObject hudButtons;
    [SerializeField] private Button backTourButton;

    [Space] [SerializeField] private HomeScreenManagerView homeScreen;
    [SerializeField] private RegionScreenManagerView regionScreen;
    [SerializeField] private PlacesScreenManagerView placesScreen;
    [SerializeField] private MapScreenManagerView mapScreen;
    [SerializeField] private ARCoreManagerView arCoreManagerView;

    [SerializeField] private ARInfoView arInfoView;
    [SerializeField] private GameObject arBoard;

    public static ScreenManagerView Instance { get; private set; }
    public string CurrentRegion { get; private set; }
    public string CurrentRegionTitle { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        regionButton.onClick.AddListener(OpenRegion);
        homeButton.onClick.AddListener(OpenHome);
        placesButton.onClick.AddListener(OpenPlaces);
        mapButton.onClick.AddListener(OpenMap);
        arButton.onClick.AddListener(OpenAR);
    }

    public void UpdateRegion(string regionName, string regionTitle)
    {
        CurrentRegion = regionName;
        CurrentRegionTitle = regionTitle;

        hud.SetActive(true);
        OpenHome();
    }

    public void PlacesHudOnTour(GameObject placeInfoScreen)
    {
        placesScreen.gameObject.SetActive(false);
        hudButtons.SetActive(false);
        placeInfoScreen.SetActive(false);

        backTourButton.gameObject.SetActive(true);
        backTourButton.onClick.RemoveAllListeners();
        backTourButton.onClick.AddListener(() =>
        {
            backTourButton.gameObject.SetActive(false);
            placeInfoScreen.SetActive(true);
            hudButtons.SetActive(true);
            placesScreen.gameObject.SetActive(true);
            TourManagerView.Instance.CloseTour();
        });
    }

    public void MapHudOnTour(GameObject locationInfoScreen)
    {
        mapScreen.gameObject.SetActive(false);
        hudButtons.SetActive(false);
        locationInfoScreen.SetActive(false);

        backTourButton.gameObject.SetActive(true);
        backTourButton.onClick.RemoveAllListeners();
        backTourButton.onClick.AddListener(() =>
        {
            backTourButton.gameObject.SetActive(false);
            locationInfoScreen.SetActive(true);
            hudButtons.SetActive(true);
            mapScreen.gameObject.SetActive(true);
            TourManagerView.Instance.CloseTour();
        });
    }

    private void OpenAR()
    {
        Clear();
        mainCamera.SetActive(false);
        arCamera.SetActive(true);

        arBoard.SetActive(true);
        hudButtons.SetActive(false);
        backTourButton.gameObject.SetActive(true);
        backTourButton.onClick.RemoveAllListeners();
        backTourButton.onClick.AddListener(() =>
        {
            backTourButton.gameObject.SetActive(false);
            hudButtons.SetActive(true);
            arInfoView.CloseInfo();
            arInfoView.gameObject.SetActive(false);
            arCoreManagerView.StopARSession();
            arBoard.SetActive(false);
            OpenMap();
        });

        arInfoView.Group.spacing = 10f;

        arActive.SetActive(true);
        arCoreManagerView.StartARSession();
    }

    public void UpdateARData(string type, string title, string description)
    {
        arInfoView.Group.spacing = 10f;
        arInfoView.Type.text = type;
        arInfoView.Title.text = title;
        arInfoView.Description.text = description;
        arInfoView.gameObject.SetActive(true);
    }

    public void OpenMap()
    {
        if (placesScreen.Places.Count <= 0)
        {
            OpenPlaces();
        }

        if (mapScreen.gameObject.activeSelf)
        {
            mapScreen.locationBlock.gameObject.SetActive(false);
            mapScreen.InitRegionMap();
            return;
        }

        Clear();
        mapScreen.gameObject.SetActive(true);
        mapScreen.ShowScreen(CurrentRegion, placesScreen.Places);
        mapActive.SetActive(true);
    }

    private void OpenRegion()
    {
        Clear();
        hud.SetActive(false);
        regionScreen.gameObject.SetActive(true);
    }

    private void OpenHome()
    {
        if (homeScreen.gameObject.activeSelf)
        {
            homeScreen.ClearOpenNew();
        }

        Clear();
        homeScreen.gameObject.SetActive(true);
        homeScreen.ShowScreen(CurrentRegion);
        homeActive.SetActive(true);
        fade.SetActive(true);
    }

    private void OpenPlaces()
    {
        Clear();
        placesScreen.gameObject.SetActive(true);
        placesScreen.ShowScreen(CurrentRegion);
        placesActive.SetActive(true);
    }

    private void Clear()
    {
        homeActive.SetActive(false);
        placesActive.SetActive(false);
        mapActive.SetActive(false);
        arActive.SetActive(false);
        fade.SetActive(false);

        mainCamera.SetActive(true);
        arCamera.SetActive(false);

        homeScreen.gameObject.SetActive(false);
        placesScreen.gameObject.SetActive(false);
        regionScreen.gameObject.SetActive(false);
        mapScreen.gameObject.SetActive(false);
    }
}