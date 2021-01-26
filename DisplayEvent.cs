using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using System.Xml;
using System.IO;
using System.Xml.Serialization;
using System.Linq;

// code quality is lowered due to scope creep.soygun event.combat event. previous event access
// redandant code at but.

public class DisplayEvent : MonoBehaviour
{
    // holds a the language file
    public TextAsset xmlTA;
    // surpress debug inputs if false
    public bool devmod = false;
    // depriciated
    public bool onlytranslation = false;

    public GameObject combatCanvas;
    // perde catches pointer events temporarily untill an animation ends
    public GameObject perde;
    Coroutine waitco;
    public float voldownTimer;

    // will be played during win/lose buttons . ctype="skill" at xml
    public AudioClip mainclip;
    public AudioSource MainSoundObject;

    public AudioClip winclip;
    public AudioSource winSoundObject;

    public AudioClip loseclip;
    public AudioSource loseSoundObject;

    public Sprite loseicon;
    public Sprite winicon;

    public Button genericreturnbutton;
    public GameObject rewardbar;
    public Component DataHolderComponent;

    // character treats data
    // variables for repetetive item collection.
    // when an item(from pool) is recieved it may spawn an other item according to luck  
    public float tierRepeatDivider;
    public float tierRepeatDivider4MID;
    public float tierRepeatDivider4HIGH;

    public string charjob;
    public List<string> charitemlist;
    // mertcan will populate. it holds characters skill points to be used during
    public Dictionary<string, int> charskills = new Dictionary<string, int>();


    //we have to keep previously clicked button during operations
    int previousClickIndex;

    // gameobjects that is visible on screen
    public Image winlose;
    public float winloseFADEOUTtimer;
    public Image img;
    public Text title;
    public Button button0;
    public Button button1;
    public Button button2;
    public Button button3;
    public Button contin;
    public GameObject notification;

    //nothing
    public Sprite testimage;

    // an event from eventDB . the event that is currently player is in. it is used to get which button goes where data.
    //current event and previous event is decided during displaynewevent() function
    public Event currentE;
    public Event previousEvent;

    //storing all events in the xml file
    public EventDB eventdb;

    // the list that contains items that is collected during 1 event. these rewards will be notified , copy merged into items and flushed immediately after new event.
    [SerializeField]
    public Dictionary<string, int> rewardlist = new Dictionary<string, int>();s

    //consistent events are 
    [SerializeField]
    public Dictionary<string, int> consistentitems = new Dictionary<string, int>();

    Coroutine swco;
    string integrity;
    Buton genericreturn;

    public void buttest()
    {
        Debug.Log("a1");
        string newstr = "";
        foreach (var item in eventdb.events)
        {
            Debug.Log("a2");
            newstr += "\n[HideInInspector] public string " + item.id + "text = \"" + item.etext + " \" ";

            if (item.buttons != null)
            {
                int index = 0;
                while (index < item.buttons.Count)
                {
                    if (item.buttons[index].text != null)
                        newstr += "\n[HideInInspector] public string " + item.id + "button" + index + "text =\" " + item.buttons[index].text + " \"";
                    if (item.buttons[index].hover != null)
                        newstr += "\n[HideInInspector] public string " + item.id + "button" + index + "hovertext =\" " + item.buttons[index].hover + " \"";
                    if (item.buttons[index].ftext != null)
                        newstr += "\n[HideInInspector] public string " + item.id + "button" + index + "ftexttext =\" " + item.buttons[index].ftext + " \"";

                    index++;
                }
            }

        }

        Debug.Log("a3 -> count " + eventdb.events.Count);
        /*
        [HideInInspector] public string sometextforhover = "some actual text for hover ";
        */

        string path = "Assets/Resources/liste.txt";

        //Write some text to the test.txt file
        StreamWriter writer = new StreamWriter(path, true);
        writer.WriteLine(newstr);
        writer.Close();

        //Re-import the file to update the reference in the editor

        Debug.Log(newstr);
        foreach (var item in eventdb.events)
        {
            item.etext = item.id + "text";
            int index = 0;

            if (item.buttons != null)
            {
                while (index < item.buttons.Count)
                {
                    if (item.buttons[index].text != null)
                        item.buttons[index].text = item.id + "button" + index + "text";
                    if (item.buttons[index].hover != null)
                        item.buttons[index].hover = item.id + "button" + index + "thovertext";
                    if (item.buttons[index].ftext != null)
                        item.buttons[index].ftext = item.id + "button" + index + "tftext";

                    index++;
                }
            }

        }


        SaveEvents(eventdb);
    }
    public bool integrityCheck()
    {

        foreach (var evnts in eventdb.events)
        {

            foreach (var but in evnts.buttons)
            {
                string[] tmpJ = but.jumpto.Split('/');
                if (eventdb.isthere(tmpJ[tmpJ.Length - 1]))
                {

                }
                else
                {
                    Debug.Log("MISSING event! " + "1 button in " + evnts.id + " tries to go to --" + tmpJ[tmpJ.Length - 1] + "-- which doesnt exist in the eventDB");
                }

            }

        }
        return false;
    }
    void Start()
    {
        if (onlytranslation == true)
        {// load xml file into the eventDB object
            eventdb = LoadEvents(xmlTA.text);
            // write eventDB into another XML file
            SaveEvents(eventdb);
            // ean emberssing function to make eventDB.event list an events2 dictionary. easy serialization on Lists & easy access on dictionaries.
            eventdb.Order();
        }
        else
        {
            MainSoundObject.clip = mainclip;
            MainSoundObject.Play();


            // load xml file into the eventDB object
            eventdb = LoadEvents(xmlTA.text);
            // write eventDB into another XML file
            SaveEvents(eventdb);
            // ean emberssing function to make eventDB.event list an events2 dictionary. easy serialization on Lists & easy access on dictionaries.
            eventdb.Order();
        }
        
    }
    void Update()
    {

        if (Input.GetKeyUp("1") && devmod)
        {
            integrityCheck();
        }

        if (Input.GetKeyUp("2") && devmod)
        {
            Debug.Log(" current but count    " + currentE.id + " -> " + currentE.buttons.Count);
            Debug.Log(" previous event count " + previousEvent.id + " -> " + previousEvent.buttons.Count);
        }
    }
    public void journeyStarts(string eventid)
    {

        // get character skills from Dataholder component
        //charskills.Clear();
        //(DataHolderComponent as DataHolder).copyLists2Dictionary(charskills, (DataHolderComponent as DataHolder).charskillsKeysFSM, (DataHolderComponent as DataHolder).charskillsValsFSM);
        takeindata();


        Setscene(eventid);
    }
    void startajourney(string eventid)
    {
        takeindata();
        Setscene(eventid);
    }
    //reshape with latest functions
    void takeindata()
    {
        // get character items keys 
        charitemlist.Clear();
        (DataHolderComponent as DataHolder).dictinarize();

        foreach (var item in (DataHolderComponent as DataHolder).itemKeysFSM)
        {
            charitemlist.Add(item);
        }

        //is it neccesary 
        //refresh char skills
        charskills.Clear();
        for (int i = 0; i < (DataHolderComponent as DataHolder).charskillsKeysFSM.Count; i++)
        {
            charskills.Add((DataHolderComponent as DataHolder).charskillsKeysFSM[i], (DataHolderComponent as DataHolder).charskillsValsFSM[i]);
        }

        //is it working ?
        // native dictionary copy 
        charskills.Clear();
        charskills = new Dictionary<string, int>((DataHolderComponent as DataHolder).skills);


        //is it neccesary 
        //refresh consistent items  get character items keys and quantities  . / axe , crowbar
        consistentitems.Clear();
        for (int i = 0; i < (DataHolderComponent as DataHolder).itemKeysFSM.Count; i++)
        {
            consistentitems.Add((DataHolderComponent as DataHolder).itemKeysFSM[i], (DataHolderComponent as DataHolder).itemValsFSM[i]);
        }

        //is it working ?
        // native dictionary copy 
        Debug.Log("consistant temizle");
        consistentitems.Clear();
        Debug.Log("data holder variablelarını dictionariye çevir");
        gameObject.GetComponent<DataHolder>().dictinarize();
        Debug.Log("data holderın dictionarysini consistenta kopyala");
        consistentitems = new Dictionary<string, int>((DataHolderComponent as DataHolder).items);


    }
    //use to setup singleton
    //will copy journey data at the end and report FSM by making flag true
    void sendoutData()
    {
        //copy display event dictionary to Data holder dictionary
        (DataHolderComponent as DataHolder).copyItemsDictionary(consistentitems);
        //unload dictionary to FSM readable dictionaries
        gameObject.GetComponent<DataHolder>().unload();
        (DataHolderComponent as DataHolder).endofjourneyFLAG = true;
    }
    // copy and add items of a rewardlist onto an other consistant items
    void copymerge()
    {

        //special case. reward type count > 5 
        //delete rewards untill reward list count is 5 when its more. loot notification animation crash MAX5 type can be notified
        while (rewardlist.Count > 5)
        {
            Debug.Log("<color=red> last added reward is getting deleted :</color>" + rewardlist.LastOrDefault().Key + " quantity:" + rewardlist.LastOrDefault().Value);
            rewardlist.Remove(rewardlist.LastOrDefault().Key);
        }

        foreach (var item in rewardlist)
        {
            Debug.Log("COPY MERGE merge : " + item.Key + " item.value" + item.Value);
            reliAdd(consistentitems, item.Key, item.Value);
        }
    }
    void printdictionary(Dictionary<string, int> dict)
    {
        Debug.Log("printdictionary " + dict + " count:" + dict.Count);
        foreach (var item in dict)
        {
            Debug.Log(item.Key + " -> " + item.Value);
        }
    }
    void reliAdd(Dictionary<string, int> dict, string key, int quantity)
    {
        Debug.Log("item ->" + key + " q ->" + quantity);


        if (dict.ContainsKey(key))
        {
            Debug.Log("dict[key]111:" + dict[key]);
            dict[key] += quantity;
            Debug.Log("dict[key]222:" + dict[key]);
        }
        else
        {
            dict.Add(key, quantity);
        }
    }

