using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using BeeGood.Plugins.AWSCloud.Data;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace BeeGood
{
    public class CloudDownloadManagerView : MonoBehaviour
    {
        public Slider ProgressSlider;

        private AmazonS3Config s3Config;
        private AmazonS3Client s3Client;

        private int downloadValue = 0;

        private void Awake()
        {
            s3Config = new AmazonS3Config()
            {
                ServiceURL = AuthenticationConstants.ServiceURL,
                AuthenticationRegion = "ru-central1"
            };
            AWSCredentials credentials = new BasicAWSCredentials(AuthenticationConstants.AccessKey, AuthenticationConstants.SecretKey);

            s3Client = new AmazonS3Client(credentials, s3Config);
        }

        public async UniTask<string> DownloadObjectFromBucketAsync(string objectName, string filePath, bool isRegion = false, Action<string> onDownloaded = null)
        {
            var request = new GetObjectRequest
            {
                BucketName = AuthenticationConstants.BucketName,
                Key = objectName
            };

            var dataPath = Path.Combine(filePath, objectName);
            Debug.Log($"Path: {dataPath}");

            using var response = await s3Client.GetObjectAsync(request);

            try
            {
                Debug.Log($"ObjectName: {response.Key} -- TotalBytes: {response.ContentLength}");

                if (isRegion)
                {
                    response.WriteObjectProgressEvent += OnDownloadValueUpdate;
                    StartCoroutine(UpdateSliderValueCoroutine());
                }

                await response.WriteResponseStreamToFileAsync(dataPath, true, CancellationToken.None);

                if (isRegion)
                {
                    StopCoroutine(UpdateSliderValueCoroutine());
                    downloadValue = 0;
                    ProgressSlider = null;
                }

                onDownloaded?.Invoke(dataPath);
                Debug.Log($"Downloaded: {objectName}");
                return dataPath;
            }
            catch (Exception e)
            {
                Debug.LogError($"Error saving {objectName}: {e.Message}");
            }

            return null;
        }

        private IEnumerator UpdateSliderValueCoroutine()
        {
            while (ProgressSlider.value < 99)
            {
                ProgressSlider.value = downloadValue;
                yield return new WaitForEndOfFrame();
            }

            ProgressSlider.value = 100;
        }

        private void OnDownloadValueUpdate(object sender, WriteObjectProgressArgs e)
        {
            downloadValue = e.PercentDone;
        }
    }
}