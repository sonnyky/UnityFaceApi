using System.Collections.Generic;
using System.Xml.Serialization;

[XmlRoot]
public class Config
{
    [XmlArray("SettingPerStore")]
    [XmlArrayItem("Parameters")]
    public List<Parameters> Parameters { get; set; }
}

public class Parameters
{
    public string id { get; set; }
    public int minHeight { get; set; }
    public int maxHeight { get; set; }
}