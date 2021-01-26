using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class translator : MonoBehaviour
{

    public string nameInXML;
    Component comp;
    public string resultOnTranslator;

    // Use this for initialization
    public void translate()
    {
        //comp = GameObject.Find("Translation").GetComponent<textlist>();
        //resultOnTranslator = comp.GetType().GetField(nameInXML).GetValue(comp).ToString();
        

        resultOnTranslator = GameObject.Find("Translation").GetComponent<textlist>().gettxt(nameInXML);
        gameObject.GetComponent<Text>().text = resultOnTranslator;
    }

    void  Start()
    {
        
        GameObject.Find("Translation").GetComponent<textlist>().translatables.Add(gameObject);
        translate();
    }
    // Update is called once per frame
    void Update()
    {

    }
}
