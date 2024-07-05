using UnityEngine;

public static class PathConstants
{
    public static string ApplicationPath()
    {
#if UNITY_EDITOR
        return "C:/TerraMobile";
#endif
        return Application.persistentDataPath;
    }

    //HOME
    public static string HomePath() => $"{ApplicationPath()}/{ScreenManagerView.Instance.CurrentRegion}/Home";
    public static string HomeHead() => $"{HomePath()}/Head.png";
    public static string HomeData() => $"{HomePath()}/News.json";
    
    //PLACES
    public static string PlacesPath() => $"{ApplicationPath()}/{ScreenManagerView.Instance.CurrentRegion}/Places";
    public static string PlacesData() => $"{PlacesPath()}/Places.json";
    public static string PlaceLocationPath(string location) => $"{PlacesPath()}/{location}";
    public static string LocationsData(string location) => $"{PlaceLocationPath(location)}/Locations.json";
    
    //TOUR
    public static string TourPath(string location) => $"{PlaceLocationPath(location)}/Tours";
    public static string TourData(string location) => $"{TourPath(location)}/Tours.json";
    
    //MAP
    public static string MapPath() => $"{ApplicationPath()}/{ScreenManagerView.Instance.CurrentRegion}/Map";
    public static string MapData() => $"{MapPath()}/Map.json";
    public static string MapLocationPath(string location) => $"{PlaceLocationPath(location)}/Map";
    public static string MapLocationData(string location) => $"{MapLocationPath(location)}/Map.json";

    //REGIONS
    public static string DownloadedRegions() => $"{ApplicationPath()}/DownloadedRegions.json";
    public static string AvailableRegions() => "AvailableRegions.json";
    
    //AR
    public static string ARPath() => $"{ApplicationPath()}/Bashkortostan/AR";
    public static string ARData() => $"{ARPath()}/AR.json";
    
    //GALLERY
    public static string GalleryPath(string location) => $"{PlaceLocationPath(location)}/Gallery";
    public static string GalleryData(string location) => $"{GalleryPath(location)}/Gallery.json";
}