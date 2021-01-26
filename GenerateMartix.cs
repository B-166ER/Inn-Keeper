using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GenerateMartix : MonoBehaviour
{
    public Canvas canvas;
    public bool finished;
    public float button_size;
    public float offset;
    public float scale_constant = 1f;
    public float screenratio = 1f;
    [Header("Tüm sizelar eşit olmalı")]
    [Tooltip("bina türleri (GameObject)")]
    public GameObject[] genericHouse = new GameObject[3];
    public GameObject[] buildings;
    float button_size_x;
    float button_size_y;
    float offset_x;
    float offset_y;

    [Tooltip("ihtimallere göre konan binalar 1 kez konduktan sonra ihtimalli 0 a düşürülür")]
    public int[] probabilities;
    [Tooltip("en az 1 tane olmalı (safe houseta bu değerin bir önemi yok)")]
    public bool[] musthave1;
    [Tooltip("bina yatay uzunluğu 2mi")]
    public bool[] versize;
    [Tooltip("bina dikey uzunluğu 2mi")]
    public bool[] horsize;


    bool[,] mapslots = new bool[8, 7];
    public GameObject[,] mapindex = new GameObject[8, 7];

    int[,] maptypeindex = new int[8, 7];
    public string mapseed;

    int[] probabilitiesConstant = new int[50];
    bool[] musthave1Constant = new bool[50];

    Vector2 v2;
    int size;

    // Start is called before the first frame update
    void Start()
    {
        button_size_x = button_size;
        button_size_y = button_size;
        offset_x = offset;
        offset_y = offset;

        buildings[0] = genericHouse[Random.Range(0, genericHouse.Length)];

        for (int i = 0; i < probabilities.Length; i++)
        {
            probabilitiesConstant[i] = probabilities[i];
            musthave1Constant[i] = musthave1[i];
        }
    }

    // Update is called once per frame
    void Update()
    {
        //button_size_x = button_size * canvas.transform.localScale.x * scale_constant;
        //button_size_y = button_size * canvas.transform.localScale.y * scale_constant;
        //offset_x = offset * canvas.transform.localScale.x * scale_constant;
        //offset_y = offset * canvas.transform.localScale.y * scale_constant;

        /*
        if (Input.GetKeyDown("n"))
        {
            Debug.Log("n pressed");
            redraw();
            Debug.Log("redraw ended");
        }
        if (Input.GetKeyDown("r"))
        {
            Debug.Log("r pressed");
            printseed();
            Debug.Log("printseed ended");
        }
        */

        if (Input.GetKeyDown("c"))
            destroymap();
        if (Input.GetKeyDown("x"))
        {
            drawnew();
        }
        if (Input.GetKeyDown("z"))
        {
            executeseed();
        }
        
    }

    public void executeseed()
    {

        string[] temp = mapseed.Split('_');
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 7; j++)
            {
                mapslots[i, j] = false;
            }
        }
        destroymap();

        for (int i = 0; i < 8 * 7; i++)
        {
            string[] build = temp[i].Split(',');
            if (int.Parse(build[0]) > 999)
            {
                put(genericHouse[int.Parse(build[0]) - 1000], new Vector3(int.Parse(build[1]), int.Parse(build[2])));
            }
            else
            {
                put(buildings[int.Parse(build[0])], new Vector3(int.Parse(build[1]), int.Parse(build[2])));
            }
        }
    }
    public void destroymap()
    {
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 7; j++)
            {
                mapslots[i, j] = false;
                if (mapindex[i, j] != null)
                {
                    Destroy(mapindex[i, j]);
                    mapindex[i, j] = null;
                }
            }
        }
    }
    public void drawnew()
    {
        for (int i = 0; i < probabilities.Length; i++)
        {
            probabilities[i] = probabilitiesConstant[i];
            musthave1[i] = musthave1Constant[i];
        }

        put(buildings[1], new Vector2(7, 3));
        for (int i = 0; i < 8 * 7; i++)
        {
            if (isthereaslot())
            {
                place1building();
            }
        }
        finished = true;
        printseed();
    }
    public void place1building()
    {

        GameObject btemp;
        Vector2 vtemp;
        int hsize = 0;
        int vsize = 0;
        int index = 0;

        btemp = what2put();
        hsize = 0;
        vsize = 0;
        for (int x = 0; x < buildings.Length; x++)
        {
            if (GameObject.ReferenceEquals(btemp, buildings[x]))
            {
                index = x;
                if (horsize[x])
                    hsize = 1;
                if (versize[x])
                    vsize = 1;
            }
        }
        vtemp = where2put(hsize, vsize);

        if (vtemp.x != -2)
        {
            put(btemp, where2put(hsize, vsize));
        }



    }



    public void printseed()
    {
        mapseed = "";
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 7; j++)
            {
                mapseed += maptypeindex[i, j].ToString() + "," + i.ToString() + "," + j.ToString() + "_";
                if (mapindex[i, j] != null)
                {
                    mapindex[i, j].GetComponent<DistanceDisplay>().positionX = i;
                    mapindex[i, j].GetComponent<DistanceDisplay>().positionY = j;
                    Debug.Log(i + "---" + j);
                }
            }
        }
    }
    void redraw()
    {
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 7; j++)
            {
                if (mapindex[i, j] != null)
                {
                    mapindex[Mathf.FloorToInt(i), Mathf.FloorToInt(j)] = Instantiate<GameObject>(mapindex[i, j]);
                    mapindex[Mathf.FloorToInt(i), Mathf.FloorToInt(j)].GetComponent<RectTransform>().localPosition = new Vector2((gameObject.transform.localPosition.x + Mathf.FloorToInt(i)) * 25, (gameObject.transform.localPosition.y + Mathf.FloorToInt(j)) * 25);
                    mapindex[Mathf.FloorToInt(i), Mathf.FloorToInt(j)].transform.parent = gameObject.transform;
                    mapindex[i, j].GetComponent<DistanceDisplay>().positionX = i;
                    mapindex[i, j].GetComponent<DistanceDisplay>().positionY = j;
                }
            }
        }
    }
    //find must have building and return it. 
    //otherwise find a building and return if random hits above the threshold. 
    //otherwise return the second building
    GameObject what2put()
    {
        int prob = 0;
        for (int i = 0; i < buildings.Length; i++)
        {
            if (musthave1[i])
            {
                musthave1[i] = false;
                return buildings[i];
            }
        }

        for (int i = 0; i < buildings.Length; i++)
        {
            prob = Random.Range(0, 101);
            if (probabilities[i] > prob)
            {
                probabilities[i] = 0;
                return buildings[i];
            }
            else
            {
                probabilities[i] = 0;
            }

        }

        return genericHouse[Random.Range(0, genericHouse.Length)];
    }

    // check if a spot is avaliable based on the size
    bool isavailable(int hsize, int vsize, int x, int y)
    {
        if (x == 7 && hsize == 1)
        {
            return false;
        }
        else if (y == 6 && vsize == 1)
        {
            return false;
        }
        else if (mapslots[x, y] == false && mapslots[x + hsize, y + vsize] == false && mapslots[x + hsize, y] == false && mapslots[x, y + vsize] == false)
        {
            return true;
        }
        return false;
    }

    //pick random spots and check if its available for a size and return the spot
    Vector2 where2put(int hsize, int vsize)
    {

        int x = Random.Range(0, 8 - hsize);
        int y = Random.Range(0, 7 - vsize);
        if (isavailable(hsize, vsize, x, y))
        {
            return new Vector2(x, y);
        }

        x = Random.Range(0, 8 - hsize);
        y = Random.Range(0, 7 - vsize);
        if (isavailable(hsize, vsize, x, y))
        {
            return new Vector2(x, y);
        }

        x = Random.Range(0, 8 - hsize);
        y = Random.Range(0, 7 - vsize);
        if (isavailable(hsize, vsize, x, y))
        {
            return new Vector2(x, y);
        }

        for (int i = 0; i < 8 - hsize; i++)
        {
            for (int j = 0; j < 7 - vsize; j++)
            {
                if (isavailable(hsize, vsize, i, j))
                {
                    return (new Vector2(i, j));
                }
            }
        }
        return new Vector2(-2, -0);
    }

    // put the object on the map and paint true on the index based on size
    void put(GameObject building, Vector2 on)
    {
        int index = 0;
        int hsize = 0;
        int vsize = 0;

        for (int i = 0; i < buildings.Length; i++)
        {
            if (GameObject.ReferenceEquals(building, buildings[i]))
            {
                index = i;
                if (horsize[index])
                    hsize = 1;
                if (versize[index])
                    vsize = 1;
            }
        }

        if (isavailable(hsize, vsize, Mathf.FloorToInt(on.x), Mathf.FloorToInt(on.y)))
        {
            mapslots[Mathf.FloorToInt(on.x), Mathf.FloorToInt(on.y)] = true;
            mapslots[Mathf.FloorToInt(on.x), Mathf.FloorToInt(on.y) + vsize] = true;
            mapslots[Mathf.FloorToInt(on.x) + hsize, Mathf.FloorToInt(on.y)] = true;
            mapslots[Mathf.FloorToInt(on.x) + hsize, Mathf.FloorToInt(on.y) + vsize] = true;



            if (index == 0)
            {
                for (int i = 0; i < genericHouse.Length; i++)
                {
                    if (GameObject.ReferenceEquals(genericHouse[i], building))
                    {
                        maptypeindex[Mathf.FloorToInt(on.x), Mathf.FloorToInt(on.y)] = i + 1000;
                    }
                }
            }
            else
            {
                maptypeindex[Mathf.FloorToInt(on.x), Mathf.FloorToInt(on.y)] = index;
            }

            GameObject obj = Instantiate<GameObject>(building);
            mapindex[Mathf.FloorToInt(on.x), Mathf.FloorToInt(on.y)] = obj;


            obj.transform.SetParent(gameObject.transform);

            print("(on.x*offset):" + (on.x * offset));
            print("gameObject.GetComponent<RectTransform>().localPosition.x):");
            print("button_size_x" + button_size_x);
            print("hsize" + hsize);
            print("(button_size_x/2)" + (button_size_x / 2));

            obj.GetComponent<RectTransform>().localPosition = new Vector2((float)((on.x * offset_x) + (on.x) * button_size_x + hsize * (button_size_x / 2)),
                                                                          (float)((on.y * offset_y) + (on.y) * button_size_y + vsize * (button_size_y / 2)));
            obj.GetComponent<RectTransform>().sizeDelta = new Vector2(Screen.height / screenratio * (hsize + 1), Screen.height / screenratio * (vsize + 1));

            DistanceDisplay ddscript = obj.AddComponent<DistanceDisplay>();

            int newx = (int)on.x;
            int newy = (int)on.y;
            if (vsize == 1 && on.y < 3)
            {
                newy = (int)(on.y + vsize);
            }
            if (hsize == 1)
            {
                newx = (int)(on.x + hsize);
            }

            Vector2 v1 = new Vector2((float)7, (float)3);
            Vector2 v2 = new Vector2((float)newx, (float)newy);


            float xdif = v1.x - v2.x;
            float ydif = v1.y - v2.y;
            float xkareler = Mathf.Pow(xdif, 2);
            float ykareler = Mathf.Pow(ydif, 2);
            float karelertop = Mathf.Sqrt(ykareler + xkareler);


            ddscript.distance = Mathf.FloorToInt(karelertop);
        }
        else
        {
            print("NOP");
        }

    }

    //is there a empty point on the map
    bool isthereaslot()
    {
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 7; j++)
            {
                if (mapslots[i, j] == false)
                {
                    return true;
                }
            }
        }

        return false;
    }

}
