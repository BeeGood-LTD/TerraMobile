using BeeGood.Managers.ARManagers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ARInfoView : MonoBehaviour
{
    [SerializeField] private ARInspectionModelManagerView arInspectionModelManagerView;
    [SerializeField] private ARImageManagerView arImageManagerView;
    [SerializeField] private ARCoreManagerView arCoreManager;
    public TextMeshProUGUI Type;
    public TextMeshProUGUI Title;
    public TextMeshProUGUI Description;
    public RectTransform Сontent;
    public RectTransform Element;
    public VerticalLayoutGroup Group;
    public GameObject InfoGroup;
    public Button OpenInfo;

    private void Start()
    {
        OpenInfo.onClick.AddListener(() =>
        {
            if (InfoGroup.activeSelf)
            {
                CloseInfo();
                return;
            }

            Сontent.localPosition = Vector3.zero;
            InfoGroup.SetActive(true);
        });
    }

    public void CloseInfo()
    {
        InfoGroup.SetActive(false);
        arInspectionModelManagerView.StopInspect();
        arCoreManager.ResetARSession();
        gameObject.SetActive(false);
    }
    
    private void Update()
    {
        if (!gameObject.activeSelf)
        {
            return;
        }
        
        Сontent.sizeDelta = new Vector2(0, Element.rect.height);
        Group.spacing = Mathf.Lerp(Group.spacing, 40f, 10 * Time.deltaTime);
    }
}