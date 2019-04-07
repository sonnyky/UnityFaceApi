using System.Collections;
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


    [SerializeField]
    string m_ImageFolderPath;

    [SerializeField]
    string m_TestImagepath;

    string m_OutputPath = "Assets/App/Resources/";

    // Start is called before the first frame update
    IEnumerator Start()
    {
        // Check if the person group ID already exists
        bool requestSucceded = false;
        yield return RequestManager.GetPersonGroup(m_PersonGroupId, value => requestSucceded = value );

        if (requestSucceded)
        {
            Debug.Log("Person group ID exists");

            // Check if the target person is already created. We only need one person for this demo.
            string firstPersonId = "Unknown";
            yield return RequestManager.GetPersonListInGroup(m_PersonGroupId, value => firstPersonId = value);
            PersonInGroup.Person[] firstPersonJson = JsonHelper.getJsonArray<PersonInGroup.Person>(firstPersonId);

            if (firstPersonJson[0].personId != null)
            {
                string id = firstPersonJson[0].personId;
                Debug.Log("Person found in group with person Id : " + id);
                
                // Get training status of the person group
                string trainingStatus = "Unknown";
                yield return RequestManager.GetPersonGroupTrainingStatus(m_PersonGroupId, value => trainingStatus = value);
                Error.ErrorResponse res_error = JsonUtility.FromJson<Error.ErrorResponse>(trainingStatus);
                Debug.Log(res_error.error.code);

                // If not trained the add images and detect faces
                if (res_error.error.code != null && res_error.error.code.Equals("PersonGroupNotTrained"))
                {
                    // Get all images in the specified folder and detect the faces rectangles in them.
                    string[] imageFiles = Directory.GetFiles(m_ImageFolderPath, "*.jpg");
                    for(int i=0; i<imageFiles.Length; i++)
                    {
                        string response = "Unknown";
                        yield return RequestManager.DetectFaces(m_ImageFolderPath + "/me" + (i+1) +  ".jpg", value => response = value);
                        Debug.Log("Response from DetectFaces : " + response);
                        FacesBasic.FacesDetectionResponse[] face = JsonHelper.getJsonArray<FacesBasic.FacesDetectionResponse>(response);

                        // Register the images to the person in person group
                        if (face[0].rect != null)
                        {
                            // Add face to person in group
                            string faceRect = "targetFace=" + face[0].rect.left + "," + face[0].rect.top + "," + face[0].rect.width + "," + face[0].rect.height;
                            string addFaceResponse = "Unknown";
                            yield return RequestManager.AddFaceToPersonInGroup(m_PersonGroupId, id,  m_ImageFolderPath + "/me" + (i + 1) + ".jpg",faceRect,  value => addFaceResponse = value);
                            Debug.Log(addFaceResponse);

                        }
                    }


                    string trainPersonGroupResult = "Unknown";
                    yield return RequestManager.TrainPersonGroup(m_PersonGroupId, value => trainPersonGroupResult = value);
                    if(trainPersonGroupResult == "")
                    {
                        Debug.Log("Training success");
                    }

                }
                else
                // Person Group is already trained
                {
                    Training.TrainingStatus status = JsonUtility.FromJson<Training.TrainingStatus>(trainingStatus);
                    Debug.Log("PersonGroup is already trained");
                    Debug.Log(status);
                    //TODO : check success json response

                    // Try to detect face in test image
                    string[] testImageFiles = Directory.GetFiles(m_TestImagepath, "*.jpg");

                    for(int i=0; i< testImageFiles.Length; i++)
                    {
                        Debug.Log(testImageFiles[i]);
                        // Detect faces in the test image
                        string detectFaces = "unknown";
                        yield return RequestManager.DetectFaces(testImageFiles[i], value => detectFaces = value);
                        Debug.Log("Response from DetectFaces : " + detectFaces);
                        FacesBasic.FacesDetectionResponse[] face = JsonHelper.getJsonArray<FacesBasic.FacesDetectionResponse>(detectFaces);

                        // Identify faces in the test image
                        string identifyFaces = "unknown";
                        yield return RequestManager.Identify(m_PersonGroupId, face, value => identifyFaces = value);
                        Debug.Log("Response from identifyFaces : " + identifyFaces);

                        IdentifiedFaces.IdentifiedFacesResponse[] idFaces = JsonHelper.getJsonArray<IdentifiedFaces.IdentifiedFacesResponse>(identifyFaces);

                        StreamWriter writer = new StreamWriter(m_OutputPath + "output" + i + ".json", true);
                        writer.WriteLine(identifyFaces);
                        writer.Close();

                        StreamWriter faces = new StreamWriter(m_OutputPath + "faces" + i + ".json", true);
                        faces.WriteLine(detectFaces);
                        faces.Close();

                    }

                }

            }
            else
            {
                Debug.Log("Person not found");
                bool personCreated = false;
                yield return RequestManager.CreatePersonInGroup(m_PersonGroupId, m_PersonInGroup, "First person", value => personCreated = value);

                if (personCreated)
                {
                    Debug.Log("Successfully created Person in Group");
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
            yield return RequestManager.CreatePersonGroup(m_PersonGroupId, "ShinMisato", "Visitors who came to our Shin Misato theme park", value => createSucceded = value);

            if (createSucceded)
            {
                Debug.Log("Successfully created Person Group");
            }
            else
            {
                Debug.Log("Failed to create Person Group");
            }


        }
       
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
