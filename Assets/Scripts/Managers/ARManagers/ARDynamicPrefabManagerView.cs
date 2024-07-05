using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityGLTF;

namespace BeeGood.Managers.ARManagers
{
    public class ARDynamicPrefabManagerView : MonoBehaviour
    {
        [SerializeField] private ARRuntimeDataHelper arDataHelper;
        [SerializeField] private ARInspectionModelManagerView inspectionModelManagerView;
        [SerializeField] private ARImageManagerView arImageManagerView;
        private Dictionary<string, DynamicARObject> trackedObjects = new();
        private Dictionary<string, DynamicARObject> needInstant = new();

        public void OnTrackedImagesVisible(ARTrackedImagesChangedEventArgs eventArgs)
        {
            foreach (var newTrackedImage in eventArgs.added)
            {
                var referenceImageName = newTrackedImage.referenceImage.name;
                Debug.Log($"Added image: {referenceImageName}");

                if (trackedObjects.ContainsKey(referenceImageName))
                {
                    PostInstantiateTrackedObject(trackedObjects[referenceImageName]);
                    return;
                }
                
                if (needInstant.ContainsKey(referenceImageName))
                {
                    Debug.Log("Image has already added and wait loading model. Skipping image");
                    return;
                }

                var modelData = arDataHelper.GetModelData(referenceImageName);
                var modelPath = Path.Combine(PathConstants.ARPath(), modelData.ModelPath);
                DynamicARObject dObject = new DynamicARObject(referenceImageName, newTrackedImage, modelPath);

                needInstant.TryAdd(referenceImageName, dObject);
            }
        }

        private async void Update()
        {
            if (needInstant.Count > 0)
                foreach (var arObject in needInstant.Values.ToList())
                {
                    if (arObject.State > DynamicObjectState.NotLoaded)
                    {
                        return;
                    }

                    arObject.State = DynamicObjectState.Loading;

                    Debug.Log($"Loading GLTF model with name: {arObject.Name}..");
                    var createdObject = await LoadGltfModel(arObject.ModelPath);

                    if (createdObject == null)
                    {
                        Debug.LogError($"An error occurred while loading GLTF Model with path: {arObject.ModelPath}. Loading Model was skipped.");
                        continue;
                    }

                    var mainObject = new GameObject(arObject.Name);
                    createdObject.transform.position = mainObject.transform.position;
                    createdObject.transform.SetParent(mainObject.transform);

                    arObject.InstantiatedModel = mainObject;
                    arObject.State = DynamicObjectState.Loaded;
                    needInstant.Remove(arObject.Name);
                    trackedObjects.Add(arObject.Name, arObject);
                    PostInstantiateTrackedObject(arObject);
                }
        }

        private void PostInstantiateTrackedObject(DynamicARObject arObject)
        {
            Debug.Log("Sync Instantiated Object..");
            var modelData = arDataHelper.GetModelData(arObject.Name);
            ScreenManagerView.Instance.UpdateARData(modelData.Type, modelData.Localization, modelData.Description);
            inspectionModelManagerView.StartInspect(arObject);
        }

        private async UniTask<GameObject> LoadGltfModel(string filePath)
        {
            GLTFSceneImporter importer = new GLTFSceneImporter(filePath, new ImportOptions());
            await importer.LoadSceneAsync();

            if (importer.CreatedObject == null)
            {
                return default;
            }

            return importer.CreatedObject;
        }
    }

    public class DynamicARObject
    {
        public ARTrackedImage TrackedImage { get; private set; }
        public string Name { get; private set; }
        public string ModelPath { get; private set; }

        public DynamicObjectState State;

        public GameObject InstantiatedModel;

        public DynamicARObject(string name, ARTrackedImage referenceImage, string modelPath)
        {
            Name = name;
            TrackedImage = referenceImage;
            ModelPath = modelPath;

            State = DynamicObjectState.NotLoaded;
        }

        public void SyncWithParent(Transform parentPos)
        {
            InstantiatedModel.transform.position = parentPos.position;
            InstantiatedModel.transform.SetParent(parentPos);
            InstantiatedModel.SetActive(true);
            InstantiatedModel.transform.GetChild(0).gameObject.SetActive(true);
        }

        public void DisableModel()
        {
            InstantiatedModel.SetActive(false);
            InstantiatedModel.transform.GetChild(0).gameObject.SetActive(false);
        }
    }

    public enum DynamicObjectState
    {
        NotLoaded,
        Loading,
        Loaded
    }
}