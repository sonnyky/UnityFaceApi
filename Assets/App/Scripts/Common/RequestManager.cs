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

    public static IEnumerator CreatePersonGroup(string endpoint, string apiKey, string personGroupId, string name, string userData, System.Action<bool> result) {
        string parameters = "?subscription-key=" + apiKey;
        string request = endpoint + "/persongroups/" + personGroupId + parameters;
        string body = "{'name': '" + name + "','userData': '" + userData + "'}";

        PersonGroupData data = new PersonGroupData();
        data.name = name;
        data.userData = userData;

        string dataJson = JsonUtility.ToJson(data);

        using (UnityWebRequest www = UnityWebRequest.Put(request, dataJson))
        {
            www.SetRequestHeader("Content-Type", "application/json");
            www.SetRequestHeader("Ocp-Apim-Subscription-Key", apiKey);

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

    public static IEnumerator CreatePersonInGroup(string endpoint, string apiKey, string personGroupId, string name, string userData, 
        System.Action<bool> result , System.Action<PersonCreateSuccess.PersonCreateSuccessResponse> success)
    {
        string request = endpoint + "/persongroups/" + personGroupId +"/persons";

        PersonGroupData data = new PersonGroupData();
        data.name = name;
        data.userData = userData;

        string dataJson = JsonUtility.ToJson(data);

        var www = new UnityWebRequest(request, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(dataJson);
        www.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
        www.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
        www.SetRequestHeader("Content-Type", "application/json");
        www.SetRequestHeader("Ocp-Apim-Subscription-Key", apiKey);

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
                PersonCreateSuccess.PersonCreateSuccessResponse successValue = JsonUtility.FromJson<PersonCreateSuccess.PersonCreateSuccessResponse>(www.downloadHandler.text);
                result(true);
                success(successValue);
            }
        }

    }

    public static IEnumerator GetPersonGroup(string endpoint, string apiKey, string personGroupId, System.Action<bool> result)
    {
        string parameters = "?subscription-key=" + apiKey;
        string request = endpoint + "/persongroups/" + personGroupId + parameters;

        Debug.Log("Endpoint is : " + endpoint + " and key is : " + apiKey);


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

    /// <summary>
    /// Get a particular person in group with a known personId. If the personId is not known use the List API to get all persons instead.
    /// </summary>
    /// <param name="personGroupId"></param>
    /// <param name="personId"></param>
    /// <param name="result"></param>
    /// <returns></returns>
    public static IEnumerator GetPersonInGroup(string endpoint, string apiKey, string personGroupId, string personId, System.Action<bool> result)
    {
        string parameters = "?subscription-key=" + apiKey;
        string request = endpoint + "/persongroups/" + personGroupId + "/persons/" + personId + parameters;

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

    public static IEnumerator GetPersonListInGroup(
        string endpoint, string apiKey, string personGroupId, 
        System.Action<bool> result, System.Action<List<PersonInGroup.Person>> personList)
    {
        List<PersonInGroup.Person> temp = new List<PersonInGroup.Person>();
        string parameters = "?subscription-key=" + apiKey;
        string request = endpoint + "/persongroups/" + personGroupId + "/persons" + parameters;

        using (UnityWebRequest www = UnityWebRequest.Get(request))
        {
            yield return www.SendWebRequest();

            if (www.isNetworkError)
            {
                result(false);
                personList(temp);
            }
            else
            {
                if (!string.IsNullOrEmpty(www.error))
                {
                    Debug.Log(www.error);
                    result(false);
                    personList(temp);
                }
                else
                {
                    string personListString = www.downloadHandler.text;
                    if (personListString != null)
                    {
                        PersonInGroup.Person[] arrayOfPersons = Tinker.JsonHelper.getJsonArray<PersonInGroup.Person>(personListString);
                        if(arrayOfPersons.Length > 0)
                        {
                            result(true);
                            temp.AddRange(arrayOfPersons);
                            personList(temp);
                        }
                        else { result(true); personList(temp); }
                    }
                }
            }
        }
    }


    public static IEnumerator GetPersonGroupTrainingStatus(string endpoint, string apiKey, string personGroupId, 
        System.Action<bool> result, System.Action<Error.ErrorResponse> err, System.Action<Training.TrainingStatus> status)
    {
        string parameters = "?subscription-key=" + apiKey;
        string request = endpoint + "/persongroups/" + personGroupId + "/training" + parameters;
        Error.ErrorResponse errorResponse = new Error.ErrorResponse();
        Training.TrainingStatus statusResponse = new Training.TrainingStatus();
        using (UnityWebRequest www = UnityWebRequest.Get(request))
        {
            yield return www.SendWebRequest();

            if (www.isNetworkError)
            {
                errorResponse.error.message = "Network error";
                errorResponse.error.code = "Network";
                err(errorResponse);
                result(false);
            }
            else
            {
                if (!string.IsNullOrEmpty(www.error))
                {
                    // TODO: Catch other errors too
                    errorResponse = JsonUtility.FromJson<Error.ErrorResponse>(www.error);
                    err(errorResponse);
                    result(false);
                }
                else
                {
                    statusResponse = JsonUtility.FromJson<Training.TrainingStatus>(www.downloadHandler.text);
                    status(statusResponse);
                    result(true);
                }
            }
        }
    }


    public static IEnumerator DetectFaces(string endpoint, string apiKey, string pathToImage, 
        System.Action<bool> result, System.Action<List<FacesBasic.FacesDetectionResponse>> data)
    {
        string request = endpoint + "/detect";

        var www = new UnityWebRequest(request, "POST");
        byte[] bodyRaw = File.ReadAllBytes(pathToImage);
        www.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
        www.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
        www.SetRequestHeader("Content-Type", "application/octet-stream");
        www.SetRequestHeader("Ocp-Apim-Subscription-Key", apiKey);

        yield return www.SendWebRequest();

        Debug.Log("Detect Faces Status Code: " + www.responseCode);

        if (www.isNetworkError)
        {
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
                FacesBasic.FacesDetectionResponse[] face = Tinker.JsonHelper.getJsonArray<FacesBasic.FacesDetectionResponse>(www.downloadHandler.text);
                List<FacesBasic.FacesDetectionResponse> temp = new List<FacesBasic.FacesDetectionResponse>();
                temp.AddRange(face);
                data(temp);
                result(true);
            }
        }

    }

    public static IEnumerator DeletePersonInGroup(string endpoint, string apiKey, string personGroupId, string personId, System.Action<bool> result, System.Action<string> error)
    {
        string request = endpoint + "/persongroups/" + personGroupId + "/persons/" + personId;
        var www = new UnityWebRequest(request, "DELETE");
        www.SetRequestHeader("Ocp-Apim-Subscription-Key", apiKey);
        yield return www.SendWebRequest();

        if (www.isNetworkError)
        {
            error(www.error);
            result(false);
        }
        else
        {
            if (!string.IsNullOrEmpty(www.error))
            {
                error(www.error);
                result(false);
            }
            else
            {
                error(www.downloadHandler.text);
                result(true);
            }
        }

    }

    public static IEnumerator AddFaceToPersonInGroup(string endpoint, string apiKey, string personGroupId, string personId,string pathToImage, string targetFace,System.Action<bool> result, System.Action<string> persistedFaceID)
    {
        string request = endpoint + "/persongroups/" + personGroupId + "/persons/" + personId + "/persistedFaces" ;
        
        var www = new UnityWebRequest(request, "POST");
        byte[] bodyRaw = File.ReadAllBytes(pathToImage);
        www.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
        www.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
        www.SetRequestHeader("Content-Type", "application/octet-stream");
        www.SetRequestHeader("Ocp-Apim-Subscription-Key", apiKey);

        yield return www.SendWebRequest();

        if (www.isNetworkError)
        {
            Debug.Log("AddFaceToPersonInGroup error : " + www.error);
            result(false);
        }
        else
        {
            if (!string.IsNullOrEmpty(www.error))
            {
                Debug.Log("AddFaceToPersonInGroup error : " + www.error);
                result(false);
            }
            else
            {
                result(true);
                persistedFaceID(www.downloadHandler.text);
            }
        }

    }


    public static IEnumerator TrainPersonGroup(string endpoint, string apiKey, string personGroupId, System.Action<bool> result, System.Action<string> error)
    {
        string request = endpoint + "/persongroups/" + personGroupId + "/train";

        var www = new UnityWebRequest(request, "POST");
       
        www.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
        www.SetRequestHeader("Ocp-Apim-Subscription-Key", apiKey);

        yield return www.SendWebRequest();

        if (www.isNetworkError)
        {
            error(www.error);
            result(false);
        }
        else
        {
            if (!string.IsNullOrEmpty(www.error))
            {
                error(www.error);
                result(false);
            }
            else
            {
                error(www.downloadHandler.text);
                result(true);
            }
        }

    }

    /// <summary>
    /// This method only supports PersonGroupId
    /// </summary>
    /// <param name="personGroupId"></param>
    /// <param name="faceIds"></param>
    /// <param name="result"></param>
    /// <returns></returns>
    public static IEnumerator Identify(string endpoint, string apiKey, string personGroupId, FacesBasic.FacesDetectionResponse[] faces, 
        System.Action<bool> identified,System.Action<string> error, System.Action<List<IdentifiedFaces.IdentifiedFacesResponse>> candidates)
    {
        string request = endpoint + "/identify";


        // Send the detected faces to identify agains the trained images
        Identify.IdentifyTarget identify = new Identify.IdentifyTarget();
        identify.faceIds = new string[faces.Length];
        identify.personGroupId = personGroupId;
        for (int k = 0; k < faces.Length; k++)
        {
            identify.faceIds[k] = faces[k].faceId;
        }
        identify.maxNumOfCandidatesReturned = 1;
        identify.confidenceThreshold = 0.5f;

        string dataJson = JsonUtility.ToJson(identify);

        Debug.Log("dataJson in Identify");
        Debug.Log(dataJson);

        var www = new UnityWebRequest(request, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(dataJson);
        www.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
        www.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
        www.SetRequestHeader("Content-Type", "application/json");
        www.SetRequestHeader("Ocp-Apim-Subscription-Key", apiKey);

        yield return www.SendWebRequest();

        if (www.isNetworkError)
        {
            identified(false);
            error(www.error);
        }
        else
        {
            if (!string.IsNullOrEmpty(www.error))
            {
                identified(false);
                error(www.error);
            }
            else
            {
                IdentifiedFaces.IdentifiedFacesResponse[] idFaces = Tinker.JsonHelper.getJsonArray<IdentifiedFaces.IdentifiedFacesResponse>(www.downloadHandler.text);
                List<IdentifiedFaces.IdentifiedFacesResponse> temp = new List<IdentifiedFaces.IdentifiedFacesResponse>();
                temp.AddRange(idFaces);
                identified(true);
                candidates(temp);
                error(www.error);
            }
        }
    }

}


