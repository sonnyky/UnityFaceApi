using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Identify : MonoBehaviour
{
    [System.Serializable]
    public class IdentifyTarget
    {
        public string personGroupId;
        public string[] faceIds;
        public int maxNumOfCandidatesReturned;
        public float confidenceThreshold;
    }
}
