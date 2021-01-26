using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataHolder : MonoBehaviour
{
    public Component displayEventComponent;

    public string fenemyavg;
    // if its false, stay idle. run end of journey operation and make it false again.
    public bool endofjourneyFLAG = false;
    public bool ozCombatFlag = false;
    public string eventhistory = "";

    public int enemyHP;
    public int playerHP;


    public int Lockpick;
    public int Multitool;
    public int FirstAid;
    public int Axe;
    public int Hammer;
    public int Flashlight;
    public int Repellent;
    public int ZombieKit;
    public int Components;
    public int Tools;
    public int Scrap;
    public int Wood;
    public int Food;
    public int Wound;
    public int Pistol;
    public int Stritem;
    public int Agiitem;
    public int Peritem;
    public int Charitem;
    public int Intitem;

    public int AGI;
    public int INT;
    public int STR;
    public int PER;
    public int CHA;

    public void unload()
    {
        Lockpick = items["Lockpick"];
        Multitool = items["Multitool"];
        FirstAid = items["FirstAid"];
        Axe = items["Axe"];
        Hammer = items["Hammer"];
        Flashlight = items["Flashlight"];
        Repellent = items["Repellent"];
        ZombieKit = items["ZombieKit"];
        Components = items["Components"];
        Tools = items["Tools"];
        Scrap = items["Scrap"];
        Wood = items["Wood"];
        Food = items["Food"];
        Wound = items["Wound"];
        Pistol = items["Pistol"];
        Stritem = items["Stritem"];
        Agiitem = items["Agiitem"];
        Peritem = items["Peritem"];
        Charitem = items["Charitem"];
        Intitem = items["Intitem"];


        AGI = skills["AGI"];
        INT = skills["INT"];
        STR = skills["STR"];
        PER = skills["PER"];
        CHA = skills["CHA"];
    }

    public void dictinarize()
    {
        items.Clear();
        items.Add("Lockpick", Lockpick);
        items.Add("Multitool", Multitool);
        items.Add("FirstAid", FirstAid);
        items.Add("Axe", Axe);
        items.Add("Hammer", Hammer);
        items.Add("Flashlight", Flashlight);
        items.Add("Repellent", Repellent);
        items.Add("ZombieKit", ZombieKit);
        items.Add("Components", Components);
        items.Add("Tools", Tools);
        items.Add("Scrap", Scrap);
        items.Add("Wood", Wood);
        items.Add("Food", Food);
        items.Add("Wound", Wound);
        items.Add("Pistol", Pistol);
        items.Add("Stritem", Stritem);
        items.Add("Agiitem", Agiitem);
        items.Add("Peritem", Peritem);
        items.Add("Charitem", Charitem);
        items.Add("Intitem", Intitem);

        skills.Clear();
        skills.Add("AGI", AGI);
        skills.Add("INT", INT);
        skills.Add("STR", STR);
        skills.Add("PER", PER);
        skills.Add("CHA", CHA);
    }



    // WOOD , IRON , AXE ...
    public List<string> itemKeysFSM = new List<string>();
    //  22  ,  4   ,  1  ...
    public List<int> itemValsFSM = new List<int>();
    public Dictionary<string, int> items = new Dictionary<string, int>();

    // AGI , INT , CAR ...
    public List<string> charskillsKeysFSM = new List<string>(new string[] { "AGI", "INT", "STR", "PER", "CHA" });
    //  3  ,  4  ,  3  ...
    public List<int> charskillsValsFSM = new List<int>(new int[] { 3, 4, 5, 6, 7 });
    public Dictionary<string, int> skills = new Dictionary<string, int>();

    public string hover0;
    public string hover1;
    public string hover2;
    public string hover3;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

        /*
        if (Input.GetKeyUp("m"))
        {
            Debug.Log("variables 2 dictionary");
            dictinarize();
        }

        if (Input.GetKeyUp("n"))
        {
            Debug.Log("dictionary 2 variables");
            unload();
        }

        if (Input.GetKeyUp("o"))
        {
            Debug.Log("printing DataHolder Dictionary . should match variables !");
            printdictionary(items);
        }
        */
    }


    public void copyDictionary2Lists(Dictionary<string, int> collection, List<string> Keys, List<int> Vals)
    {
        foreach (var item in collection)
        {
            Keys.Add(item.Key);
            Vals.Add(item.Value);
        }
    }
    public void copyLists2Dictionary(Dictionary<string, int> collection, List<string> Keys, List<int> Vals)
    {
        for (int i = 0; i < Keys.Count; i++)
        {
            collection.Add(Keys[i], Vals[i]);
        }

    }

    public void copyItemsDictionary(Dictionary<string, int> collection)
    {
        itemKeysFSM.Clear();
        itemValsFSM.Clear();
        foreach (var item in collection)
        {
            itemKeysFSM.Add(item.Key);
            itemValsFSM.Add(item.Value);
        }

        items.Clear();
        items = null;
        items = new Dictionary<string, int>(collection);

        Debug.Log("Converting dataholder dictionaries to variables");
        unload();
    }

    void printdictionary(Dictionary<string, int> dict)
    {
        Debug.Log("printdictionary " + dict + " count:" + dict.Count);
        foreach (var item in dict)
        {
            Debug.Log(item.Key + " -> " + item.Value);
        }
    }
}
