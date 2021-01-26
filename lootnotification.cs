using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class lootnotification : MonoBehaviour
{
    public List<GameObject> bars;

    public GameObject pos1;
    public GameObject pos2;
    Coroutine co;

    public List<string> itemname = new List<string>();
    public List<string> quantity = new List<string>();

    // Start is called before the first frame update
    void Start()
    {
        // move to original position
        gameObject.transform.position = pos1.transform.position;
        // no extensions

        //Addreward("ActionRpgArmor_01", "+22");
        //Addreward("ActionRpgArmor_01", "+01");

    }

    // Update is called once per frame
    void Update()
    {
        /*if (Input.GetKeyUp("1"))
        {
            co = StartCoroutine(MoveToPositionCoroutine(gameObject, pos2.transform.position, 2f, 0f, true));
        }
        //bars[0].GetComponent<itemholder>().textlist = "aa";
        //bars[0].GetComponent<itemholder>().iconlist = "icon";


        //bars[1].GetComponent<itemholder>().textlist = "bb";
        //bars[1].GetComponent<itemholder>().iconlist = "icon";

        //bars[2].SetActive(false);
        //bars[2].SetActive(false);
        */
    }


    //item names are the same with file names in the resources and itemicons directory
    // no file extensions
    public void Addreward(string name, string no)
    {
        itemname.Add(name);
        quantity.Add(no);
    }

    public void Notify(Dictionary<string, int> items)
    {
        if (items.ContainsKey("Wound"))
        {
            items.Remove("Wound");
        }
        // get item name which are also thumbnail file names extentions
        // get the strings from the list as the quantities
        // set them into game objects which is a gameobject list inside notification gameobject list
        // start the coroutine and move the notification list 

        //refresh
        itemname.Clear();
        quantity.Clear();
        if (co != null) StopCoroutine(co);
        gameObject.transform.position = pos1.transform.position;

        //refresh item list with parameter
        foreach (var item in items)
        {
            itemname.Add(item.Key);
            string sign = "+";
            if (item.Value < 0)
            {
                sign = "";
            }
            if (GameObject.Find("Canvas").GetComponent<Balancer>().toollist.Contains(item.Key) && item.Value > 1)
                quantity.Add(sign + "1");
            else quantity.Add(sign + item.Value.ToString());
     
        }

        for (int i = 0; i < itemname.Count; i++)
        {
            Sprite tempsprite = Resources.Load<Sprite>("itemicons/" + itemname[i]);
            if (tempsprite != null)
            {
                bars[i].GetComponent<itemholder>().textgo.GetComponent<Text>().text = quantity[i];
                bars[i].GetComponent<itemholder>().icongo.GetComponent<Image>().overrideSprite = tempsprite;
            }
            else
            {

                Debug.Log("ITEM NOTIFICATION : wrong file name entered -> " + "itemicons/" + itemname[i]);
            }

        }

        if (itemname.Count > 0)
        {
            Vector3 moveVec = pos1.transform.position;
            float movey = ((pos1.transform.position.y - pos2.transform.position.y) / 5f) * itemname.Count + 6;
            moveVec = new Vector3(pos1.transform.position.x, pos1.transform.position.y - movey, pos1.transform.position.z);

            co = StartCoroutine(MoveToPositionCoroutine(gameObject, moveVec, 2f, 0f, true));
        }
    }

    //raises issues on multiple executions
    //need a check if coroutine in use

    public IEnumerator MoveToPositionCoroutine(GameObject obj, Vector3 moveto, float time, float withdelay, bool moveback)
    {
        Debug.Log("time ->" + time);
        // when movement ends move back to original position autonomously via calling self
        if (withdelay > 0)
        {
            float delaytimer = 0f;
            while (delaytimer < withdelay)
            {
                delaytimer += Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }
        }

        float elapsedTime = 0;
        Vector3 startingPos = obj.transform.position;
        //translate to vector 3 and exclude z axis from target
        moveto = new Vector3(moveto.x, moveto.y, obj.transform.position.z);
        while (elapsedTime <= time)
        {
            // in case of multiple object destroyed
            //if (obj == null)
            //{
            //    yield break;
            //}

            obj.transform.position = Vector3.Lerp(startingPos, moveto, elapsedTime / time);
            elapsedTime += Time.deltaTime;


            if (elapsedTime / time >= 1)
            {
                //1 last frame to snap on to target
                obj.transform.position = Vector3.Lerp(startingPos, moveto, 1);
                if (moveback)
                    StartCoroutine(MoveToPositionCoroutine(gameObject, pos1.transform.position, 2f, 3f, false));

                //notification animation completly ended clear the reward list stay idle
                //if (!moveback){
                //    itemname.Clear();
                //    quantity.Clear();
                //    yield break;
                //}

                yield break;
            }


            yield return new WaitForEndOfFrame();
        }
    }

}
