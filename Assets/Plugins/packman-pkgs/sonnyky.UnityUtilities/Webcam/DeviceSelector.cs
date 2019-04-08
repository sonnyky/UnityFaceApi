using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class DeviceSelector : MonoBehaviour
{

    public UnityEvent m_DevicesReadyEvent;

    private WebCamDevice[] devices;
    private List<WebCamTexture> texs;

    public RawImage m_Screen;

    public struct Resolution
    {
        public int height;
        public int width;
    }

    // Use this for initialization
    void Start()
    {
        texs = new List<WebCamTexture>();
        devices = WebCamTexture.devices;
        if (devices.Length > 0)
        {
            GetDeviceMaxResolution();
        }

        m_DevicesReadyEvent.Invoke();
    }

    public int GetNumberOfDevices()
    {
        return devices.Length;
    }

    public Resolution[] GetDeviceMaxResolution()
    {
        Resolution[] res = new Resolution[devices.Length];
        for(int i=0; i< devices.Length; i++)
        {
            Debug.Log(devices[i].name);
            WebCamTexture tex = new WebCamTexture(devices[i].name, 4096, 4096, 60);
            texs.Add(tex);
            texs[i].Play();
            Debug.Log("tex" + i + " height and width : " + texs[i].height + ", " + texs[i].width);
            res[i].height = texs[i].height;
            res[i].width = texs[i].width;
            texs[i].Stop();
        }
        return res;
    }

    public WebCamDevice GetDevice(int id)
    {

        if (id > devices.Length - 1)
        {
            Debug.LogError("Specified ID exceeds number of available devices");
            return devices[0];
        }
        return devices[id];
    }
    
}
