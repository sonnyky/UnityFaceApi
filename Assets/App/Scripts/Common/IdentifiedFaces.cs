using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdentifiedFaces : MonoBehaviour
{
    [System.Serializable]
    public class Candidates
    {
        public string personId;
        public float confidence;
    }

    [System.Serializable]
    public class IdentifiedFacesResponse
    {
        public string faceId;
        public Candidates candidates;
    }
}
