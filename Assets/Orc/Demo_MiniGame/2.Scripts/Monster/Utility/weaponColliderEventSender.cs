using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class weaponColliderEventSender : MonoBehaviour {
    public enum Type
    {
        Mons,
        Player,
        Soldier

    }

    public  enum AttackState
    {
        Default,
        Skill1,
        Skill2,
        Skill3,
        Skill4


    }
    public Type CharacterType = Type.Player;


    public PlayerController m_PlayerRoot;
    public Mon_Bass m_MonsterRoot;
    public AttackState m_AttackState = AttackState.Default;

    public List<GameObject> HittedObjectList = new List<GameObject>();

    void Start()
    {


        switch (CharacterType)
        {
            case Type.Mons:
                m_MonsterRoot = this.transform.root.transform.GetComponent<Mon_Bass>();        
                break;
            case Type.Player:
                m_PlayerRoot = this.transform.root.transform.GetComponent<PlayerController>();
                break;
     
        }
    }

    void OnEnable()
    {
        if(HittedObjectList.Count>0)
            HittedObjectList.Clear();

    }

    void OnDisable()
    {
        HittedObjectList.Clear();

    }


    void OnTriggerStay2D(Collider2D other)
    {

    
         // Debug.Log("othe1111r::" + other.name);
        if (!HittedObjectList.Contains(other.gameObject))
        {
            HittedObjectList.Add(other.gameObject);
        }
        else
        {
            return;
        }
   //      Debug.Log("2222::" + other.name);

        switch (CharacterType)
        {
            case Type.Mons:


                switch (m_AttackState)
                {
                    case AttackState.Default:
                        m_MonsterRoot.DefaulAttack_Collider(other.gameObject);
                        break;
                    case AttackState.Skill1:
                        m_MonsterRoot.Skill_1Attack_Collider(other.gameObject);
                        break;
                    case AttackState.Skill2:
                        m_MonsterRoot.Skill_2Attack_Collider(other.gameObject);
                        break;
                    case AttackState.Skill3:
                        m_MonsterRoot.Skill_3Attack_Collider(other.gameObject);
                        break;
                    case AttackState.Skill4:
                        m_MonsterRoot.Skill_4Attack_Collider(other.gameObject);
                        break;

                }



                break;
            case Type.Player:


                switch (m_AttackState)
                {
                    case AttackState.Default:
                        m_PlayerRoot.DefaulAttack_Collider(other.gameObject);
                        break;
                    case AttackState.Skill1:
                        m_PlayerRoot.Skill_1Attack_Collider(other.gameObject);
                        break;
                    case AttackState.Skill2:
                        m_PlayerRoot.Skill_2Attack_Collider(other.gameObject);
                        break;
                    case AttackState.Skill3:
                        m_PlayerRoot.Skill_3Attack_Collider(other.gameObject);
                        break;
                    case AttackState.Skill4:
                        m_PlayerRoot.Skill_4Attack_Collider(other.gameObject);
                        break;

                }





                break;
            case Type.Soldier:

                break;


        }


      //  m_MonsterRoot.OnAttackCollision(other.gameObject);



    }
    
}
