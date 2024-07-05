using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LocationInfoScreenView : MonoBehaviour
{
    public Image Head;
    public Image Media;
    public TextMeshProUGUI Title;
    public TextMeshProUGUI Info;
    public TextMeshProUGUI Description;
    public Button Close;
    public Button OpenInfo;
    public Button Tour;
    public Button Gallery;
    public RectTransform content;
    public RectTransform group;
    
    private void Update() => content.sizeDelta = new Vector2(0, group.rect.height + 20f);
}