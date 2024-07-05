using UnityEngine;

public class TourButtonView : MonoBehaviour
{
    public TourData Data;

    private void Start()
    {
        transform.LookAt(Vector3.zero);
    }

    private void OnMouseDown()
    {
        TourManagerView.Instance.SetSkyboxTexture(Data);
    }
}