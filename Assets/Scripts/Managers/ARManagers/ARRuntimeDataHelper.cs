using System;
using System.Collections.Generic;
using System.IO;
using Cysharp.Threading.Tasks;
using Data;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;


namespace BeeGood.Managers.ARManagers
{
    public class ARRuntimeDataHelper : MonoBehaviour
    {
        private Dictionary<string, ReferenceImage> deserializedImages = new();
        private Dictionary<string, ModelData> deserializedModel = new();

        public async UniTask<RuntimeReferenceImageLibrary> FillRuntimeLibrary(RuntimeReferenceImageLibrary library)
        {
            deserializedImages.Clear();
            deserializedModel.Clear();
            
            var arPath = PathConstants.ARPath();
            var arDataPath = PathConstants.ARData();
            var arData = FileExtensions.GetJsonData<ARObjectData>(arDataPath);
            
            deserializedImages = await DeserializeARImages(arPath, arData);
            deserializedModel = DeserializeARModels(arPath, arData);
                
            if (Application.platform == RuntimePlatform.Android)
            {
                var mutableLibrary = library as MutableRuntimeReferenceImageLibrary;

                foreach (var kv in deserializedImages)
                {
                    kv.Value.AddJobState = mutableLibrary.ScheduleAddImageWithValidationJob(kv.Value.MainTexture, kv.Key, kv.Value.With);
                }

                library = mutableLibrary;
                
                if (await WaitUntilAllImagesValidated() == false)
                {
                    return null;
                }
            }

            Debug.LogWarning($"Images Status Add -----");
            foreach (var kv in deserializedImages)
            {
                Debug.Log($"ImageName: {kv.Value.ImageName} -- Status: {kv.Value.AddJobState.status}");
            }
            
            return library;
        }

        public ModelData GetModelData(string key)
        {
            return deserializedModel.GetValueOrDefault(key);
        }

        private async UniTask<bool> WaitUntilAllImagesValidated()
        {
            var timeoutController = new TimeoutController();

            bool isAllValidated = true;
            foreach (var image in deserializedImages)
            {
                try
                {
                    await UniTask.WaitUntil(() => image.Value.AddJobState.status == AddReferenceImageJobStatus.Success, 
                    PlayerLoopTiming.Update, timeoutController.Timeout(TimeSpan.FromSeconds(5)));
                    
                    timeoutController.Reset();
                }
                catch (OperationCanceledException operationException)
                {
                    Debug.LogError($"Timeout exception when validate image texture for AR Session. {operationException}");
                    isAllValidated = false;
                    break;
                }
            }
            
            Debug.LogWarning("Successful validate All Images for AR Session.");

            return isAllValidated;
        }
        
        private UniTask<Dictionary<string, ReferenceImage>> DeserializeARImages(string arDataPath, List<ARObjectData> arData)
        {
            Dictionary<string, ReferenceImage> images = new();

            foreach (var data in arData)
            {
                var texturePath = Path.Combine(arDataPath, data.ImagePath);
                var imageTexture = FileExtensions.GetTexture(texturePath);
                Debug.Log($"GetTexture from image: {texturePath}; {imageTexture}");
                if (imageTexture == null)
                {
                    Debug.LogError($"Null texture");
                }
                ReferenceImage rImage = new ReferenceImage(imageTexture, data.With, data.Name);

                if (!images.TryAdd(data.Name, rImage))
                {
                    images[data.Name] = rImage;
                }
            }

            return UniTask.FromResult(images);
        }

        private Dictionary<string, ModelData> DeserializeARModels(string arDataPath, List<ARObjectData> arData)
        {
            Dictionary<string, ModelData> models = new();
            
            foreach (var data in arData)
            {
                ModelData mData = new ModelData(data.Name, data.Type, data.Localization,data.Description, data.ModelPath);

                if (!models.TryAdd(data.Name, mData))
                {
                    models[data.Name] = mData;
                }
            }

            return models;
        }
    }
    
    public class ReferenceImage
    {
        public Texture2D MainTexture { get; private set; }
        public float With { get; private set; }
        public string ImageName { get; private set; }
        public AddReferenceImageJobState AddJobState { get; set; }

        public bool IsAdded;
        
        public ReferenceImage(Texture2D mainTexture, float with, string imageName)
        {
            MainTexture = mainTexture;
            With = with;
            ImageName = imageName;
        }
    }

    public class ModelData
    {
        public string Name { get; private set; }
        public string Type { get; private set; }
        public string Localization { get; private set; }
        public string Description { get; private set; }
        public string ModelPath { get; private set; }

        public ModelData(string name, string type, string localization, string description, string modelPath)
        {
            Name = name;
            Type = type;
            Localization = localization;
            ModelPath = modelPath;
            Description = description;
        }
    }
}