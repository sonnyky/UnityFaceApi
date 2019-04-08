using System.Collections;
using System.Collections.Generic;
using System.IO;
using TinkerExtensions;
using UnityEngine;

public class ReadConfig : MonoBehaviour {

	// Use this for initialization
	void Start () {
        string xml = File.ReadAllText(@"Assets/UnityUtilities/Resources/sample.xml");
        var catalog1 = xml.ParseXML<Config>();
        Debug.Log(catalog1.Parameters[0].minHeight);
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
