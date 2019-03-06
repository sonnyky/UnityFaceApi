using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FacesBasic : MonoBehaviour
{
    [System.Serializable]
    public class FaceRectangle
    {
        public string top;
        public string left;
        public string width;
        public string height;
    }

    [System.Serializable]
    public class FacesDetectionResponse
    {
        public string faceId;
        public FaceRectangle rect;
    }
}
