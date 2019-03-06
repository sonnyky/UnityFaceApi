using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public static class RequestManager 
{

    public class PersonGroupData
    {
        public string name;
        public string userData;
    }

    static IEnumerator Post(string url, string bodyJsonString)
    {
        var request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(bodyJsonString);
        request.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        Debug.Log("Status Code: " + request.responseCode);
    }

    public static IEnumerator CreatePersonGroup(string personGroupId, string name, string userData, System.Action<bool> result) {
        string parameters = "?subscription-key=" + Constants.FACE_API_KEY_1;
        string request = Constants.FACE_API_ENDPOINT + "/persongroups/" + personGroupId + parameters;
        string body = "{'name': '" + name + "','userData': '" + userData + "'}";

        PersonGroupData data = new PersonGroupData();
        data.name = name;
        data.userData = userData;

        string dataJson = JsonUtility.ToJson(data);

        using (UnityWebRequest www = UnityWebRequest.Put(request, dataJson))
        {
            www.SetRequestHeader("Content-Type", "application/json");
            yield return www.SendWebRequest();

            if(www.isNetworkError || www.isHttpError)
            {
                Debug.Log(www.error);
                result(false);
            }
            else
            {
                Debug.Log("Person group created");
                result(true);
            }
        }

    }

    public static IEnumerator CreatePersonInGroup(string personGroupId, string name, string userData, System.Action<bool> result)
    {
        string request = Constants.FACE_API_ENDPOINT + "/persongroups/" + personGroupId +"/persons";

        PersonGroupData data = new PersonGroupData();
        data.name = name;
        data.userData = userData;

        string dataJson = JsonUtility.ToJson(data);

        var www = new UnityWebRequest(request, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(dataJson);
        www.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
        www.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
        www.SetRequestHeader("Content-Type", "application/json");
        www.SetRequestHeader("Ocp-Apim-Subscription-Key", Constants.FACE_API_KEY_1);

        yield return www.SendWebRequest();

        Debug.Log("Status Code: " + www.responseCode);

        if (www.isNetworkError)
        {
            Debug.Log(" Error: " + www.error);
            result(false);
        }
        else
        {
            if (!string.IsNullOrEmpty(www.error))
            {
                Debug.Log(www.error);
                result(false);
            }
            else
            {
                result(true);
            }
        }

    }

    public static IEnumerator GetPersonGroup(string personGroupId, System.Action<bool> result)
    {
        string parameters = "?subscription-key=" + Constants.FACE_API_KEY_1;
        string request = Constants.FACE_API_ENDPOINT + "/persongroups/" + personGroupId + parameters;

        using(UnityWebRequest www = UnityWebRequest.Get(request))
        {
            yield return www.SendWebRequest();

            if (www.isNetworkError)
            {
                Debug.Log(" Error: " + www.error);
                result(false);
            }
            else
            {
                Debug.Log(":\nReceived: " + www.downloadHandler.text);
                result(true);
            }
        }
    }

    /// <summary>
    /// Get a particular person in group with a known personId. If the personId is not known use the List API to get all persons instead.
    /// </summary>
    /// <param name="personGroupId"></param>
    /// <param name="personId"></param>
    /// <param name="result"></param>
    /// <returns></returns>
    public static IEnumerator GetPersonInGroup(string personGroupId, string personId, System.Action<bool> result)
    {
        string parameters = "?subscription-key=" + Constants.FACE_API_KEY_1;
        string request = Constants.FACE_API_ENDPOINT + "/persongroups/" + personGroupId + "/persons/" + personId + parameters;

        using (UnityWebRequest www = UnityWebRequest.Get(request))
        {
            yield return www.SendWebRequest();

            if (www.isNetworkError)
            {
                Debug.Log(" Error: " + www.error);
                result(false);
            }
            else
            {
                if (!string.IsNullOrEmpty(www.error))
                {
                    Debug.Log(www.error);
                    result(false);
                }
                else
                {
                    result(true);
                }
            }
        }
    }

    public static IEnumerator GetPersonListInGroup(string personGroupId, System.Action<bool> result)
    {
        string parameters = "?subscription-key=" + Constants.FACE_API_KEY_1;
        string request = Constants.FACE_API_ENDPOINT + "/persongroups/" + personGroupId + "/persons" + parameters;

        using (UnityWebRequest www = UnityWebRequest.Get(request))
        {
            yield return www.SendWebRequest();

            if (www.isNetworkError)
            {
                Debug.Log(" Error: " + www.error);
                result(false);
            }
            else
            {
                if (!string.IsNullOrEmpty(www.error))
                {
                    Debug.Log(www.error);
                    result(false);
                }
                else
                {
                    Debug.Log(":\nReceived: " + www.downloadHandler.text);
                    result(true);
                }
            }
        }

    }


    public static IEnumerator GetPersonGroupTrainingStatus(string personGroupId, System.Action<string> result)
    {
        string parameters = "?subscription-key=" + Constants.FACE_API_KEY_1;
        string request = Constants.FACE_API_ENDPOINT + "/persongroups/" + personGroupId + "/training" + parameters;

        using (UnityWebRequest www = UnityWebRequest.Get(request))
        {
            yield return www.SendWebRequest();

            if (www.isNetworkError)
            {
                result(www.error);
            }
            else
            {
                if (!string.IsNullOrEmpty(www.error))
                {
                    result(www.downloadHandler.text);
                }
                else
                {
                    result(www.downloadHandler.text);
                }
            }
        }
    }


    public static IEnumerator DetectFaces(string pathToImage, System.Action<string> result)
    {
        string request = Constants.FACE_API_ENDPOINT + "/detect";

        var www = new UnityWebRequest(request, "POST");
        byte[] bodyRaw = File.ReadAllBytes(pathToImage);
        www.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
        www.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
        www.SetRequestHeader("Content-Type", "application/octet-stream");
        www.SetRequestHeader("Ocp-Apim-Subscription-Key", Constants.FACE_API_KEY_1);

        yield return www.SendWebRequest();

        Debug.Log("Detect Faces Status Code: " + www.responseCode);

        if (www.isNetworkError)
        {
            result(www.error);
        }
        else
        {
            if (!string.IsNullOrEmpty(www.error))
            {
                Debug.Log(www.error);
                result(www.error);
            }
            else
            {
                result(www.downloadHandler.text);
            }
        }

    }

}
