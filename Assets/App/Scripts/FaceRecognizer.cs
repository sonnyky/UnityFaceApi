using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaceRecognizer : MonoBehaviour
{

    [SerializeField]
    // Must be English letters in lower case, numbers, _ , - . and the max length is 64
    string m_PersonGroupId;

    [SerializeField]
    string m_PersonInGroup;

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
            bool personExists = false;
            yield return RequestManager.GetPersonListInGroup(m_PersonGroupId, value => personExists = value);
            if (personExists)
            {
                Debug.Log("Person found in group");
                
                // Get training status of the person group
                string trainingStatus = "Unknown";
                yield return RequestManager.GetPersonGroupTrainingStatus(m_PersonGroupId, value => trainingStatus = value);
                Error.ErrorResponse res_error = JsonUtility.FromJson<Error.ErrorResponse>(trainingStatus);

                // If not trained the add images and detect faces
                if (res_error.error.code.Equals("PersonGroupNotTrained"))
                {
                    // Send images to detect faces
                    string response = "Unknown";
                    yield return RequestManager.DetectFaces("Assets/App/Resources/me1.jpg", value => response = value);
                    Debug.Log("Response from DetectFaces : " + response);

                }
                else
                {
                    //TODO : check success json response
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
