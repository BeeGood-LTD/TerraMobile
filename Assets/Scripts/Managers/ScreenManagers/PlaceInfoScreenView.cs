using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class PlaceInfoScreenView : MonoBehaviour
{
    [SerializeField] private Button tourButton;
    [SerializeField] private Button mapButton;
    [SerializeField] private Button backButton;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI placeText;
    [SerializeField] private TextMeshProUGUI infoText;
    [SerializeField] private RectTransform content;
    [SerializeField] private RectTransform group;
    [SerializeField] private VideoPlayer videoClip;
    
    private void Update() => content.sizeDelta = new Vector2(0, group.rect.height + 20f);
    private void OnClose() => Destroy(gameObject);

    public void UpdateData(string namePlace, string title, string place, string info, string videoPath)
    {
        var tourManager = TourManagerView.Instance;
        var locationManager = LocationsManagerView.Instance;
        locationManager.UpdateLocationData(namePlace);
        tourManager.UpdateTourData(namePlace);

        var tourData = tourManager.GetTourData(locationManager.Locations[0].Index);
        tourButton.onClick.AddListener(() =>
        {
            tourManager.SetSkyboxTexture(tourData, true);
            ScreenManagerView.Instance.PlacesHudOnTour(gameObject);
        });
        mapButton.onClick.AddListener(() =>
        {
            OnClose();
            ScreenManagerView.Instance.OpenMap();
            MapScreenManagerView.Instance.InitPlaceMap(namePlace);
        });
        backButton.onClick.AddListener(OnClose);

        videoClip.url = $"{PathConstants.PlaceLocationPath(namePlace)}/{videoPath}";
        titleText.text = title;
        placeText.text = place;
        infoText.text = info;
    }
}