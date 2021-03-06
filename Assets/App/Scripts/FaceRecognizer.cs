﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class FaceRecognizer : MonoBehaviour
{

    [SerializeField]
    // Must be English letters in lower case, numbers, _ , - . and the max length is 64
    string m_PersonGroupId;

    [SerializeField]
    string m_PersonInGroup;

    bool m_PersonListInGroupNotEmpty = false;

    [SerializeField]
    string m_ImageFolderPath;

    [SerializeField]
    string m_TestImagepath;

    string m_OutputPath = "Assets/App/Resources/";

    [SerializeField]
    string ENV_KEY = "None";

    [SerializeField]
    string ENDPOINT = "Default";

    string API_KEY;

    // Start is called before the first frame update
    IEnumerator Start()
    {
        if (ENV_KEY.Equals("None")){
            Debug.LogError("No environment variable key is defined. Stopping.");
            yield break;
        }

        GetApiKey();

        // Check if the person group ID already exists
        bool requestSucceded = false;
        yield return RequestManager.GetPersonGroup(ENDPOINT, API_KEY, m_PersonGroupId, value => requestSucceded = value );

        if (requestSucceded)
        {
            Debug.Log("Person group ID exists");

            // Check if the target person is already created. We only need one person for this demo.
            List<PersonInGroup.Person> personList = new List<PersonInGroup.Person>();
            yield return RequestManager.GetPersonListInGroup(ENDPOINT, API_KEY, m_PersonGroupId, res => m_PersonListInGroupNotEmpty = res, value => personList = value);

            if (personList.Count  > 0 && personList[0].personId != null)
            {
                // Check if the target person exists in the list
                bool personFound = false;
                for(int i=0; i< personList.Count; i++)
                {
                    Debug.Log("name : "  + personList[i].name);
                    if (personList[i].name.Equals(m_PersonInGroup)) personFound = true;
                }

                if(!personFound) yield break;

                string id = personList[0].personId;
                Debug.Log("Person found in group with person Id : " + id);
                
                // Get training status of the person group
                Training.TrainingStatus trainingSuccess = new Training.TrainingStatus();
                Error.ErrorResponse resError = new Error.ErrorResponse();
                bool trained = false;
                yield return RequestManager.GetPersonGroupTrainingStatus(ENDPOINT, API_KEY, m_PersonGroupId, res => trained = res, err => resError = err, value => trainingSuccess = value);

                // If not trained the add images and detect faces
                if (!trained && resError.error.code != null && resError.error.code.Equals("PersonGroupNotTrained"))
                {
                    // Get all images in the specified folder and detect the faces rectangles in them.
                    string[] imageFiles = Directory.GetFiles(m_ImageFolderPath, "*.jpg");
                    for(int i=0; i<imageFiles.Length; i++)
                    {
                        bool faceFound = false;
                        List<FacesBasic.FacesDetectionResponse> face = new List<FacesBasic.FacesDetectionResponse>();
                        byte[] imageByte = File.ReadAllBytes(m_ImageFolderPath + "/me" + (i + 1) + ".jpg");
                        Texture2D img = new Texture2D(1, 1);
                        img.LoadImage(imageByte);
                        yield return RequestManager.DetectFaces(ENDPOINT, API_KEY, img, res => faceFound = res ,value => face = value);
                        
                        // Register the images to the person in person group
                        if (face[0].rect != null)
                        {
                            // Add face to person in group
                            string faceRect = "targetFace=" + face[0].rect.left + "," + face[0].rect.top + "," + face[0].rect.width + "," + face[0].rect.height;
                            string addFaceResponse = "Unknown";
                            bool addFaceSuccess = false;
                            byte[] imgByte = File.ReadAllBytes(m_ImageFolderPath + "/me" + (i + 1) + ".jpg");
                            Texture2D curImg = new Texture2D(1,1);
                            curImg.LoadImage(imgByte);
                            yield return RequestManager.AddFaceToPersonInGroup(ENDPOINT, API_KEY, m_PersonGroupId, id, curImg, faceRect, res => addFaceSuccess = res,  value => addFaceResponse = value);
                            Debug.Log(addFaceResponse);

                        }
                    }


                    string trainPersonGroupResult = "Unknown";
                    bool trainingResult = false;
                    yield return RequestManager.TrainPersonGroup(ENDPOINT, API_KEY, m_PersonGroupId, res => trainingResult = res, value => trainPersonGroupResult = value);
                    if(trainingResult)
                    {
                        Debug.Log("Training success. Stop and restart app to try identification.");
                    }

                }
                else if(trainingSuccess.status.Equals("succeeded"))
                // Person Group is already trained
                {

                    Debug.Log("PersonGroup is already trained");
                    // Try to detect face in test image
                    string[] testImageFiles = Directory.GetFiles(m_TestImagepath, "*.jpg");

                    for(int i=0; i< testImageFiles.Length; i++)
                    {
                        Debug.Log(testImageFiles[i]);
                        // Detect faces in the test image
                        bool faceFound = false;
                        List<FacesBasic.FacesDetectionResponse> face = new List<FacesBasic.FacesDetectionResponse>();
                        byte[] testImgByte = File.ReadAllBytes(testImageFiles[i]);
                        Texture2D testImg = new Texture2D(1, 1);
                        testImg.LoadImage(testImgByte);
                        yield return RequestManager.DetectFaces(ENDPOINT, API_KEY, testImg, res => faceFound = res, value => face = value);

                        // Identify faces in the test image
                        bool identified = false;
                        List<IdentifiedFaces.IdentifiedFacesResponse> facesList = new List<IdentifiedFaces.IdentifiedFacesResponse>();
                        string errors = "";
                        yield return RequestManager.Identify(ENDPOINT, API_KEY, m_PersonGroupId, face.ToArray(), res => identified = res, err =>errors = err, value => facesList = value);
                        if (identified)
                        {
                            Debug.Log(facesList);
                        }
                    }

                }

            }
            else
            {
                Debug.Log("Person not found");
                PersonCreateSuccess.PersonCreateSuccessResponse successResponse = new PersonCreateSuccess.PersonCreateSuccessResponse();
                bool createdPerson = false;
                yield return RequestManager.CreatePersonInGroup(ENDPOINT, API_KEY, m_PersonGroupId, m_PersonInGroup, "First person",res => createdPerson = res,   value => successResponse = value);

                if (createdPerson)
                {
                    Debug.Log("Successfully created Person in Group. Stop and restart app to try identification");
                }
                else
                {
                    Debug.Log("Failed to create Person in Group");
                }
            }

        }
        else
        {
            Debug.Log("Person group ID does not exist. Creating Person Group");
            bool createSucceded = false;
            yield return RequestManager.CreatePersonGroup(ENDPOINT, API_KEY, m_PersonGroupId, "ShinMisato", "Visitors who came to our Shin Misato theme park", value => createSucceded = value);

            if (createSucceded)
            {
                Debug.Log("Successfully created Person Group. Stop and restart app to try performing identification");
            }
            else
            {
                Debug.Log("Failed to create Person Group. Something went wrong. Please debug.");
            }


        }
       
    }

    void GetApiKey()
    {
        API_KEY = EnvironmentVariables.GetVariable(ENV_KEY);
    }
}
