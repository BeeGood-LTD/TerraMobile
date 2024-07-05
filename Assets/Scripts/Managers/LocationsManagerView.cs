using System.Collections.Generic;
using UnityEngine;

public class LocationsManagerView : MonoBehaviour
{
     public static LocationsManagerView Instance { get; private set; }
     public List<LocationData> Locations { get; private set; } = new();

     private void Awake()
     {
          Instance = this;
     }

     public void UpdateLocationData(string location)
     {
          Locations.Clear();
          Locations = FileExtensions.GetJsonData<LocationData>(PathConstants.LocationsData(location));
     }
}