    public List<string> soygunedTools = new List<string>();

    public void testsoygun(string evid, string cond)
    {
        string targetevent = evid;
        // test case //string targetevent = "basicloot11";
        //string targetevent = previousEvent.buttons[previousClickIndex].cwin;

        string condition = cond;
        // test case //string condition = "Axe/Hammer/Pistol";
        //string condition = previousEvent.buttons[previousClickIndex].condition

        string[] stolen = condition.Split('/');
        //string[] stolen = condition;

        string eventmods = "";

        List<string> ownedtools = new List<string>();
        List<string> ownedresources = new List<string>();

        foreach (var item in consistentitems)
        {
            //check all items. copy tools that player has
            if (item.Value > 0 && gameObject.GetComponent<Balancer>().havingthistool(item.Key))
            {
                ownedtools.Add(item.Key);
            }
            if (item.Value > 0 && !(gameObject.GetComponent<Balancer>().havingthistool(item.Key)))
            {
                ownedresources.Add(item.Key);
            }
        }

        foreach (var item in stolen)
        {
            Debug.Log("attempting to remove " + item);
            if (gameObject.GetComponent<Balancer>().havingthistool(item))
            {
                if (ifitexist(ownedtools, item))
                {
                    Debug.Log("exact tool match " + item);
                    //exact tool match
                   // soygunedTools.Add(item);
                    eventmods += item + "$-1$100/";
                    removefromlist(ownedtools, item);
                }
                else
                {
                    //find another tool
                    Debug.Log("find another tool");
                    if (ownedtools.Count > 0)
                    {
                        int rand = Random.Range(0, ownedtools.Count - 1);
                       // soygunedTools.Add(item);
                        eventmods += ownedtools[rand] + "$-1$100/";
                        Debug.Log("selected swap tool " + ownedtools[rand]);
                        removefromlist(ownedtools, ownedtools[rand]);
                    }
                    else
                    {
                        Debug.Log("we have no tool. swap with resources");
                        if (ownedresources.Count > 0)
                        {
                            int rand = Random.Range(0, ownedresources.Count - 1);
                            eventmods += ownedresources[rand] + "$-1$100/";
                            Debug.Log("first resource for the tool stealing " + ownedresources[rand]);
                            removefromlist(ownedresources, ownedresources[rand]);
                        }
                        else
                        {
                            Debug.Log("no resource to exchange with a tool");
                        }

                        if (ownedresources.Count > 0)
                        {
                            int rand = Random.Range(0, ownedresources.Count - 1);
                            eventmods += ownedresources[rand] + "$-1$100/";
                            Debug.Log("second resource for the tool stealing " + ownedresources[rand]);
                            removefromlist(ownedresources, ownedresources[rand]);
                        }
                        else
                        {
                            Debug.Log("no resource second resource cant be stolen");
                        }

                    }
                }
            }
            else
            {
                Debug.Log("exact resource match " + item);
                //its a resource
                if (ifitexist(ownedresources, item))
                {
                    //exact resource match
                    eventmods += item + "$-1$100/";
                    removefromlist(ownedresources, item);
                }
                else
                {
                    //find another resource
                    Debug.Log("find another resource");
                    if (ownedresources.Count > 0)
                    {
                        int rand = Random.Range(0, ownedresources.Count - 1);
                        eventmods += ownedresources[rand] + "$-1$100/";
                        Debug.Log("selected swap resource " + ownedresources[rand]);
                        removefromlist(ownedresources, ownedresources[rand]);
                    }
                    else
                    {
                        Debug.Log("we have no resources at all");
                    }
                }
            }
        }

        targetevent = eventmods + targetevent;

        Debug.Log("soygun result -> " + targetevent);
        Setscene(targetevent);
    }
    bool ifitexist(List<string> lst, string it)
    {
        foreach (var item in lst)
        {
            if (it == item)
            {
                return true;
            }
        }
        return false;
    }
    public void removefromlist(List<string> lst, string it)
    {
        Debug.Log("remove from list started with item->" + it + " list.count->" + lst.Count);
        for (int i = 0; i < lst.Count; i++)
        {
            Debug.Log("trying to remove " + lst[i]);
            if (lst[i] == it)
            {
                Debug.Log(lst[i] + "is removed since its a " + it);
                lst.RemoveAt(i);
            }
        }
    }
    public void returnfromgeneric()
    {

        if (gameObject.GetComponent<DataHolder>().enemyHP < 1)
        {
            Debug.Log("continue button changed for win ----- " + previousEvent.buttons[previousClickIndex].combatwin);
            genericreturn.jumpto = previousEvent.buttons[previousClickIndex].combatwin;

        }
        Setscene(genericreturn.jumpto);
    }




