using Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RegionBlockView : MonoBehaviour
{
    public Button DownloadContentButton;
    public Button DeleteContentButton;
    public Button OpenContentButton;
    public Button UpdateContentButton;
    public TextMeshProUGUI TitleText;
    public Slider ProgressSlider;
    public RegionData RegionData { get; private set; }
    
    public void SetRegionData(RegionData data)
    {
        RegionData = data;
    }
}
