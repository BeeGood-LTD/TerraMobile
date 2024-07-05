using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

namespace BeeGood.Managers.ARManagers
{
    public class ARCoreManagerView : MonoBehaviour
    {
        [SerializeField] private ARSession arSession;
        [SerializeField] private ARInputManager arInputManager;
        
        [SerializeField] private ARImageManagerView arImageManagerView;
        
        public bool IsInitialized { get; private set; }
        public bool IsSupported { get; private set; }
        public bool IsRun { get; private set; }

        private void Awake()
        {
            SetSession(false);
            arImageManagerView.SetTracking(false);
        }
        
        public async void StartARSession()
        {
            if (arSession == null || arInputManager == null)
            {
                Debug.LogError($"Cannot find ARSession GameObject! Further launch has been stopped!");
                return;
            }
            
            Debug.Log($"Starting Initialize ARCore");
            IsInitialized = await TryInitCore();

            if (await WaitUntilSessionState(ARSessionState.SessionTracking) == false)
            {
                IsInitialized = false;
                return;
            }
            
            Debug.LogWarning("Session is FULL Initialized!");

            if (await arImageManagerView.TryInit() == false)
            {
                Debug.LogError("Device not support mutable Library in Runtime. AR does not work!");
                IsSupported = false;
                IsInitialized = false;
            }
        }

        private async UniTask<bool> TryInitCore()
        {
            Debug.Log("Start Check ARSession state");
            if (ARSession.state == ARSessionState.None ||
                ARSession.state == ARSessionState.CheckingAvailability)
            {
                await ARSession.CheckAvailability();
                Debug.Log("End Check Availability");
            }

            if (ARSession.state == ARSessionState.Unsupported)
            {
                IsSupported = false;
                return false;
            }
            
            if (ARSession.state > ARSessionState.CheckingAvailability)
            {
                IsSupported = true;
            }

            if (ARSession.state == ARSessionState.NeedsInstall)
            {
                await ARSession.Install();
                await ARSession.CheckAvailability();
            }

            if (ARSession.state <= ARSessionState.Installing)
            {
                return false;
            }
            
            Debug.Log($"ARSession state: {ARSession.state}");
            SetSession(true);
            
            return true;
        }

        public async void StopARSession()
        {
            if (await WaitUntilSessionState(ARSessionState.SessionTracking))
            {
                arImageManagerView.StopImageTracking();
                SetSession(false);
            }
        }

        public void ResetARSession()
        {
            Debug.Log("Reset AR Session.");
            arSession.Reset();
        }
        
        private async UniTask<bool> WaitUntilSessionState(ARSessionState state)
        {
            bool isAll;
            try
            {
                var cts = new CancellationTokenSource();
                cts.CancelAfterSlim(TimeSpan.FromSeconds(5));
                
                Debug.LogWarning("Start Wait Until Session is Tracking..");
                await UniTask.WaitUntil(() => ARSession.state == state, PlayerLoopTiming.Update, cts.Token);
                isAll = true;
            }
            catch (OperationCanceledException ex)
            {
                Debug.LogError($"Device cannot initialize completely. Initialization stopped by cancel time. {ex} ");
                isAll = false;
            }
            
            return isAll;
        }
        
        private void SetSession(bool isEnable)
        {
            Debug.LogWarning($"Set AR Session: {isEnable}");
            arSession.enabled = isEnable;
            //arInputManager.enabled = isEnable;
        }
    }
}