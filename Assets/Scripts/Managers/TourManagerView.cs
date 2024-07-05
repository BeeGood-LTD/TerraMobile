using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TourManagerView : MonoBehaviour
{
    public static TourManagerView Instance { get; private set; }
    public List<TourData> Tours { get; private set; } = new();
    public TourButtonView Prefab;
    
    private List<TourButtonView> buttons = new();
    private Transform camera;
    private Vector3 mousePos;
    private string currentLocation;
    private bool inTour;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        camera = Camera.main!.gameObject.transform;
    }

    public void CloseTour()
    {
        inTour = false;
        RenderSettings.skybox.mainTexture = null;
        foreach (var button in buttons)
        {
            Destroy(button.gameObject);
        }
        
        buttons.Clear();
    }
    
    public void UpdateTourData(string location)
    {
        Tours.Clear();
        currentLocation = null;

        currentLocation = location;
        Tours = FileExtensions.GetJsonData<TourData>(PathConstants.TourData(currentLocation));
    }
    
    public void SetSkyboxTexture(TourData tourData, bool activate = false)
    {
        inTour = true;
        StopAllCoroutines();
        StartCoroutine(activate ? ActivateSkybox(tourData) : ChangeSkybox(tourData));
    }

    public TourData GetTourData(int index)
    {
        foreach (var tour in Tours)
        {
            if (tour.Index == index)
            {
                return tour;
            }
        }
        
        Debug.LogError($"Tour with index {index} not found!");
        return null;
    }
    
    private IEnumerator ChangeSkybox(TourData tourData)
    {
        var texture = FileExtensions.GetTexture($"{PathConstants.TourPath(currentLocation)}/{tourData.ImagePath}");
        var startValue = RenderSettings.skybox.GetFloat("_Exposure");
        yield return StartCoroutine(Interpolate(0.25f, startValue, 0.0f, UpdateExposureCallback));

        RenderSettings.skybox.mainTexture = texture;
        SpawnTourButton(tourData);

        startValue = RenderSettings.skybox.GetFloat("_Exposure");
        yield return StartCoroutine(Interpolate(0.25f, startValue, 1.0f, UpdateExposureCallback));
    }
    
    private IEnumerator ActivateSkybox(TourData tourData)
    {
        var texture = FileExtensions.GetTexture($"{PathConstants.TourPath(currentLocation)}/{tourData.ImagePath}");
        UpdateExposureCallback(0);
        
        RenderSettings.skybox.mainTexture = texture;
        SpawnTourButton(tourData);

        var startValue = RenderSettings.skybox.GetFloat("_Exposure");
        yield return StartCoroutine(Interpolate(0.5f, startValue, 1.0f, UpdateExposureCallback));
    }

    private void SpawnTourButton(TourData tourData)
    {
        foreach (var button in buttons)
        {
            Destroy(button.gameObject);
        }
        
        buttons.Clear();
        var indexes = tourData.NextIndex.ToArray();
        var angles = tourData.NextAngle.ToArray();

        for (var i = 0; i < indexes.Length; i++)
        {
            var tourButton = Instantiate(Prefab);
            tourButton.transform.position = SpawnPoint(angles[i]);
            tourButton.Data = GetTourData(indexes[i]);
            buttons.Add(tourButton);
        }
    }
    
    private void UpdateExposureCallback(float value)
    {
        RenderSettings.skybox.SetFloat("_Exposure", value);
    }
    
    private IEnumerator Interpolate(float targetTime, float startValue, float endValue, Action<float> action)
    {
        float lerpTime = 0.0f;

        while(lerpTime < targetTime)
        {
            lerpTime += Time.deltaTime;

            float percentage = lerpTime / targetTime;
            float finalValue = Mathf.Lerp(startValue, endValue, percentage);

            if (action != null) action.Invoke(finalValue);

            yield return null;
        }
    }
    
    private Vector3 SpawnPoint(float angle)
    {
        var randAng = angle * Mathf.Deg2Rad;
 
        var x = Mathf.Sin(randAng) * 10;
        var z = Mathf.Cos(randAng) * 10;
            
        return Vector3.zero + new Vector3(x, 0, z);
    }
    
    private void Update()
    {
        if (inTour == false)
        {
            return;
        }
        
        if (Input.GetMouseButtonDown(0))
        {
            mousePos = Input.mousePosition;
        }
	    
        if (Input.GetMouseButton(0) == false)
        {
            return;
        }
	    
        var currentMousePosition = Input.mousePosition;
        var direction =  currentMousePosition - mousePos;
        var dirMove = new Vector3(-direction.y, direction.x, 0);

        var speed = dirMove.magnitude / 4;
        if (speed > 100f)
        {
            speed = 100f;
        }

        if (camera.eulerAngles.x is > 180 and < 360 and < 320)
        {
            if (dirMove.x < 0)
            {
                dirMove.x = 0;
            }
        }
        if (camera.eulerAngles.x is < 90 and > 0 and > 45)
        {
            if (dirMove.x > 0)
            {
                dirMove.x = 0;
            }
        }
        
        camera.eulerAngles += dirMove.normalized * speed * Time.deltaTime;
    }
}