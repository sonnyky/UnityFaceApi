using System.Collections.Generic;
using System.Xml.Serialization;

[XmlRoot]
public class opencv_storage 
{
    [XmlArrayItem("Homography")]
    public homography_data Homography { get; set; }
}

public class homography_data
{
    public int rows { get; set; }
    public int cols { get; set; }
    public string d { get; set; }
    public string data { get; set; }
}