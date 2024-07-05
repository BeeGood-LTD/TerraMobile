using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using BeeGood;
using Cysharp.Threading.Tasks;
using Data;
using UnityEngine;

public class RegionScreenManagerView : MonoBehaviour
{
    [SerializeField] private CloudDownloadManagerView cloudDownloadManagerView;
    [SerializeField] private RegionBlockView regionBlockPrefab;
    [SerializeField] private Transform contentParent;
    
    private List<RegionData> regionsData = new();
    private List<RegionData> localRegions = new();
    private List<RegionBlockView> regions = new();


    private async void Start()
    {
        if (File.Exists(PathConstants.DownloadedRegions()))
        {
            localRegions = FileExtensions.GetJsonData<RegionData>(PathConstants.DownloadedRegions());
        }

        var availableRegionsPath = await cloudDownloadManagerView.DownloadObjectFromBucketAsync(PathConstants.AvailableRegions(), PathConstants.ApplicationPath(), false);
        if (availableRegionsPath == null)
        {
            Debug.LogError("Cannot download Available Regions!");
        }
        
        regionsData = FileExtensions.GetJsonData<RegionData>(availableRegionsPath);
        OnDeleteFile(availableRegionsPath);
        InitRegions();
    }

    private void InitRegions()
    {
        foreach (var region in regionsData)
        {
            var item = Instantiate(regionBlockPrefab, contentParent);
            item.SetRegionData(region);

            item.TitleText.text = region.Localization;

            item.DeleteContentButton.onClick.AddListener(() => ClearRegion(item));
            item.UpdateContentButton.onClick.AddListener(() => OnDownloadRegion(item));
            item.DownloadContentButton.onClick.AddListener(() => OnDownloadRegion(item));
            item.OpenContentButton.onClick.AddListener(() => ScreenManagerView.Instance.UpdateRegion(item.RegionData.Name, region.Localization));

            foreach (var local in localRegions)
            {
                if (local.Name != region.Name)
                {
                    continue;
                }
                
                item.DownloadContentButton.gameObject.SetActive(false);
                item.DeleteContentButton.gameObject.SetActive(true);
                item.OpenContentButton.gameObject.SetActive(true);
                    
                if (local.Version < region.Version)
                {
                    item.UpdateContentButton.gameObject.SetActive(true);
                }
            }

            regions.Add(item);
        }
    }

    private async void OnDownloadRegion(RegionBlockView region)
    {
        Directory.CreateDirectory(PathConstants.ApplicationPath());

        region.ProgressSlider.gameObject.SetActive(true);
        region.DownloadContentButton.gameObject.SetActive(false);

        cloudDownloadManagerView.ProgressSlider = region.ProgressSlider;
        var downloadPath = await cloudDownloadManagerView.DownloadObjectFromBucketAsync($"{region.RegionData.Name}.zip", PathConstants.ApplicationPath(), true);
        var extractResult = await ExtractArchiveAsync(downloadPath, PathConstants.ApplicationPath());

        var isUpdate = false;
        foreach (var local in localRegions)
        {
            if (local.Name == region.RegionData.Name)
            {
                local.Version = region.RegionData.Version;
                isUpdate = true;
                break;
            }
        }

        if (isUpdate == false)
        {
            localRegions.Add(region.RegionData);
        }
            
        FileExtensions.SetJsonData(PathConstants.DownloadedRegions(), localRegions);
        region.ProgressSlider.gameObject.SetActive(false);
        region.DeleteContentButton.gameObject.SetActive(true);
        region.OpenContentButton.gameObject.SetActive(true);
        region.UpdateContentButton.gameObject.SetActive(false);
    }

    private async UniTask<bool> ExtractArchiveAsync(string filePath, string directory)
    {
        try
        {
            await Task.Run(() => ZipFile.ExtractToDirectory(filePath, directory));
            OnDeleteFile(filePath);
        }
        catch (Exception e)
        {
            Debug.LogError($"Error extract archive with filePath: {filePath} in directory: {directory} with message {e}");
            return false;
        }
        
        return true;
    }

    private void ClearRegion(RegionBlockView region)
    {
        foreach (var local in localRegions)
        {
            if (local.Name == region.RegionData.Name)
            {
                localRegions.Remove(local);
                FileExtensions.SetJsonData(PathConstants.DownloadedRegions(), localRegions);
                break;
            }
        }
        
        region.DeleteContentButton.gameObject.SetActive(false);
        region.UpdateContentButton.gameObject.SetActive(false);
        region.OpenContentButton.gameObject.SetActive(false);
        region.ProgressSlider.gameObject.SetActive(false);

        region.DownloadContentButton.gameObject.SetActive(true);
        OnDeleteDirectory($"{PathConstants.ApplicationPath()}/{region.RegionData.Name}");
    }

    private void OnDeleteFile(string filePath)
    {
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
        else
        {
            Debug.LogError($"File path '{filePath}' was not founded");
        }
    }

    private void OnDeleteDirectory(string directoryPath)
    {
        if (Directory.Exists(directoryPath))
        {
            Directory.Delete(directoryPath, true);
        }
        else
        {
            Debug.LogError($"Directory path '{directoryPath}' was not founded");
        }
    }
}