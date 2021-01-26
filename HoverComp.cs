using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using TMPro;
using UnityEngine.UI;

using UnityEngine.EventSystems;

public class HoverComp : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    //public Image hoverbg;
    public GameObject hoverobj;
    public float enterdelay;
    public float exitdelay;
    public string hovertext;
    public int offsetx;
    public int offsety;
    int addy = 25;
    int addx = 20;
    Coroutine co;
    float tmpdelay;
    float currentstate;

    public string nameInXML;
    public string resultOnTranslator;
    // Start is called before the first frame update

    public Vector3 test;
    bool onmouse;

    public void translate()
    {

        Component comp = GameObject.Find("Translation").GetComponent<textlist>();


        if (nameInXML != "")
            resultOnTranslator = comp.GetType().GetField(nameInXML).GetValue(comp).ToString();
        if (resultOnTranslator != "")
            hovertext = resultOnTranslator;
        //gameObject.GetComponent<Text>().text = resultOnTranslator;
    }

    void Start()
    {
        GameObject.Find("Translation").GetComponent<textlist>().translatablesHover.Add(gameObject);
    }

    int xdir = +1;
    int ydir = +1;
    // Update is called once per frame
    void Update()
    {
        if (onmouse)
        {
            //hoverobj.transform.position = new Vector3(Input.mousePosition.x + (((RectTransform)hoverobj.transform).rect.width + ((RectTransform)gameObject.transform).rect.width) * xdir * 0.3f, Input.mousePosition.y + (((RectTransform)hoverobj.transform).rect.height + ((RectTransform)gameObject.transform).rect.height) * ydir * 0.3f, 0);
            //hoverobj.transform.parent.gameObject.transform.position = hoverobj.transform.position;
            hoverobj.transform.parent.gameObject.transform.position = new Vector3
                (Input.mousePosition.x +  (   ((RectTransform)gameObject.transform).rect.width * xdir * 0.3f)+ addx * xdir, 
                 Input.mousePosition.y +  (   ((RectTransform)gameObject.transform).rect.height * ydir * 0.3f)+ addy * ydir, 0);
        }
        test = gameObject.transform.InverseTransformPoint(hoverobj.transform.position);

    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        onmouse = true;
        GameObject DCanvas = GameObject.Find("Canvas");

        if (gameObject.transform.position.x > DCanvas.transform.position.x)
            xdir = -1;
        if (gameObject.transform.position.y > DCanvas.transform.position.y)
            ydir = -1;



        if (xdir == 1 && ydir == -1)
        {
            addy = 40;
            addx = 32;
        }
        else { 
         addy = 25;
         addx = 20;
    }//Debug.Log("x->" + xdir + " y->" + ydir);
        //Debug.Log("gameObject.transform.position.x->" + gameObject.transform.position.x + " gameObject.transform.position.y->" + gameObject.transform.position.y);
        //Debug.Log("DCanvas.transform.position.x->" + DCanvas.transform.position.x + " DCanvas.transform.position.y->" + DCanvas.transform.position.y);
        //Debug.Log("((RectTransform)gameObject.transform).rect.width->" + ((RectTransform)gameObject.transform).rect.width + " ((RectTransform)gameObject.transform).rect.height->" + ((RectTransform)gameObject.transform).rect.height);
        //Log("resx->" + ((((RectTransform)gameObject.transform).rect.width * xdir)) + " resy->" + ((((RectTransform)gameObject.transform).rect.height * ydir)));


        translate();
        // hoverobj.GetComponentInChildren hovertext
        // hoverobj.GetComponent<Image>().color = new Color(1, 1, 1, 0);

        //hoverobj.GetComponentInChildren<TMPro.TextMeshProUGUI>().color = new Color(1, 1, 1, 0);
        hoverobj.GetComponent<Text>().text = hovertext;

        //hoverobj.transform.localPosition = new Vector2(gameObject.transform.localPosition.x + offsetx, gameObject.transform.localPosition.y + offsety);

        //Vector3 newpos = new Vector3(gameObject.transform.localPosition.x + ((((RectTransform)gameObject.transform).rect.width / 2 + ((RectTransform)hoverobj.transform).rect.width / 2) * xdir)
        //                            , gameObject.transform.localPosition.y + ((((RectTransform)gameObject.transform).rect.height / 2 + ((RectTransform)hoverobj.transform).rect.height / 2) * ydir)
        //                            , 0);


        hoverobj.transform.parent.gameObject.transform.position = gameObject.transform.position;

        //Debug.Log("ho x)" + ((RectTransform)hoverobj.transform).rect.width);
        //Debug.Log("go x)" + ((RectTransform)gameObject.transform).rect.width);
        //Debug.Log("hv x)" + hoverobj.transform.position.x);

        hoverobj.transform.parent.gameObject.transform.position = gameObject.transform.position + new Vector3((((RectTransform)hoverobj.transform).rect.width + ((RectTransform)gameObject.transform).rect.width) * xdir * 0.3f, (((RectTransform)hoverobj.transform).rect.height + ((RectTransform)gameObject.transform).rect.height) * ydir * 0.3f, 0);
        hoverobj.transform.parent.gameObject.transform.position = new Vector3(eventData.position.x, eventData.position.y, 0);

        //hoverobj.transform.parent.gameObject.transform.position = hoverobj.transform.position;



        //  Debug.Log("go x)" + gameObject.transform.position.x);
        //Debug.Log("x->"+gameObject.transform.InverseTransformPoint(hoverobj.transform.position).x+ "y->" + gameObject.transform.InverseTransformPoint(hoverobj.transform.position).y);



        //hoverobj.transform.position = new Vector2(gameObject.transform. position.x + offsetx, gameObject.transform.position.y + offsety);
        if (co != null) StopCoroutine(co);
        co = StartCoroutine(FadeImage(false));
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        onmouse = false;
        //hoverobj.GetComponent<Image>().color = new Color(1, 1, 1, 1);
        //hoverobj.GetComponentInChildren<TMPro.TextMeshProUGUI>().color = new Color(1, 1, 1, 1);
        if (co != null) StopCoroutine(co);
        co = StartCoroutine(FadeImage(true));
    }

    IEnumerator FadeImage(bool fadeAway)
    {


        // fade from opaque to transparent
        if (fadeAway)
        {

            if (exitdelay > 0)
            {
                tmpdelay = exitdelay;
                while (0 < tmpdelay)
                {
                    tmpdelay -= Time.deltaTime;
                    yield return new WaitForEndOfFrame();
                }
            }
            // loop over 1 second backwards
            for (float i = currentstate; i >= 0; i -= Time.deltaTime * 10)
            {
                // set color with i as alpha
                hoverobj.GetComponent<Text>().color = new Color(hoverobj.GetComponent<Text>().color.r, hoverobj.GetComponent<Text>().color.g, hoverobj.GetComponent<Text>().color.b, i);
                hoverobj.transform.parent.gameObject.GetComponent<Image>().color = new Color(hoverobj.transform.parent.gameObject.GetComponent<Image>().color.r, hoverobj.transform.parent.gameObject.GetComponent<Image>().color.g, hoverobj.transform.parent.gameObject.GetComponent<Image>().color.b, i);
                //hoverobj.GetComponentInChildren<TMPro.TextMeshProUGUI>().color = new Color(1, 1, 1, i);
                currentstate = i;
                yield return null;
            }
        }
        // fade from transparent to opaque
        else
        {
            if (enterdelay > 0)
            {
                float tmpdelay = enterdelay;
                while (0 < tmpdelay)
                {
                    tmpdelay -= Time.deltaTime;
                    yield return new WaitForEndOfFrame();
                }
            }

            // loop over 1 second
            for (float i = currentstate; i <= 1; i += Time.deltaTime * 5)
            {
                // set color with i as alpha
                hoverobj.GetComponent<Text>().color = new Color(hoverobj.GetComponent<Text>().color.r, hoverobj.GetComponent<Text>().color.g, hoverobj.GetComponent<Text>().color.b, i);
                hoverobj.transform.parent.gameObject.GetComponent<Image>().color = new Color(hoverobj.transform.parent.gameObject.GetComponent<Image>().color.r, hoverobj.transform.parent.gameObject.GetComponent<Image>().color.g, hoverobj.transform.parent.gameObject.GetComponent<Image>().color.b, i);
                //hoverobj.GetComponentInChildren<TMPro.TextMeshProUGUI>().color = new Color(1, 1, 1, i);
                yield return null;
            }
        }
    }

}