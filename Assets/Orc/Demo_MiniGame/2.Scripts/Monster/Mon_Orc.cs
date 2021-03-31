using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Lofle;


public class Mon_Orc_Boss : Mon_Bass
{

    protected StateMachine<Mon_Orc_Boss> _stateMachine = null;

  

    //public PhotonView m_Photonview;

    public override void Init()
    {
       
             _stateMachine = new StateMachine<Mon_Orc_Boss>(this);
            StateCo = StartCoroutine(_stateMachine.Coroutine<RunState>());

        

    }

    public override void DefaulAttack_Collider(GameObject obj)
    {
    

        if (obj.CompareTag("Player") || obj.CompareTag("Tree"))
        {

            //Debug.Log("dfadfjakfd:::" + obj);
           // Is_OnceAttack = false;
            //   Debug.Log("Core::"+ obj.name);
            if (obj.CompareTag("Player") || obj.CompareTag("Tree")) // 맞는 처리는 서버에서만 보내준다. 
            {
                Bird tmp_Player = obj.GetComponent<Bird>();

       
                if(tmp_Player != null)
                {
                    Vector2 dir = new Vector2(0, 0);
                    tmp_Player.isDead = true;
                }
                else
                {
                    Tree tree = obj.GetComponent<Tree>();
                    tree.hitPoints--;
                }
                

            }
          
        }


    }


    public override void Skill_1Attack_Collider(GameObject obj)
    {

      



    }
    public override void Skill_2Attack_Collider(GameObject obj)
    {

      



    }
    public override void Skill_3Attack_Collider(GameObject obj)
    {

   



    }
    public override void Skill_4Attack_Collider(GameObject obj)
    {

    }


    public bool b_DefaultAttack_Anim = false;

    public override void DefaultAttack_Anim_1_Enter()
    {
 
        b_DefaultAttack_Anim = true;

    }

    public override void DefaultAttack_Anim_1_Exit()
    {
        b_DefaultAttack_Anim = false;
    }



    void Update()
    {
      
       
    }

 
    public override void Damaged(float DamageValue, Vector2 dir, float stunTime)
    {

        if (IsDie)
            return;

        if (stunTime > 0)
            HittedFuc(stunTime);

        m_rigidbody2D.velocity = new Vector2(0, 0);
        m_rigidbody2D.AddForce(dir, ForceMode2D.Impulse);

 

        float PreHP = m_HP;
        m_HP -= DamageValue;


        if (DamageValue > 0)
            SetCreateBloodEffect(DamageValue);

        //  photonView.RPC("RPC_SetCreateBloodEffect", PhotonTargets.All, DamageValue);


        SyncHp(m_HP);
  

        if (m_HP <= 0)
        {
            IsDie = true;
            StopCoroutine(StateCo);
            StateCo = StartCoroutine(_stateMachine.Coroutine<DieState>());



        }

    }



    public float StuneTime;
    public override void HittedFuc(float stunTime)
    {
        StuneTime = stunTime;
        StopCoroutine(StateCo);
        StateCo = StartCoroutine(_stateMachine.Coroutine<HitState>());
  
    }




    private class IdleState : State<Mon_Orc_Boss>
    {
        protected override void Begin()
        {
            Owner.SetAnim("Demo_Orc_boss_Idle");
            TimeTic = 0;
            Owner.MoveDir = Vector2.zero;
        }

        private float RandomTime=0.2f;
        private float TimeTic = 0;
        protected override void Update()
        {

            TimeTic += Time.deltaTime;
            if (TimeTic > RandomTime)
            {
                TimeTic = 0;


                if (Owner.Current_Tartget != null)
                {
         

                    float CurrentEnermyDis = Vector2.Distance(Owner.Current_Tartget.transform.position, Owner.transform.position);

                    if (CurrentEnermyDis <= Owner.AttackDis)
                    {
                        Invoke<AttackState>();
                    }
                    else
                    {
                        Invoke<RunState>();
                    }

                }
                else
                {
                    Invoke<IdleState>();

                }

            }



        }

        protected override void End()
        {

        }

    }


    private class RunState : State<Mon_Orc_Boss>
    {
        protected override void Begin()
        {

            Owner.SetAnim("Demo_Orc_boss_Run");

            //     Debug.Log("Run");

        }

        private float updateTimeTic = 0;
        private float updateTime = 0.1f;

        protected override void Update()
        {


            if (Owner.Current_Tartget == null)
            {
                if(Owner.Constant_Target != null)
                {
                    Owner.Move();
                }
                else
                {
                    Invoke<IdleState>();
                }
                
                return;
            }
              
            Owner.Move();
         


            updateTimeTic += Time.deltaTime;
            if (updateTimeTic > updateTime)
            {
                updateTimeTic = 0;

         
                float CurrentEnermyDis = Vector2.Distance(Owner.Current_Tartget.transform.position, Owner.transform.position);

                if (CurrentEnermyDis <= Owner.AttackDis)
                {
                    Invoke<AttackState>();
                }
        

                
            }

        }

        protected override void End()
        {

        }
    }


    private class AttackState : State<Mon_Orc_Boss>
    {
        protected override void Begin()
        {
       
            Owner.MoveDir = Vector2.zero;
            Owner.SetAnim("Demo_Orc_boss_Attack");
         


        }

        //private float updateTimeTic = 0;
       // private float updateTime = 0.1f;

        protected override void Update()
        {

            if (Owner.Current_Tartget == null)
            {
                Invoke<IdleState>();
                return;
            }


            if (!Owner.Current_Tartget.activeSelf)
                Invoke<RunState>();



            if (!Owner.b_DefaultAttack_Anim)
                {
                // Debug.Log("애니메이션 끝::");





                float CurrentEnermyDis = Vector2.Distance(Owner.Current_Tartget.transform.position, Owner.transform.position);
                Owner.Filp();
                if (CurrentEnermyDis > Owner.AttackDis)
                    {
                        Invoke<RunState>();
                    }
                }

            


        }

        protected override void End()
        {
        }
    }

   
    private class HitState : State<Mon_Orc_Boss>
    {
        protected override void Begin()
        {
            Owner.SetAnim("Demo_Orc_boss_Hit");
            TimeTic = 0;
            Owner.MoveDir = Vector2.zero;
        }

        private float TimeTic = 0;
        protected override void Update()
        {
         

            TimeTic += Time.deltaTime;
            if (TimeTic > Owner.StuneTime)
            {
                TimeTic = 0;

   
                Invoke<IdleState>();


            }
        }

        protected override void End()
        {

        }

    }






    private class DieState : State<Mon_Orc_Boss>
    {
        protected override void Begin()
        {
            Owner.SetAnim("Demo_Orc_boss_Die");
            DieTime = 2;
            Owner.MoveDir = Vector2.zero;
        }
        private float DieTime = 2;
        private float TimeTic = 0;
        protected override void Update()
        {
          
            TimeTic += Time.deltaTime;
            if (TimeTic > DieTime)
            {
                TimeTic = 0;
                Owner.SetDie();
                Destroy(Owner.gameObject);
            }

        }

        protected override void End()
        {

        }

    }


}
