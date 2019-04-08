using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using TinkerExtensions;

public class ReadHomography : MonoBehaviour {

    private string[] homography_str;
    private List<float> homography;
    // Use this for initialization
    void Start()
    {
        homography = new List<float>();
        string xml = File.ReadAllText(@"Assets/UnityUtilities/Resources/homography.xml");
        var catalog1 = xml.ParseXML<opencv_storage>();
        homography_str = catalog1.Homography.data.ToString().Split(" "[0]);
        for (int i = 0; i < homography_str.Length; i++)
        {
            try
            {
                homography.Add(float.Parse(homography_str[i]));

            }
            catch { }
            
        }

        for(int j = 0; j < homography.Count; j++)
        {
            Debug.Log(homography[j]);
        }
        Debug.Log(homography.Count);
    }

    // Update is called once per frame
    void Update () {
		
	}
}
