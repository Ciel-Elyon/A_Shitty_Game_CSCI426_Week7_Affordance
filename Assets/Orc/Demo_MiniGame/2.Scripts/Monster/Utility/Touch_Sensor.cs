using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class Touch_Sensor : MonoBehaviour {

    public enum Type
    {
        Mons,
        Player
    }
    public Type CharacterType = Type.Mons;
    public Mon_Bass m_MonBass;
    public PlayerController m_Playerroot;


    public event Action<Collider2D> TriggerEnter;
    public event Action<GameObject> TriggerExit;
    // Use this for initialization
    void Start () {
        TriggerEnter -= MonsTriggerEnter;
        TriggerExit -= MonsOnTriggerExit2D;
        TriggerEnter -= PlayerTriggerEnter;
        TriggerExit -= PlayerOnTriggerExit2D;



        switch (CharacterType)
        {
            case Type.Mons:
                TriggerEnter += MonsTriggerEnter;
                TriggerExit += MonsOnTriggerExit2D;
                m_MonBass = this.transform.root.GetComponent<Mon_Bass>();
                break;
            case Type.Player:
                TriggerEnter += PlayerTriggerEnter;
                TriggerExit += PlayerOnTriggerExit2D;

                m_Playerroot = this.transform.root.GetComponent<PlayerController>();
                break;
   

        }
  
          

    }


     void OnTriggerStay2D(Collider2D other)
     {

        TriggerEnter(other);
    }
     
     void OnTriggerExit2D(Collider2D other)
     {

        TriggerExit(other.gameObject);
    }


    public void MonsTriggerEnter(Collider2D other)
    {
        if(other.CompareTag("Player"))
        m_MonBass.Touch_SensorEnter(other);
    }
    public void MonsOnTriggerExit2D(GameObject other)
    {
        if (other.CompareTag("Player"))
            m_MonBass.Touch_SensorExit(other.gameObject);
    }

    public void PlayerTriggerEnter(Collider2D other)
    {
      //  m_Playerroot.Touch_SensorEnter(other.gameObject);
    }
    public void PlayerOnTriggerExit2D(GameObject other)
    {
      //  m_Playerroot.Touch_SensorExit(other.gameObject);
    }



}
