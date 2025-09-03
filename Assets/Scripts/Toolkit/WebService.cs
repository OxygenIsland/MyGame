/*
┌────────────────────────────────────────────────────────┐
│	Author：Jeff
│	Description：
│
│
└────────────────────────────────────────────────────────┘
*/


using StarWorld.Common.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace StarWorld.Common
{
    public static class WebService
    {
        public static void GetWebRequestPostResponse(string url, byte[] post_data, Dictionary<string, string> headers, Action<string, UnityWebRequest> callback, Action<string, UnityWebRequest> onError = null)
        {
            CoroutineHelper.Instance.AddCoroutine(GetWebRequestPostResponseIEnumerator(url, post_data, headers, callback, onError));
        }

        public static IEnumerator GetWebRequestPostResponseIEnumerator(string url, byte[] post_data, Dictionary<string, string> headers, Action<string, UnityWebRequest> callback, Action<string, UnityWebRequest> onError = null)
        {
            using (UnityWebRequest webRequest = new UnityWebRequest(url, "POST"))
            {
                webRequest.useHttpContinue = false;
                webRequest.uploadHandler = new UploadHandlerRaw(post_data);
                webRequest.downloadHandler = new DownloadHandlerBuffer();
                //webRequest.certificateHandler = new WebRequestCert();
                webRequest.timeout = 10;    //单位：秒

                if (headers != null)
                {
                    foreach (var h in headers)
                    {
                        webRequest.SetRequestHeader(h.Key, h.Value);
                    }
                }
                Debug.Log($"[GetWebRequestPostResponse  Post] {url}");
                yield return webRequest.SendWebRequest();
                if (webRequest.result == UnityWebRequest.Result.Success)
                {
                    callback?.Invoke(webRequest.downloadHandler.text, webRequest);
                }
                else if (webRequest.result == UnityWebRequest.Result.ProtocolError || webRequest.result == UnityWebRequest.Result.ConnectionError)
                {
                    Debug.LogError($"[GetWebRequestPostResponse] ProtocolError or ConnectionError error = {webRequest.error} url ={webRequest.url}");
                    onError?.Invoke(webRequest.error, webRequest);
                }
                else if (webRequest.result == UnityWebRequest.Result.DataProcessingError)
                {
                    Debug.LogError($"[GetWebRequestPostResponse] DataProcessingError error = {webRequest.error} url ={webRequest.url}");
                    onError?.Invoke(webRequest.error, webRequest);
                }
            }
        }
      
        public static void GetWebRequestGetResponse(string url, byte[] post_data, Dictionary<string, string> headers, Action<string, UnityWebRequest> callback, Action<string, UnityWebRequest> onError = null)
        {
            CoroutineHelper.Instance.AddCoroutine(GetWebRequestGetResponseIEnumerator(url, post_data, headers, callback, onError));
        }

        public static IEnumerator GetWebRequestGetResponseIEnumerator(string url, byte[] post_data, Dictionary<string, string> headers, Action<string, UnityWebRequest> callback, Action<string, UnityWebRequest> onError = null)
        {
            using (UnityWebRequest webRequest = new UnityWebRequest(url, "Get"))
            {
                webRequest.useHttpContinue = false;
                webRequest.uploadHandler = new UploadHandlerRaw(post_data);
                webRequest.downloadHandler = new DownloadHandlerBuffer();
                //webRequest.certificateHandler = new WebRequestCert();
                webRequest.timeout = 10;    //单位：秒

                if (headers != null)
                {
                    foreach (var h in headers)
                    {
                        webRequest.SetRequestHeader(h.Key, h.Value);
                    }
                }
                Debug.Log($"[GetWebRequestGetResponse  Get] {url}");
                yield return webRequest.SendWebRequest();
                if (webRequest.result == UnityWebRequest.Result.Success)
                {
                    callback?.Invoke(webRequest.downloadHandler.text, webRequest);
                }
                else if (webRequest.result == UnityWebRequest.Result.ProtocolError || webRequest.result == UnityWebRequest.Result.ConnectionError)
                {
                    Debug.LogError($"[GetWebRequestGetResponse] ProtocolError or ConnectionError error = {webRequest.error} url ={webRequest.url} text ={webRequest.downloadHandler.text}");
                    onError?.Invoke(webRequest.error, webRequest);
                }
                else if (webRequest.result == UnityWebRequest.Result.DataProcessingError)
                {
                    Debug.LogError($"[GetWebRequestGetResponse] DataProcessingError error = {webRequest.error} url ={webRequest.url} text ={webRequest.downloadHandler.text}");
                    onError?.Invoke(webRequest.error, webRequest);
                }
            }
        }

    }
}
