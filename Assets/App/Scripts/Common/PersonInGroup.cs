using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PersonInGroup : MonoBehaviour
{
    [System.Serializable]
    public class Person
    {
        public string personId;
        public string[] persistedFaceIds;
        public string name;
        public string userData;
    }
}
