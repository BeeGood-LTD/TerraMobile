using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace BeeGood.Managers.ARManagers
{
    public class ARImageManagerView : MonoBehaviour
    {
        [SerializeField] private ARTrackedImageManager trackedImageManager;
        [SerializeField] private ARRuntimeDataHelper arDataHelper;
        [SerializeField] private ARDynamicPrefabManagerView dynamicPrefabManager;
        [SerializeField] private XRReferenceImageLibrary referenceImageLibrary;
#if UNITY_EDITOR
        [SerializeField] private XRReferenceImageLibrary TestUnityEditorReferenceImageLibrary;
#endif

        public async UniTask<bool> TryInit()
        {
            RuntimeReferenceImageLibrary newLibrary = null;

            if (Application.platform == RuntimePlatform.Android)
            {
                if (IsSupportMutableLibraries() == false)
                {
                    Debug.Log("Mutable Libraries NOT SUPPORTED!");
                    return false;
                }

                newLibrary = trackedImageManager.CreateRuntimeLibrary(referenceImageLibrary);
            }
#if UNITY_EDITOR
            else
            {
                newLibrary = trackedImageManager.CreateRuntimeLibrary(TestUnityEditorReferenceImageLibrary);
            }
#endif
            var runtimeLibrary = await arDataHelper.FillRuntimeLibrary(newLibrary);

            Debug.Log("Setting new library..");
            trackedImageManager.referenceLibrary = runtimeLibrary;

            StartImageTracking();
            
            return true;
        }

        public void StartImageTracking()
        {
            trackedImageManager.trackedImagesChanged += dynamicPrefabManager.OnTrackedImagesVisible;
            SetTracking(true);
        }

        public void StopImageTracking()
        {
            Debug.LogWarning("Stop Image Tracking");
            trackedImageManager.trackedImagesChanged -= dynamicPrefabManager.OnTrackedImagesVisible;
            SetTracking(false);
        }

        public void SetTracking(bool isEnable)
        {
            trackedImageManager.enabled = isEnable;
        }

        public bool IsSupportMutableLibraries()
        {
            return trackedImageManager.descriptor.supportsMutableLibrary;
        }
    }
}