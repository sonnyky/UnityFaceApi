using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Error : MonoBehaviour
{
    [System.Serializable]
    public class ErrorObject
    {
        public string code;
        public string message;
    }

    [System.Serializable]
    public class ErrorResponse
    {
        public ErrorObject error;
    }
}