    public void Setscene(string eventid)
    {

        if (currentE.buttons.Count > 0 && currentE.buttons.Count > previousClickIndex && currentE.buttons[previousClickIndex].special != null)
        {
            Debug.Log("<color=red>OLD EVENT ID:</color>" + eventid);
            eventid = gameObject.GetComponent<Balancer>().retrunspecialreward() + eventid;
            Debug.Log("<color=red>NEW EVENT ID:</color>" + eventid);
        }

        if (currentE.buttons.Count > 0 && currentE.buttons.Count > previousClickIndex && currentE.buttons[previousClickIndex].ctype != null && currentE.buttons[previousClickIndex].ctype == "item")
        {
            Debug.Log("<color=red>ctype item</color>" + currentE.buttons[previousClickIndex].cvalue);
            string[] cvaluestrs = currentE.buttons[previousClickIndex].cvalue.Split('/');
            Debug.Log("<color=red>cvalue length</color>" + cvaluestrs.Length);

            for (int i = 0; i < cvaluestrs.Length; i++)
            {
                Debug.Log("<color=red>trying</color>" + cvaluestrs[i]);
                if (consistentitems.ContainsKey(cvaluestrs[i]) && consistentitems[cvaluestrs[i]] > 0)
                {
                    Debug.Log("<color=red>item found :</color>" + cvaluestrs[i]);
                    consistentitems[cvaluestrs[i]] = consistentitems[cvaluestrs[i]] - 1;
                    break;
                }
                else
                {
                    Debug.Log("<color=red>cant find the item :</color>" + cvaluestrs[i]);
                }
            }

        }

        // StopAllCoroutines(); yapışkan ctype:item fix için. waitscene çağırılan yere taşındı
        Debug.Log(" setscene with ->" + eventid);
        gameObject.GetComponent<DataHolder>().eventhistory += "." + eventid;
        // changes the event to the next event depending on the data eventDB.
        // all scene changes has to be done by here so currentE can change which is used by other functions.


        //clear rewards since its a new event
        rewardlist.Clear();


        //specialcase for 4 part event id in zombie damage2

        /* burn it with fire
        string[] rewardstringsemp = eventid.Split('/');

        if (rewardstringsemp.Length == 4)
        {
            Debug.Log("2 damage zombie event");
            zombieDmg2 = int.Parse( rewardstringsemp[3] );
            eventid = rewardstringsemp[0] + "/" + rewardstringsemp[1] + "/" + rewardstringsemp[2];
        }
        else
        {
            if (rewardstringsemp.Length == 3)
                zombieDmg2 = 0;
        }
        */

        //eventids might be the event to sent or it can also contain rewards rolls as this
        // a$2$50/b$2$50/c$2$50/jumpstart   50% of chance to get 2 a ,50% of chance to get 2 b ,50% of chance to get 2 c
        string[] rewardstrings = eventid.Split('/');
        int size = rewardstrings.Length;
        string[] reward = new string[3];



        //if we have a new reward
        if (rewardstrings.Length > 1)
        {
            for (int i = 0; i <= rewardstrings.Length - 2; i++)
            {

                reward = new string[] { "", "", "" };
                reward = rewardstrings[i].Split('$');

                //do a roll. goes here for reward[2].no roll right now for event id rewards.
                //run the balancer for tier type items
                //roller
                if (reward[0] == "lowtier" || reward[0] == "midtier" || reward[0] == "hightier")
                {
                    int basepercent = 100;
                    int randroll = Random.Range(2, 99);
                    float divider = 1;

                    Debug.Log("reward switched from " + reward[0]);

                    while (reward[0] == "lowtier" && randroll < basepercent / divider)
                    {
                        divider *= tierRepeatDivider;
                        reliAdd(rewardlist, gameObject.GetComponent<Balancer>().pickrandom(reward[0]), int.Parse(reward[1]));
                        Debug.Log("to " + reward[0]);
                    }

                    while (reward[0] == "midtier" && randroll < basepercent / divider)
                    {
                        divider *= tierRepeatDivider4MID;
                        reliAdd(rewardlist, gameObject.GetComponent<Balancer>().pickrandom(reward[0]), int.Parse(reward[1]));
                        Debug.Log("to " + reward[0]);
                    }


                    while (reward[0] == "hightier" && randroll < basepercent / divider)
                    {
                        divider *= tierRepeatDivider4HIGH;
                        reliAdd(rewardlist, gameObject.GetComponent<Balancer>().pickrandom(reward[0]), int.Parse(reward[1]));
                        Debug.Log("to " + reward[0]);
                    }
                }
                else
                {
                    // a$2$50
                    reliAdd(rewardlist, reward[0], int.Parse(reward[1]));
                }



            }

            // clear rewards . Normalized eventid
            eventid = rewardstrings[rewardstrings.Length - 1];



            // notify rewards
            Dictionary<string, int> tempdic2 = new Dictionary<string, int>(rewardlist);
            foreach (var item in tempdic2)
            {
                if (gameObject.GetComponent<Balancer>().havingthistool(item.Key) && consistentitems.ContainsKey(item.Key) && consistentitems[item.Key] == 3)
                {
                    rewardlist.Remove(item.Key);
                }
            }
            tempdic2.Clear();


            Debug.Log("<color=red>Notification ended</color>");

            Dictionary<string, int> tempdic = new Dictionary<string, int>(rewardlist);
            //triple the tool rewards
            foreach (var item in rewardlist)
            {
                if (gameObject.GetComponent<Balancer>().toollist.Contains(item.Key))
                {
                    if (rewardlist[item.Key] > 0 )
                    tempdic[item.Key] = rewardlist[item.Key] * 3;
                }
            }
            rewardlist = new Dictionary<string, int>(tempdic);


            // merge copy rewardlist dictionary into consistant reward dictionary
            copymerge();
            //yandere
            tempdic = new Dictionary<string, int>(consistentitems);
            foreach (var item in tempdic)
            {
                if (item.Value > 3 && gameObject.GetComponent<Balancer>().toollist.Contains(item.Key))
                    consistentitems[item.Key] = 3;
            }
            tempdic.Clear();


            rewardbar.GetComponent<lootnotification>().Notify(rewardlist);
            foreach (var item in rewardlist)
            {
                if (item.Value < 0 && gameObject.GetComponent<Balancer>().toollist.Contains(item.Key))
                {
                    soygunedTools.Add(item.Key);
                }
            }
            //if there was a soygun. remove the tools
            for (int i = 0; i < soygunedTools.Count; i++)
            {
                Debug.Log("<color=red> çalınan itemın değerini 1 yapıyorum</color>");
                consistentitems[soygunedTools[i]] = 0;
            }
            //clear soygun list
            soygunedTools.Clear();
            //clear rewards since we are done with it.
            rewardlist.Clear();
        }

        //////////////////////refresh 
        //copy display event dictionary to Data holder dictionary
        (DataHolderComponent as DataHolder).copyItemsDictionary(consistentitems);
        //unload dictionary to FSM readable dictionaries
        gameObject.GetComponent<DataHolder>().unload();



        previousEvent = currentE;


        //fenemyavg
        if (previousEvent.buttons.Count > previousClickIndex && previousEvent.buttons[previousClickIndex].fenemyavg != null && previousEvent.buttons[previousClickIndex].fenemyavg != "")
            gameObject.GetComponent<DataHolder>().fenemyavg = previousEvent.buttons[previousClickIndex].fenemyavg;

        //generic return start
        if (eventid.Contains('>'))
        {
            genericreturn = new Buton();
            genericreturn.jumpto = eventid.Split('>')[1];
            eventid = eventid.Split('>')[0];

            //filler event mods
            //change text by button
            currentE = eventdb.GetAnEvent(eventid);

            if (previousEvent.buttons[previousClickIndex].ftext != null && previousEvent.buttons[previousClickIndex].ftext != "")
                currentE.etext = previousEvent.buttons[previousClickIndex].ftext;



        }
        else
        {
            genericreturn = null;
            currentE = eventdb.GetAnEvent(eventid);
        }
        //generic return end
        if (previousEvent.buttons.Count > previousClickIndex && previousEvent.buttons[previousClickIndex] != null)
            Debug.Log("COMBATWIN" + previousEvent.buttons[previousClickIndex].combatwin);
        Debug.Log("f");
        if (previousEvent.buttons.Count > previousClickIndex && previousEvent.buttons[previousClickIndex] != null && previousEvent.buttons[previousClickIndex].fenemyhp != null)
        {
            Debug.Log("Enemy damage is set");
            gameObject.GetComponent<DataHolder>().enemyHP = int.Parse(previousEvent.buttons[previousClickIndex].fenemyhp);



            gameObject.GetComponent<DataHolder>().ozCombatFlag = true;
        }

        if (previousEvent.buttons.Count > previousClickIndex && previousEvent.buttons[previousClickIndex] != null && previousEvent.buttons[previousClickIndex].fdmgtaken != null)
        {
            Debug.Log("damagetaken");
            gameObject.GetComponent<DataHolder>().playerHP -= Random.Range(int.Parse(previousEvent.buttons[previousClickIndex].fdmgtaken.Split('|')[0]), int.Parse(previousEvent.buttons[previousClickIndex].fdmgtaken.Split('|')[1]));

        }

        if (previousEvent.buttons.Count > previousClickIndex && previousEvent.buttons[previousClickIndex] != null && previousEvent.buttons[previousClickIndex].fdmggiven != null)
        {
            Debug.Log("damagegiven");
            gameObject.GetComponent<DataHolder>().enemyHP -= Random.Range(int.Parse(previousEvent.buttons[previousClickIndex].fdmggiven.Split('|')[0]), int.Parse(previousEvent.buttons[previousClickIndex].fdmggiven.Split('|')[1]));
            if (gameObject.GetComponent<DataHolder>().enemyHP < 1)
            {
                gameObject.GetComponent<DataHolder>().ozCombatFlag = false;
            }
        }

        if (previousEvent.buttons.Count > previousClickIndex && previousEvent.buttons[previousClickIndex] != null && previousEvent.buttons[previousClickIndex].combatwin != null && gameObject.GetComponent<DataHolder>().enemyHP < 1)
        {
            Debug.Log("win condition");
            genericreturn.jumpto = previousEvent.buttons[previousClickIndex].combatwin;
        }

        Debug.Log("event id  check -->" + eventid);

        //old soygun event dont use it
        if (eventid == "soyguneventDEPRIC")
        {

            Debug.Log(" current but count    " + currentE.id + " -> " + currentE.buttons.Count);
            Debug.Log(" previous event count " + previousEvent.id + " -> " + previousEvent.buttons.Count);
            Debug.Log("SOYGUN!!");
            Debug.Log("attempting to remove " + previousEvent.buttons[previousClickIndex].condition);
            //if item is present
            if (consistentitems[previousEvent.buttons[previousClickIndex].condition] >= 1)
            {
                Debug.Log("player has this item sending to new scene with removal signal -> " + previousEvent.buttons[previousClickIndex].condition + "$-1" + "$100/" + previousEvent.buttons[previousClickIndex].cwin);
                //lowtier$1$100/electricbasementrisk
                Debug.Log("soygun ended with ideal item loss");
                Setscene(previousEvent.buttons[previousClickIndex].condition + "$-1" + "$100/" + previousEvent.buttons[previousClickIndex].cwin);
                //send to cwin event and remove item
            }
            //if player doesnt have this item
            else
            {
                Debug.Log("player does not have a " + previousEvent.buttons[previousClickIndex].condition);
                //if this item is a tool give an other item that user has IF USER HAS ANY TOOL
                if (gameObject.GetComponent<Balancer>().havingthistool(previousEvent.buttons[previousClickIndex].condition))
                {
                    Debug.Log("this missing item is a tool attempting to find another tool that player has");
                    List<string> ownedtools = new List<string>();
                    List<string> ownedresources = new List<string>();
                    foreach (var item in consistentitems)
                    {
                        //check all items. copy tools that player has
                        if (item.Value > 0 && gameObject.GetComponent<Balancer>().havingthistool(item.Key))
                        {
                            ownedtools.Add(item.Key);
                        }
                        if (item.Value > 0 && !(gameObject.GetComponent<Balancer>().havingthistool(item.Key)))
                        {
                            ownedresources.Add(item.Key);
                        }

                    }
                    foreach (var item in ownedtools)
                    {
                        Debug.Log("owned " + item);
                    }


                    if (ownedtools.Count > 0)
                    {
                        Debug.Log("player has " + ownedtools.Count + " tools");
                        int rnd = Random.Range(0, ownedtools.Count - 1);

                        Debug.Log("chosen tool ->" + ownedtools[rnd]);
                        Debug.Log(" player has " + consistentitems[ownedtools[rnd]] + " " + ownedtools[rnd]);
                        Debug.Log("soygun ended with tool to tool trade");
                        Debug.Log(" sending eventid " + ownedtools[rnd] + "$-1" + "$100/" + previousEvent.buttons[previousClickIndex].cwin);
                        Setscene(ownedtools[rnd] + "$-1" + "$100/" + previousEvent.buttons[previousClickIndex].cwin);
                    }
                    else
                    {
                        Debug.Log("player doesnt have any tool. !!!find a resource!!!");
                        if (ownedresources.Count > 0)
                        {
                            Debug.Log("player has " + ownedresources.Count + " resources");
                            int rnd = Random.Range(0, ownedresources.Count - 1);
                            Debug.Log("soygun ended with trade to resorce trade");
                            //Debug.Log(" sending eventid " + ownedtools[rnd] + "$-1" + "$100/" + previousEvent.buttons[previousClickIndex].cwin);
                            Setscene(ownedresources[rnd] + "$-1" + "$100/" + previousEvent.buttons[previousClickIndex].cwin);
                        }
                        else
                        {
                            Debug.Log("player has no resource to substitue for a tool");
                            Setscene(previousEvent.buttons[previousClickIndex].cwin);
                        }
                    }
                }
                else
                {
                    Debug.Log("this missing item is a resource attempting to find another resource that player has");

                    List<string> ownedresources = new List<string>();
                    foreach (var item in consistentitems)
                    {
                        if (item.Value > 0 && !(gameObject.GetComponent<Balancer>().havingthistool(item.Key)))
                        {
                            ownedresources.Add(item.Key);
                        }
                    }

                    if (ownedresources.Count > 0)
                    {
                        foreach (var item in ownedresources)
                        {
                            Debug.Log("owned resource " + item);
                        }
                        int rnd = Random.Range(0, ownedresources.Count - 1);
                        Debug.Log("soygun ended with resource to resorce trade");
                        Setscene(ownedresources[rnd] + "$-1" + "$100/" + previousEvent.buttons[previousClickIndex].cwin);
                    }
                    else
                    {
                        Debug.Log("player has no resource go to default");
                        Setscene(previousEvent.buttons[previousClickIndex].cwin);
                    }
                }
                //!!! MULTIPLE ITEM REDUCTION . REMOVE RESOURCE iif there is no tool . 
                //Debug.Log("event id double check 2  ->" + eventid);
                //Debug.Log("index ->" + previousClickIndex);
                //Debug.Log(" current but count    " + currentE.id + " -> " + currentE.buttons.Count);
                //Debug.Log(" previous event count " + previousEvent.id + " -> " + previousEvent.buttons.Count);
                //Debug.Log("buttons count->" + previousEvent.buttons.Count);
                //Debug.Log("soygun ended with fallback. jump to ->" + previousEvent.buttons[previousClickIndex].cwin);
                //Debug.Log("clicked index ->" + previousClickIndex);
            }
        }


        //special cases which requires pseduo events. will be skipped immediately after a custom function. item reduction for stealing.
        if (eventid == "soygunevent")
        {
            testsoygun(previousEvent.buttons[previousClickIndex].cwin, previousEvent.buttons[previousClickIndex].condition);
        }
        //old combat system this function is depriciated dont use it.
        else if (eventid == "combateventDEPRIC")
        {
            combatFlag = true;
            StartCoroutine(fadeOutJournalSceneForFight());
            Debug.LogError("Using old combat system .use new combat system!!!");


            //previousEvent.buttons[previousClickIndex].close;
            //previousEvent.buttons[previousClickIndex].cwin;

            //send player items
        }
        // this means we are at a regular event . continue normally
        else
        {
            string[] buttexts = new string[4] { null, null, null, null };
            for (int i = 0; i < currentE.buttons.Count; i++)
            {
                buttexts[i] = currentE.buttons[i].text;
            }
            if (waitco != null) StopCoroutine(waitco);
            StopAllCoroutines();

            waitco = StartCoroutine(waitscene(currentE.sprite, currentE.etext, buttexts[0], buttexts[1], buttexts[2], buttexts[3]));
        }

        //DisplayNewEvent(currentE.sprite, currentE.etext, buttexts[0], buttexts[1], buttexts[2], buttexts[3]);
    }

