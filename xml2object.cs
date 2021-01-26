using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Xml;

public class xml2object : MonoBehaviour
{
    public TextAsset languagefileXML;
    public Component comp;

    void Start()
    {
        XmlDocument data = new XmlDocument();
        data.Load(new StringReader(languagefileXML.text));

        foreach (XmlNode node in data.DocumentElement.ChildNodes)
        {
            comp.GetType().GetField(node.Attributes["name"].InnerText).SetValue(comp, node.InnerText);
        }


    }

    void Update()
    {
       /* if (Input.GetKey("space"))
        {
            setfields();
        }*/
    }

    public void setfields()
    {
        XmlDocument data = new XmlDocument();
        data.Load(new StringReader(languagefileXML.text));

        foreach (XmlNode node in data.DocumentElement.ChildNodes)
        {
            comp.GetType().GetField(node.Attributes["name"].InnerText).SetValue(comp, node.InnerText);
        }
    }
}


