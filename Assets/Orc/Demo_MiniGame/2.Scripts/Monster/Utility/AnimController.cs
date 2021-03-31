using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class AnimController : MonoBehaviour {


    public enum Type
    {
        Mons,
        Player,
    }
    public Type CharacterType = Type.Mons;
    public PlayerController m_PlayerRoot;
    public Mon_Bass m_MonsterRoot;

  //  public event Action<GameObject> TriggerEnter;
  //  public event Action<GameObject> TriggerExit;
    // Use this for initialization
    void Start () {

        switch (CharacterType)
        {
            case Type.Mons:
                m_MonsterRoot = this.transform.root.transform.GetComponent<Mon_Bass>();
                break;
            case Type.Player:
                m_PlayerRoot = this.transform.root.transform.GetComponent<PlayerController>();
                break;
     


        }


        //if (!ISMonster)
            
        //else
            
    }



    public void Anim_DefaultAttack_Enter()
    {
  
        switch (CharacterType)
        {
            case Type.Mons:
                    m_MonsterRoot.DefaultAttack_Anim_1_Enter();
                 
                break;
            case Type.Player:
                    m_PlayerRoot.DefaultAttack_Anim_1_Enter();
                    break;
      


        }

    }
    public void Anim_DefaultAttack_Exit()
    {
   
        switch (CharacterType)
        {
            case Type.Mons:
                m_MonsterRoot.DefaultAttack_Anim_1_Exit();

                break;
            case Type.Player:
                m_PlayerRoot.DefaultAttack_Anim_1_Exit();
                break;
        


        }


    }



    public void Anim_AttackSkill_1_Enter()
    {
       

        switch (CharacterType)
        {
            case Type.Mons:
                m_MonsterRoot.SkillAttack_Anim_1_Enter();

                break;
            case Type.Player:
                m_PlayerRoot.SkillAttack_Anim_1_Enter();
                break;
       


        }



    }
    public void Anim_AttackSkill_1_Exit()
    {

        switch (CharacterType)
        {
            case Type.Mons:
                m_MonsterRoot.SkillAttack_Anim_1_Exit();

                break;
            case Type.Player:
                m_PlayerRoot.SkillAttack_Anim_1_Exit();
                break;
         


        }

    }



    public void Anim_AttackSkill_2_Enter()
    {
   
        switch (CharacterType)
        {
            case Type.Mons:
                m_MonsterRoot.SkillAttack_Anim_2_Enter();

                break;
            case Type.Player:
                m_PlayerRoot.SkillAttack_Anim_2_Enter();
                break;
         


        }

    }
    public void Anim_AttackSkill_2_Exit()
    {
       
        switch (CharacterType)
        {
            case Type.Mons:
                m_MonsterRoot.SkillAttack_Anim_2_Exit();

                break;
            case Type.Player:
                m_PlayerRoot.SkillAttack_Anim_2_Exit();
                break;
         


        }


    }



    public void Anim_AttackSkill_3_Enter()
    {
       

        switch (CharacterType)
        {
            case Type.Mons:
                m_MonsterRoot.SkillAttack_Anim_3_Enter();

                break;
            case Type.Player:
                m_PlayerRoot.SkillAttack_Anim_3_Enter();
                break;
         


        }


    }
    public void Anim_AttackSkill_3_Exit()
    {
     
        switch (CharacterType)
        {
            case Type.Mons:
                m_MonsterRoot.SkillAttack_Anim_3_Exit();

                break;
            case Type.Player:
                m_PlayerRoot.SkillAttack_Anim_3_Exit();
                break;
          


        }


    }


    public void Anim_AttackSkill_4_Enter()
    {
      

        switch (CharacterType)
        {
            case Type.Mons:
                m_MonsterRoot.SkillAttack_Anim_4_Enter();

                break;
            case Type.Player:
                m_PlayerRoot.SkillAttack_Anim_4_Enter();
                break;
        


        }


    }
    public void Anim_AttackSkill_4_Exit()
    {
       
        switch (CharacterType)
        {
            case Type.Mons:
                m_MonsterRoot.SkillAttack_Anim_4_Exit();

                break;
            case Type.Player:
                m_PlayerRoot.SkillAttack_Anim_4_Exit();
                break;
      

        }

    }



}
