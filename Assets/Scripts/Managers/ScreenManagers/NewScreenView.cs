using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NewScreenView : MonoBehaviour
{
    public Image Image;
    public TextMeshProUGUI Data;
    public TextMeshProUGUI Title;
    public TextMeshProUGUI Info;
    public RectTransform Сontent;
    public RectTransform Group;

    private void Update() => Сontent.sizeDelta = new Vector2(0, Group.rect.height + 250f);
}