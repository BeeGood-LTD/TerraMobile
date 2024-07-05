using UnityEngine;

namespace BeeGood.Managers.ARManagers
{
    public class ARInspectionModelManagerView : MonoBehaviour
    {
        [SerializeField] private GameObject pointInspect;
        public bool IsInspect { get; private set; }
        
        private DynamicARObject currentInspectObj;
        private float rotationSpeed = 5f;
        
        public void StartInspect(DynamicARObject arObject)
        {
            currentInspectObj = arObject;
            arObject.SyncWithParent(pointInspect.transform);
            IsInspect = true;
        }

        public void StopInspect()
        {
            if (currentInspectObj != null)
            {
                currentInspectObj.DisableModel();
            }
            IsInspect = false;
        }
        
        private void Update()
        {
            if (IsInspect == false)
            {
                return;
            }

            if (Input.GetMouseButton(0))
            {
                float rotX = Input.GetAxis("Mouse X") * rotationSpeed * Mathf.Deg2Rad;
                float rotY = Input.GetAxis("Mouse Y") * rotationSpeed * Mathf.Deg2Rad;

                currentInspectObj.InstantiatedModel.transform.RotateAround(Vector3.up, -rotX);
                currentInspectObj.InstantiatedModel.transform.RotateAround(Vector3.right, rotY);
            }
        }
    }
}