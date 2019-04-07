using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Training : MonoBehaviour
{
    [System.Serializable]
    public class TrainingStatus
    {
        public string status;
        public string createdDateTime;
        public string lastActionDateTime;
        public string message;
    }
}