    // get the text from GameObject.Find("Translation")
    public string getExternalText(string id)
    {
        //Component comp = GameObject.Find("Translation").GetComponent<textlist>();
        //string qwert = comp.GetType().GetField(currentE.id).GetValue(comp).ToString();
        Debug.Log("EXTERNAL TEXT ---- >>>> " + id);
        string res = GameObject.Find("Translation").GetComponent<textlist>().gettxt(id);
        Debug.Log("EXTERNAL TEXT ---- >>>> " + res);
        return res;
    }
    public IEnumerator fadeOutJournalSceneForFight()
    {
        Debug.Log("PERDE ACTIVE");
        perde.SetActive(true);
        float opacity = 1;
        while (opacity > 0)
        {
            opacity = opacity - Time.deltaTime;

            Color tmpclr1 = title.color;
            Color tmpclr2 = img.color;
            tmpclr1.a = opacity;
            tmpclr2.a = opacity;
            //img.GetComponent<Image>().color = tmpclr2;

            button0.image.color = tmpclr2;
            button1.image.color = tmpclr2;
            button2.image.color = tmpclr2;
            button3.image.color = tmpclr2;

            title.color = tmpclr1;
            img.color = tmpclr2;

            button0.transform.Find("Text").GetComponent<Text>().color = tmpclr1;
            button0.transform.Find("Image").GetComponent<Image>().color = tmpclr2;
            button1.transform.Find("Text").GetComponent<Text>().color = tmpclr1;
            button1.transform.Find("Image").GetComponent<Image>().color = tmpclr2;
            button2.transform.Find("Text").GetComponent<Text>().color = tmpclr1;
            button2.transform.Find("Image").GetComponent<Image>().color = tmpclr2;
            button3.transform.Find("Text").GetComponent<Text>().color = tmpclr1;
            button3.transform.Find("Image").GetComponent<Image>().color = tmpclr2;

            contin.image.color = tmpclr2;
            contin.transform.Find("Text").GetComponent<Text>().color = tmpclr1;

            yield return new WaitForEndOfFrame();
        }
        combatCanvas.SetActive(true);
        string[] zombieStats = new string[3] { null, null, null };
        Debug.Log("previousClickIndex " + previousClickIndex);
        Debug.Log("previousEvent.buttons[previousClickIndex].cvalue " + previousEvent.buttons[previousClickIndex].cvalue);
        Debug.Log("previousEvent.buttons[previousClickIndex].cvalue.Split('/') " + previousEvent.buttons[previousClickIndex].cvalue.Split('/'));

        zombieStats = previousEvent.buttons[previousClickIndex].cvalue.Split('/');
        if (zombieStats.Length != 4)
            combatCanvas.GetComponent<CombatManager2>().combatinit(zombieStats[0], zombieStats[1], zombieStats[2], previousEvent.buttons[previousClickIndex].cwin, previousEvent.buttons[previousClickIndex].close);
        else
            combatCanvas.GetComponent<CombatManager2>().combatinit(zombieStats[0], zombieStats[1], zombieStats[2], zombieStats[3], previousEvent.buttons[previousClickIndex].cwin, previousEvent.buttons[previousClickIndex].close);

        //combat initiator has its own fade in 

    }
    public bool ozCombatFlag = false;
    bool combatFlag = false;
    public IEnumerator waitscene(string sprite, string etext, string buttexts0, string buttexts1, string buttexts2, string buttexts3)
    {

        /*
        imgholder.transform.localScale = new Vector3(1f + (scalemultip * opac), 1f + (scalemultip * opac), imgholder.transform.localScale.z);
        tmp = new Color(imgholder.GetComponent<Image>().color.r, imgholder.GetComponent<Image>().color.g, imgholder.GetComponent<Image>().color.b, opac);
        imgholder.GetComponent<Image>().color = tmp;
        opac += Time.deltaTime * scaleupmultip;
        */
        Debug.Log("PERDE ACTIVE");
        perde.SetActive(true);
        float opacity = 1;
        while (opacity > 0 && combatFlag == false)
        {
            opacity = opacity - Time.deltaTime;

            Color tmpclr1 = title.color;
            Color tmpclr2 = img.color;
            tmpclr1.a = opacity;
            tmpclr2.a = opacity;
            //img.GetComponent<Image>().color = tmpclr2;
            button0.image.color = tmpclr2;
            button1.image.color = tmpclr2;
            button2.image.color = tmpclr2;
            button3.image.color = tmpclr2;
            genericreturnbutton.image.color = tmpclr2;

            title.color = tmpclr1;
            button0.transform.Find("Text").GetComponent<Text>().color = tmpclr1;
            button0.transform.Find("Image").GetComponent<Image>().color = tmpclr2;

            button1.transform.Find("Text").GetComponent<Text>().color = tmpclr1;
            button1.transform.Find("Image").GetComponent<Image>().color = tmpclr2;
            button2.transform.Find("Text").GetComponent<Text>().color = tmpclr1;
            button2.transform.Find("Image").GetComponent<Image>().color = tmpclr2;
            button3.transform.Find("Text").GetComponent<Text>().color = tmpclr1;
            button3.transform.Find("Image").GetComponent<Image>().color = tmpclr2;


            genericreturnbutton.transform.Find("Text").GetComponent<Text>().color = tmpclr1;
            genericreturnbutton.transform.Find("Image").GetComponent<Image>().color = tmpclr2;

            contin.image.color = tmpclr2;
            contin.transform.Find("Text").GetComponent<Text>().color = tmpclr1;

            yield return new WaitForEndOfFrame();
        }
        yield return new WaitForSeconds(1f);

        DisplayNewEvent(sprite, etext, buttexts0, buttexts1, buttexts2, buttexts3);

        opacity = 0;
        while (opacity < 1)
        {


            Color tmpclr1 = title.color;
            Color tmpclr2 = img.color;
            tmpclr1.a = opacity;
            tmpclr2.a = opacity;
            //img.GetComponent<Image>().color = tmpclr2;
            button0.image.color = tmpclr2;
            button1.image.color = tmpclr2;
            button2.image.color = tmpclr2;
            button3.image.color = tmpclr2;
            genericreturnbutton.image.color = tmpclr2;

            if (combatFlag) img.color = tmpclr2;
            title.color = tmpclr1;
            button0.transform.Find("Text").GetComponent<Text>().color = tmpclr1;
            button0.transform.Find("Image").GetComponent<Image>().color = tmpclr2;
            button1.transform.Find("Text").GetComponent<Text>().color = tmpclr1;
            button1.transform.Find("Image").GetComponent<Image>().color = tmpclr2;
            button2.transform.Find("Text").GetComponent<Text>().color = tmpclr1;
            button2.transform.Find("Image").GetComponent<Image>().color = tmpclr2;
            button3.transform.Find("Text").GetComponent<Text>().color = tmpclr1;
            button3.transform.Find("Image").GetComponent<Image>().color = tmpclr2;


            genericreturnbutton.transform.Find("Text").GetComponent<Text>().color = tmpclr1;
            genericreturnbutton.transform.Find("Image").GetComponent<Image>().color = tmpclr2;


            contin.image.color = tmpclr2;
            contin.transform.Find("Text").GetComponent<Text>().color = tmpclr1;


            opacity += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        Debug.Log("PERDE GIDER");
        perde.SetActive(false);
        combatFlag = false;
    }
    void DisplayNewEvent(string newimg, string newtext, string newbut0, string newbut1, string newbut2, string newbut3)
    {
        //Set hover texts

        if (genericreturn != null)
        {
            genericreturnbutton.gameObject.SetActive(true);
        }
        else
        {
            genericreturnbutton.gameObject.SetActive(false);
        }

        // parameters are the stuff to be shown on the screen.
        // if there is no button in the event . make a continue button that ends the journal
        if (currentE.buttons.Count == 0)
        {
            contin.gameObject.SetActive(true);
        }
        else
            contin.gameObject.SetActive(false);

        // should i change the image ?
        if (newimg != null && newimg != "")
        {
            Debug.Log("00000000" + newimg);
            this.img.sprite = Resources.Load<Sprite>("sketches/" + newimg);
        }
        else
        {
            //noimage
        }

        // title is the text that is shown on the screen
        // newbut0
        this.title.text = getExternalText(newtext);

        // for each button check if it should be visible on the screen
        // if a buttton visible it should be set. meaining is it interactable, is there a condition, is there a item requirement
        button0.transform.Find("Image").gameObject.SetActive(false);
        button1.transform.Find("Image").gameObject.SetActive(false);
        button2.transform.Find("Image").gameObject.SetActive(false);
        button3.transform.Find("Image").gameObject.SetActive(false);




        if (newbut0 != null && newbut0 != "")
        {
            gameObject.GetComponent<DataHolder>().hover0 = getExternalText(currentE.buttons[0].hover);
            button0.gameObject.SetActive(true);
            setbuttons(currentE.buttons[0], 0);
        }
        else
        {
            gameObject.GetComponent<DataHolder>().hover0 = "";
            button0.gameObject.SetActive(false);
            button0.transform.Find("Image").gameObject.SetActive(false);
        }

        if (newbut1 != null && newbut1 != "")
        {
            gameObject.GetComponent<DataHolder>().hover1 = getExternalText(currentE.buttons[1].hover);
            button1.gameObject.SetActive(true);
            setbuttons(currentE.buttons[1], 1);
        }
        else
        {
            gameObject.GetComponent<DataHolder>().hover1 = "";
            button1.gameObject.SetActive(false);
            button1.transform.Find("Image").gameObject.SetActive(false);
        }

        if (newbut2 != null && newbut2 != "")
        {
            gameObject.GetComponent<DataHolder>().hover2 = getExternalText(currentE.buttons[2].hover);
            button2.gameObject.SetActive(true);
            setbuttons(currentE.buttons[2], 2);
        }
        else
        {
            gameObject.GetComponent<DataHolder>().hover2 = "";
            button2.gameObject.SetActive(false);
            button2.transform.Find("Image").gameObject.SetActive(false);
        }

        if (newbut3 != null && newbut3 != "")
        {
            gameObject.GetComponent<DataHolder>().hover3 = getExternalText(currentE.buttons[3].hover);
            button3.gameObject.SetActive(true);
            setbuttons(currentE.buttons[3], 3);
        }
        else
        {
            gameObject.GetComponent<DataHolder>().hover3 = "";
            button3.gameObject.SetActive(false);
            button3.transform.Find("Image").gameObject.SetActive(false);
        }

        // start of generic buton
        /*
        if (genericreturn != null)
        {
            Debug.Log("Generic event buton build");
            Debug.Log("Generic return to "+genericreturn.jumpto);
            button0.gameObject.SetActive(true);
            genericreturn.text = "...";
            newbut0 = " ";
            setbuttons(genericreturn, 0);
        }
        */
        // end of generic buton

        Debug.Log("QQ->" + this.button0.GetComponentInChildren<Text>().text);
        Debug.Log("Q1->" + newbut0);
        this.button0.transform.Find("Text").GetComponent<Text>().text = getExternalText(newbut0);
        this.button1.transform.Find("Text").GetComponent<Text>().text = getExternalText(newbut1);
        this.button2.transform.Find("Text").GetComponent<Text>().text = getExternalText(newbut2);
        this.button3.transform.Find("Text").GetComponent<Text>().text = getExternalText(newbut3);
    }
    public void setbuttons(Buton but, int butindex)
    {
        // the button becomes the corresponding gameobject.
        Button thebutton = button0;
        if (butindex == 1)
            thebutton = button1;
        if (butindex == 2)
            thebutton = button2;
        if (butindex == 3)
            thebutton = button3;
        thebutton.interactable = true;
        //if button has a condition . it might be cause of job,item or there might be a chance of losing depending on the skill point.

        if (but.ctype != null)
        {

            if (but.ctype == "job")
            {
                //set icon for item
                thebutton.transform.Find("Image").gameObject.SetActive(true);
                thebutton.transform.Find("Image").GetComponent<Image>().sprite = Resources.Load<Sprite>("buttonicons/" + but.cvalue);

                if (charjob != but.cvalue) //is char job is NOT the required job
                    thebutton.interactable = false;



            }
            else if (but.ctype == "item")
            {




                // is the item presented in the char item list. char can carry multiple items so charitem is a list and required item is a list.
                // carring 1 of the required items is enough

                string[] items = but.cvalue.Split('/');

                //set icon for item
                thebutton.transform.Find("Image").gameObject.SetActive(true);
                thebutton.transform.Find("Image").GetComponent<Image>().sprite = Resources.Load<Sprite>("buttonicons/" + items[0]);


                // so this is how an array becomes a list

                /* -------------------
                List<string> lst = new List<string>();
                foreach (var item in items)
                {
                    Debug.Log("GEREKEN ITEMLAR -> "+item);
                    lst.Add(item);
                }
                */


                //swtich if multiple items

                if (items.Length >= 2)
                {
                    List<string> tmplist = new List<string>();
                    for (int i = 0; i < items.Length; i++)
                    {
                        tmplist.Add(items[i]);
                    }
                    spriteswitcher(tmplist, thebutton.transform.Find("Image").gameObject.GetComponent<Image>());

                }
                // does lst and charitemlist has a common item
                Debug.Log("ITEM BAKACAM BACIM");

                thebutton.interactable = false;
                if (items.Length == 1 && consistentitems.ContainsKey(items[0]) && consistentitems[items[0]] > 0)
                {
                    Debug.Log("ITEM VAR BACIM");
                    thebutton.interactable = true;
                }
                else
                {
                    Debug.Log("ITEM YOK BACIM");
                }

                if (items.Length == 2 && ((consistentitems.ContainsKey(items[1]) && consistentitems[items[1]] > 0) || (consistentitems.ContainsKey(items[0]) && consistentitems[items[0]] > 0)))
                {

                    thebutton.interactable = true;
                }
                if (items.Length == 3 && ((consistentitems.ContainsKey(items[2]) && consistentitems[items[2]] > 0) || (consistentitems.ContainsKey(items[1]) && consistentitems[items[1]] > 0) || (consistentitems.ContainsKey(items[0]) && consistentitems[items[0]] > 0)))
                {
                    thebutton.interactable = true;
                }



            }
            else if (but.ctype == "skill")
            {

                string[] items = but.cvalue.Split('/');
                int rollpercent = 0;
                Debug.Log(" key " + items[0]);
                int skillpoint = charskills[items[0]];
                int skillthresehold = int.Parse(items[1]);//parseint
                int basepercent = int.Parse(items[2]); //parseint


                //set icon for skill
                thebutton.transform.Find("Image").gameObject.SetActive(true);
                thebutton.transform.Find("Image").GetComponent<Image>().sprite = Resources.Load<Sprite>("buttonicons/" + items[0]);



                //"INT/5/20" skill condition representation
                foreach (var item in items)
                {
                    // Debug.Log("items array ///" + item);
                }
                if (skillpoint > skillthresehold)
                {
                    rollpercent = ((100 - basepercent) / (skillpoint - skillthresehold)) * (skillpoint - skillthresehold) + basepercent;
                }
                else
                {
                    rollpercent = basepercent;
                }
                //D roll happens when scene is set. on a scene all rolls already happened and roll result is known and but.jumpto has the event with the win/lose target already.
                int randroll = Random.Range(0, 101);
                if (randroll < rollpercent)
                {
                    //win condition. inside skill points default jumpto value is lose condition ... if its win, cwin atrribute writes jumpto.
                    Debug.Log("Win");
                    but.jumpto = but.cwin;
                }
                else
                {
                    Debug.Log("Lose");
                    //lose condition. inside skill points default jumpto value is lose condition.
                }

            }
            else if (but.ctype == "skill")
            {
                /*
                string[] items = but.cvalue.Split('/');
                int rollpercent = 0;
                Debug.Log(" key " + items[0]);
                int skillpoint = charskills[items[0]];
                int skillthresehold = int.Parse(items[1]);//parseint
                int basepercent = int.Parse(items[2]); //parseint


                //set icon for skill
                thebutton.transform.Find("Image").gameObject.SetActive(true);
                thebutton.transform.Find("Image").GetComponent<Image>().sprite = Resources.Load<Sprite>("buttonicons/" + items[0]);



                //"INT/5/20" skill condition representation
                foreach (var item in items)
                {
                    // Debug.Log("items array ///" + item);
                }
                if (skillpoint > skillthresehold)
                {
                    rollpercent = ((100 - basepercent) / (skillpoint - skillthresehold)) * (skillpoint - skillthresehold) + basepercent;
                }
                else
                {
                    rollpercent = basepercent;
                }
                //D roll happens when scene is set. on a scene all rolls already happened and roll result is known and but.jumpto has the event with the win/lose target already.
                int randroll = Random.Range(0, 101);
                if (randroll < rollpercent)
                {
                    //win condition. inside skill points default jumpto value is lose condition ... if its win, cwin atrribute writes jumpto.
                    Debug.Log("Win");
                    but.jumpto = but.cwin;
                }
                else
                {
                    Debug.Log("Lose");
                    //lose condition. inside skill points default jumpto value is lose condition.
                }*/

            }
        }
    }
    public void rollnotif(bool iswin)
    {
        gameObject.GetComponent<WinLoseAnim>().rollnotif(iswin);
    }
    /*
    public void rollnotif(bool iswin)
    {
        winlose.transform.localScale = new Vector3(1f, 1f, 1f);
        winlose.GetComponent<Image>().color = new Color(winlose.GetComponent<Image>().color.r, winlose.GetComponent<Image>().color.g, winlose.GetComponent<Image>().color.b, 0f);

        if (iswin)
            loseSoundObject.Play();
        else
            winSoundObject.Play();

        if (iswin)
            winlose.sprite = winicon;
        else
            winlose.sprite = loseicon;

        StartCoroutine(rollanim(iswin, winlose));
        StartCoroutine(voldown());
    }
    float defaultvol;
    public IEnumerator voldown()
    {
        MainSoundObject.volume = defaultvol * 0.5f;
        yield return new WaitForSeconds(voldownTimer);
        MainSoundObject.volume = defaultvol;
        yield return null;
    }
    Coroutine volco;
    public IEnumerator rollanim(bool win, Image imgholder)
    {
        if (volco != null) StopCoroutine(volco);
        volco = StartCoroutine(voldown());



        float opac = 0f;
        float scalemultip = 0.6f;
        float scaleupmultip = 10f;
        float scaledownmultip = winloseFADEOUTtimer;
        float max = 1.1f;
        Color tmp;

        while (opac < max)
        {
            imgholder.transform.localScale = new Vector3(1f + (scalemultip * opac), 1f + (scalemultip * opac), imgholder.transform.localScale.z);
            tmp = new Color(imgholder.GetComponent<Image>().color.r, imgholder.GetComponent<Image>().color.g, imgholder.GetComponent<Image>().color.b, opac);
            imgholder.GetComponent<Image>().color = tmp;
            opac += Time.deltaTime * scaleupmultip;
            yield return new WaitForEndOfFrame();
        }
        max = -1f;
        while (opac > 0f)
        {
            imgholder.transform.localScale = new Vector3(1f + (scalemultip * opac), 1f + (scalemultip * opac), imgholder.transform.localScale.z);
            tmp = new Color(imgholder.GetComponent<Image>().color.r, imgholder.GetComponent<Image>().color.g, imgholder.GetComponent<Image>().color.b, opac);
            imgholder.GetComponent<Image>().color = tmp;
            opac -= Time.deltaTime * scaledownmultip;
            yield return new WaitForEndOfFrame();
        }

        opac = 0f;
        tmp = new Color(imgholder.GetComponent<Image>().color.r, imgholder.GetComponent<Image>().color.g, imgholder.GetComponent<Image>().color.b, opac);
        imgholder.GetComponent<Image>().color = tmp;



        yield return null;
    }*/
    public void spriteswitcher(List<string> itemlist, Image imgholder)
    {
        // get a list of items and switch a sprite constantly
        if (swco != null) { StopCoroutine(swco); }
        Debug.Log("SWITCH ANIM -starts");
        StartCoroutine(switchanim(imgholder, itemlist, 1f));
    }
    public IEnumerator switchanim(Image imgholder, List<string> itemlist, float timer)
    {
        while (true)
        {
            foreach (var item in itemlist)
            {
                Debug.Log("item name to be shown --> " + item);
                Sprite tempsprite = Resources.Load<Sprite>("buttonicons/" + item);
                if (tempsprite != null)
                {
                    Debug.Log("BUTON ICON : file name ACCEPTED -> " + "buttonicons/" + item);
                    imgholder.sprite = tempsprite;
                }
                else
                {
                    Debug.Log("BUTON ICON : wrong file name entered -> " + "buttonicons/" + item);
                }
                yield return new WaitForSeconds(timer);
            }

        }
    }
    public void setButtonInactive()
    {
        // nah iw ill just deactivate button inline
    }
    public void endofevent()
    {
        //FSM needs to be informed about the event scene is ended.
        StopAllCoroutines();
        sendoutData();
    }
    // if else clean up required
    public void jump(int bno)
    {
        previousClickIndex = bno;
        // 4 button so 4 ifs . so what?
        switch (bno)
        {
            case 0:
                Debug.Log("JUMPTO id:" + currentE + " butsize:" + currentE.buttons.Count);
                if (currentE.buttons[0].ctype == "skill")
                    if (currentE.buttons[0].cwin == currentE.buttons[0].jumpto)
                    {
                        rollnotif(true);
                        Debug.Log("WIN WIN WIN WIN WIN");
                    }
                    else
                    {
                        rollnotif(false);
                        Debug.Log("LOSE LOSE LOSE LOSE LOSE");
                    }
                Setscene(currentE.buttons[0].jumpto);
                break;
            case 1:
                Debug.Log("JUMPTO id:" + currentE + " butsize:" + currentE.buttons.Count);
                if (currentE.buttons[1].ctype == "skill")
                    if (currentE.buttons[1].cwin == currentE.buttons[1].jumpto)
                    {
                        rollnotif(true);
                        Debug.Log("WIN WIN WIN WIN WIN");
                    }
                    else
                    {
                        rollnotif(false);
                        Debug.Log("LOSE LOSE LOSE LOSE LOSE");
                    }
                Setscene(currentE.buttons[1].jumpto);
                break;
            case 2:
                Debug.Log("JUMPTO id:" + currentE + " butsize:" + currentE.buttons.Count);
                if (currentE.buttons[2].ctype == "skill")
                    if (currentE.buttons[2].cwin == currentE.buttons[2].jumpto)
                    {
                        rollnotif(true);
                        Debug.Log("WIN WIN WIN WIN WIN");
                    }
                    else
                    {
                        rollnotif(false);
                        Debug.Log("LOSE LOSE LOSE LOSE LOSE");
                    }
                Setscene(currentE.buttons[2].jumpto);
                break;
            case 3:
                Debug.Log("JUMPTO id:" + currentE + " butsize:" + currentE.buttons.Count);
                if (currentE.buttons[3].ctype == "skill")
                    if (currentE.buttons[3].cwin == currentE.buttons[3].jumpto)
                    {
                        rollnotif(true);
                        Debug.Log("WIN WIN WIN WIN WIN");
                    }
                    else
                    {
                        rollnotif(false);
                        Debug.Log("LOSE LOSE LOSE LOSE LOSE");
                    }
                Setscene(currentE.buttons[3].jumpto);
                break;
            default:
                break;
        }
    }
    public static void SaveEvents(object item)
    {
        //eventDB object will be populated by the xml file.
        // object will be serialized and put into a file for later use. this will be used to save a game.
        XmlSerializer serializer = new XmlSerializer(item.GetType());
        StreamWriter writer = new StreamWriter(Application.dataPath + "/Resources/eventsout.xml");
        serializer.Serialize(writer.BaseStream, item);
        writer.Close();
    }
    public static EventDB LoadEvents(string datum)
    {
        StringReader STRreader = new StringReader(datum);


        string m_Path = Application.dataPath;
        Debug.Log("PATH -> " + m_Path);
        // an xml file will loaded from a file.
        // eventDB will be populated depending on this file.
        XmlSerializer serializer = new XmlSerializer(typeof(EventDB));
        //StreamReader reader = new StreamReader(Application.dataPath + "/Resources/events.xml");
        //EventDB deserialized = (EventDB)serializer.Deserialize(reader.BaseStream);
        EventDB deserialized = (EventDB)serializer.Deserialize(STRreader);
        //reader.Close();

        // return the object so it could be replaced by eventDB
        return deserialized;
    }


    [System.Serializable]
    public class Event
    {
        // every data that can be seen in screen when player is in the journal and see 1 event
        public string id;
        public string etext;
        public string isdisabled;
        public string sprite;

        [XmlArray("buttons")]
        public List<Buton> buttons = new List<Buton>();

        public override string ToString()
        {
            return "id:" + id + "|bool:" + isdisabled + "|text:" + etext + "|image:" + sprite + "|e1:";
        }

    }
    [System.Serializable]
    [XmlRoot("Buton")]
    public class Buton
    {
        // every event can cause to 4 different events.
        // each button has a text, a text to be shown during hover, where to go when pressed and condition data if the button has a specific requirements to be calculated before jump
        [XmlElement("text")]
        public string text;
        [XmlElement("hover")]
        public string hover;
        [XmlElement("jumpto")]
        public string jumpto;
        [XmlAttributeAttribute("condition")]
        public string condition;
        [XmlAttributeAttribute("ctype")]
        public string ctype; //meslek,item,skill,combat
        [XmlAttributeAttribute("cvalue")]
        public string cvalue; //meslekadı,itemadı,skillise:5,10
        [XmlAttributeAttribute("cwin")]
        public string cwin; //meslekadı,itemadı,skillise:5,10
        [XmlAttributeAttribute("close")]
        public string close; //meslekadı,itemadı,skillise:5,10
        [XmlAttributeAttribute("ftext")]
        public string ftext;
        [XmlAttributeAttribute("fdmgtaken")]
        public string fdmgtaken;
        [XmlAttributeAttribute("fdmggiven")]
        public string fdmggiven;
        [XmlAttributeAttribute("fenemyhp")]
        public string fenemyhp;
        [XmlAttributeAttribute("combatwin")]
        public string combatwin;
        [XmlAttributeAttribute("fenemyavg")]
        public string fenemyavg;
        [XmlAttributeAttribute("special")]
        public string special;

    }
    [System.Serializable]
    public class EventDB
    {
        // 1 big object to be populazied once in the begining by xml file.
        // and will be written to save file when game is over.

        // this list contains all events but will be used only once.
        // events list stays untouched during the game.
        public List<Event> events = new List<Event>();

        // not public . so serilizable cant see.
        // indexing is need to access events by their ids so list is copied to a dictionary.
        [System.NonSerialized]
        private Dictionary<string, Event> events2 = new Dictionary<string, Event>();

        //get function to retrieve 1 event
        public Event GetAnEvent(string id)
        {
            Debug.Log("eventDB returns ->" + id);
            return events2[id];
        }

        //get function to retrieve 1 event
        public bool isthere(string id)
        {
            if (events2.ContainsKey(id)) { return true; }
            else return false;
        }

        // this will populate the events2 dictionary with events List
        public void Order()
        {
            events2.Clear();
            foreach (var item in events)
            {
                events2.Add(item.id, item);
            }
        }


        public override string ToString()
        {
            string res = "";
            foreach (var item in events2)
            {
                res += item.ToString() + "\n";
            }
            return res;
        }
    }
}
