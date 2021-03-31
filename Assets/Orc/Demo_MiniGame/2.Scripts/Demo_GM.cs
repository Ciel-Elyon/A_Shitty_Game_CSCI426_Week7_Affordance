using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;




public class Demo_GM : MonoBehaviour {



    public static Demo_GM Gm;



    public Sprite[] Idle_SwapImageGroup;
    public Sprite[] Pressed_SwapImageGroup;

    public Image[] UIImage;

    public List<GameObject> MonsterList = new List<GameObject>();
    public GameObject PlayerObj;

    // Use this for initialization
    void Awake () {
        Screen.fullScreen = false;

        Gm = this;


        Physics2D.IgnoreLayerCollision(0, 1);
        
    }
	
	// Update is called once per frame
	void Update () {

        KeyUPDownchange();


    }


    void InitColor()
    {

        for (int i = 0; i < UIImage.Length; i++)
        {
            UIImage[i].color = new Color(255, 255, 255);


        }

    }

    public void KeyUPDownchange()
    {
        // Idle
        if (Input.GetKeyUp(KeyCode.A))
        {
            //Color myColor = new Color32(255, 255, 255, 255);

            //Demo_GM.Gm.UIImage[2].color = myColor;

            Demo_GM.Gm.UIImage[2].sprite = Idle_SwapImageGroup[2];
        }
     
        if (Input.GetKeyUp(KeyCode.D))
        {
            Demo_GM.Gm.UIImage[3].sprite = Idle_SwapImageGroup[3];
        }

        if (Input.GetKeyUp(KeyCode.W))
        {
            Demo_GM.Gm.UIImage[0].sprite = Idle_SwapImageGroup[0];
        }
        if (Input.GetKeyUp(KeyCode.S))
        {
            Demo_GM.Gm.UIImage[1].sprite = Idle_SwapImageGroup[1];
        }
        if (Input.GetKeyUp(KeyCode.Mouse0))
        {
            Demo_GM.Gm.UIImage[4].sprite = Idle_SwapImageGroup[4];
        }
        if (Input.GetKeyUp(KeyCode.Mouse1))
        {
            Demo_GM.Gm.UIImage[5].sprite = Idle_SwapImageGroup[5];
        }
        if (Input.GetKeyUp(KeyCode.Alpha1))
        {

            Demo_GM.Gm.UIImage[6].sprite = Idle_SwapImageGroup[6];
        }
        if (Input.GetKeyUp(KeyCode.Space))
        {

            Demo_GM.Gm.UIImage[7].sprite = Idle_SwapImageGroup[7];
        }


        //pressed
        if (Input.GetKeyDown(KeyCode.A))
        {
            Demo_GM.Gm.UIImage[2].sprite = Pressed_SwapImageGroup[2];
        }

        if (Input.GetKeyDown(KeyCode.D))
        {
            Demo_GM.Gm.UIImage[3].sprite = Pressed_SwapImageGroup[3];
        }

        if (Input.GetKeyDown(KeyCode.W))
        {
            Demo_GM.Gm.UIImage[0].sprite = Pressed_SwapImageGroup[0];
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            Demo_GM.Gm.UIImage[1].sprite = Pressed_SwapImageGroup[1];
        }

        if (Input.GetKeyDown(KeyCode.Mouse0))
        {

            Demo_GM.Gm.UIImage[4].sprite = Pressed_SwapImageGroup[4];

        }

        if (Input.GetKeyDown(KeyCode.Mouse1))
        {


            Demo_GM.Gm.UIImage[5].sprite = Pressed_SwapImageGroup[5];

        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            Demo_GM.Gm.UIImage[6].sprite = Pressed_SwapImageGroup[6];
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Demo_GM.Gm.UIImage[7].sprite = Pressed_SwapImageGroup[7];
        }

    }


    public GameObject[] RespawnPosObjs;
    public GameObject[] MonsterPrefabs;
    public GameObject smokePrefab;

    public void Create_Sword()
    {
        int idx = Random.Range(0, 2);


        GameObject monobj=  Instantiate(MonsterPrefabs[0], RespawnPosObjs[idx].transform.position, Quaternion.identity);
        Instantiate(smokePrefab, RespawnPosObjs[idx].transform.position, Quaternion.identity);

        MonsterList.Add(monobj);
    }
    public void Create_Axe()
    {
        int idx = Random.Range(0, 2);


        GameObject monobj = Instantiate(MonsterPrefabs[1], RespawnPosObjs[idx].transform.position, Quaternion.identity);
        Instantiate(smokePrefab, RespawnPosObjs[idx].transform.position, Quaternion.identity);

        MonsterList.Add(monobj);
    }
    public void Create_Hammer()
    {
        int idx = Random.Range(0, 2);


        GameObject monobj = Instantiate(MonsterPrefabs[2], RespawnPosObjs[idx].transform.position, Quaternion.identity);
        Instantiate(smokePrefab, RespawnPosObjs[idx].transform.position, Quaternion.identity);
        MonsterList.Add(monobj);
    }
    public void Create_Wizard()
    {
        int idx = Random.Range(0, 2);


        GameObject monobj = Instantiate(MonsterPrefabs[3], RespawnPosObjs[idx].transform.position, Quaternion.identity);
        Instantiate(smokePrefab, RespawnPosObjs[idx].transform.position, Quaternion.identity);
        MonsterList.Add(monobj);
    }
    public void Create_Spear()
    {
        int idx = Random.Range(0, 2);


        GameObject monobj = Instantiate(MonsterPrefabs[4], RespawnPosObjs[idx].transform.position, Quaternion.identity);
        Instantiate(smokePrefab, RespawnPosObjs[idx].transform.position, Quaternion.identity);
        MonsterList.Add(monobj);
    }
    public void Create_Archer()
    {
        int idx = Random.Range(0, 2);


        GameObject monobj = Instantiate(MonsterPrefabs[5], RespawnPosObjs[idx].transform.position, Quaternion.identity);
        Instantiate(smokePrefab, RespawnPosObjs[idx].transform.position, Quaternion.identity);
        MonsterList.Add(monobj);
    }
    public void Create_Sniper()
    {
        int idx = Random.Range(0, 2);


        GameObject monobj = Instantiate(MonsterPrefabs[6], RespawnPosObjs[idx].transform.position, Quaternion.identity);
        Instantiate(smokePrefab, RespawnPosObjs[idx].transform.position, Quaternion.identity);
        MonsterList.Add(monobj);
    }

}
