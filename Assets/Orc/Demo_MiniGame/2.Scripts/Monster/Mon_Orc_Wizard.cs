using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Lofle;


public class Mon_Orc_Wizard : Mon_Bass
{

    public float StuneTime;
    protected StateMachine<Mon_Orc_Wizard> _stateMachine = null;
    public bool b_SkillAttack_Anim_1 = false;
    public bool b_SkillAttack_Anim_2 = false;




    [Header("[Setting_2]")]

    public GameObject Skill_1_Socket;





    //public PhotonView m_Photonview;

    public override void Init()
    {


        //  m_Anim = this.transform.Find("model").GetComponent<Animator>();
        //  m_Canvas_Trans = this.transform.Find("Canvas").transform;
        ////  m_Anim.Play("Idle");
        //  HpBarImage = m_Canvas_Trans.Find("HpBar").Find("HpImage").GetComponent<Image>();

        //  Tartget_Core = GameObject.FindGameObjectWithTag("Core");
        //  m_rigidbody2D = this.transform.GetComponent<Rigidbody2D>();



        //  Current_Tartget = Tartget_Core;



            _stateMachine = new StateMachine<Mon_Orc_Wizard>(this);
            StateCo = StartCoroutine(_stateMachine.Coroutine<RunState>());

       

    }


    void Update()
    {
       

    }


   
    public override void Damaged(float DamageValue, Vector2 dir, float stunTime)
    {
        //  AttackPlayer = Attacker;
        SetDamaged(  DamageValue, dir, stunTime);

    }


  
    public void SetDamaged(float DamageValue, Vector2 dir, float stunTime)
    {

        if (IsDie)
            return;

        if (stunTime > 0)
            HittedFuc(stunTime);

        m_rigidbody2D.velocity = new Vector2(0, 0);
        m_rigidbody2D.AddForce(dir, ForceMode2D.Impulse);

        //  int tmpid = PlayerManagement.Instance.PlayerObjList.FindIndex(x => x.GetComponent<PlayerController>().photonView.ownerId == Playerid);

        //  Debug.Log("tmpId::" + tmpid);
        //    TargetSwitch(PlayerManagement.Instance.PlayerObjList[tmpid]);

        float PreHP = m_HP;
        m_HP -= DamageValue;

        if (DamageValue > 0)
            SetCreateBloodEffect(DamageValue);

        //  photonView.RPC("RPC_SetCreateBloodEffect", PhotonTargets.All, DamageValue);


         SyncHp( m_HP);
      
        if (m_HP <= 0)
        {
            IsDie = true;
            m_HP = 0;
        
            StopCoroutine(StateCo);
            StateCo = StartCoroutine(_stateMachine.Coroutine<DieState>());



            //InGameManager.Instance.Del_mini_Monster(this.gameObject);


        }
        

    }




    public override void HittedFuc(float stunTime)
    {
        StuneTime = stunTime;
        StopCoroutine(StateCo);
        StateCo = StartCoroutine(_stateMachine.Coroutine<HitState>());
       
    }

    



    public override void DefaulAttack_Collider(GameObject obj)
    {
       

      

        if (obj.CompareTag("Player"))
        {

                Bird tmp_Player = obj.transform.root.GetComponent<Bird>();

                if(tmp_Player != null)
            {
                Vector2 dir = new Vector2(0, 0);


                tmp_Player.isDead = true;

            }


        }


    }


    public override void Skill_1Attack_Collider(GameObject obj)
    {

    
        if (obj.CompareTag("Player"))
        {

            Bird tmp_Player = obj.transform.root.GetComponent<Bird>();


            if (tmp_Player != null)
            {
                Vector2 dir = new Vector2(0, 0);


                tmp_Player.isDead = true;

            }




        }
    }

    public override void Skill_2Attack_Collider(GameObject obj)
    {

        

        if (obj.CompareTag("Player"))
        {

            Bird tmp_Player = obj.transform.root.GetComponent<Bird>();


            if (tmp_Player != null)
            {
                Vector2 dir = new Vector2(0, 0);


                tmp_Player.isDead = true;

            }



        }

    }
    public override void Skill_3Attack_Collider(GameObject obj)
    {

        if (obj.CompareTag("Monster") )
        {


        }



    }
    public override void Skill_4Attack_Collider(GameObject obj)
    {

    }


      public bool b_DefaultAttack_Anim = false;
      
      public override void DefaultAttack_Anim_1_Enter()
      {
      
          b_DefaultAttack_Anim = true;
         // Debug.Log("Attack1공격");
        //CreateArrow();
        GameObject tmpObj = Instantiate(FireBall_castingPrefab, Skill_1_Socket.transform.position, Quaternion.identity);
        tmpObj.transform.SetParent(Skill_1_Socket.transform);
    }




    public GameObject FireBall_castingPrefab;
    public GameObject FireBallPrefab;
    public GameObject HealPrefab;
    public GameObject HastePrefab;
    public override void SkillAttack_Anim_1_Enter()
    {

        
        b_SkillAttack_Anim_1 = true;

    

       
           
            GameObject tmpObj = Instantiate(FireBallPrefab, Skill_1_Socket.transform.position, Quaternion.identity);

         
            tmpObj.GetComponent<FireballScript>().Fire(Current_Tartget, m_Damage);
        


    }



    public override void SkillAttack_Anim_1_Exit()
    {
        b_SkillAttack_Anim_1 = false;

    }

    public override void SkillAttack_Anim_2_Enter()
    {


        b_SkillAttack_Anim_2 = true;

        // 맵 전지역 애들한테 광역 힐을 준다. 30%가 찬다. 
        for (int i = 0; i < EnemySpawn.es.MonsterList.Count; i++)
        {

            if (EnemySpawn.es.MonsterList[i] == null)
                continue;

            Vector2 pos = new Vector2(EnemySpawn.es.MonsterList[i].transform.position.x, EnemySpawn.es.MonsterList[i].transform.position.y);

            Instantiate(HealPrefab, pos, Quaternion.identity);

            EnemySpawn.es.MonsterList[i].GetComponent<Mon_Bass>().Damaged(-50,  new Vector2(0, 0), 0);


        }


    }

    public override void SkillAttack_Anim_2_Exit()
    {
        b_SkillAttack_Anim_2 = false;

        //   Debug.Log("취소222");
    }



    public override void SkillAttack_Anim_3_Enter()
    {
      

     
        // 맵 전지역 애들한테 광역 힐을 준다. 30%가 찬다. 
        for (int i = 0; i < EnemySpawn.es.MonsterList.Count; i++)
        {

            //   Debug.Log("InGameManager.Instance.MonsterList::" + InGameManager.Instance.MonsterList[i].gameObject.name);

            Vector2 pos = new Vector2(EnemySpawn.es.MonsterList[i].transform.position.x,EnemySpawn.es.MonsterList[i].transform.position.y );

            Instantiate(HastePrefab, pos, Quaternion.identity);

            EnemySpawn.es.MonsterList[i].GetComponent<Mon_Bass>().Damaged(-50, new Vector2(0, 0), 0);


        }


        StartCoroutine(HasteTimmer_Co());
    }

    IEnumerator HasteTimmer_Co()
    {

      

        for (int i = 0; i < EnemySpawn.es.MonsterList.Count; i++)
        {
             EnemySpawn.es.MonsterList[i].GetComponent<Mon_Bass>().SetAttackSpeed(2f);
             EnemySpawn.es.MonsterList[i].GetComponent<Mon_Bass>().SetMoveSpeed(1);
        }

        yield return new WaitForSeconds(3);
      
        for (int i = 0; i < EnemySpawn.es.MonsterList.Count; i++)
        {
             EnemySpawn.es.MonsterList[i].GetComponent<Mon_Bass>().SetAttackSpeed(1);
            EnemySpawn.es.MonsterList[i].GetComponent<Mon_Bass>().SetMoveSpeed(1);
        }


    }




    private class IdleState : State<Mon_Orc_Wizard>
    {
        protected override void Begin()
        {
            Owner.SetAnim("Demo_Idle");
            RandomTime = 0.5f;
            Owner.MoveDir = Vector2.zero;
         //   Debug.Log("IDle");
        }

        private float RandomTime;
        private float TimeTic = 1;
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


    private class RunState : State<Mon_Orc_Wizard>
    {
        protected override void Begin()
        {
           
                Owner.SetAnim("Demo_Run");
         //   Debug.Log("Run");
        }


        float updateTimeTic = 0;
        private float updateTime = 1f;

        protected override void Update()
        {


            if (Owner.Current_Tartget == null)
            {
                Invoke<IdleState>();
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

    private class AttackState : State<Mon_Orc_Wizard>
    {
        protected override void Begin()
        {

            Owner.SetAnim("Demo_Attack_Wizard");

        }


        float UpdateTic = 0;
        protected override void Update()
        {



            if (Owner.Current_Tartget == null)
            {
                Invoke<IdleState>();
                return;
            }


            if (!Owner.m_Anim.GetCurrentAnimatorStateInfo(0).IsName("Demo_Attack_Wizard"))
            {
                // Debug.Log("애니메이션 끝::");
                Owner.Filp();

                float CurrentEnermyDis = Vector2.Distance(Owner.Current_Tartget.transform.position, Owner.transform.position);
                //  Owner.Filp();
                if (CurrentEnermyDis > Owner.AttackDis)
                {
                    Invoke<RunState>();
                }
                else
                {
                    int tmpRandom = Random.Range(1, 101);
                    if (Owner.Skill_Chance > tmpRandom)
                    {
                        //   Debug.Log("스킬 발동::30%"+tmpRandom);
                        int[] tmpID = new int[] { 1, 2 };

                        if (Owner.Search_usable_Skill(tmpID))
                        {
                            int tmpSkillId = Random.Range(0, Owner.usable_Skill_Id.Count);
                            // Debug.Log("스킬뽑기:" + tmpSkillId+"::::"+ Owner.usable_Skill_Id[tmpSkillId]);
                            //   Debug.Log("스킬뽑기::::" + tmpSkillId);
                            switch (Owner.usable_Skill_Id[tmpSkillId])
                            {
                                case 1:
                                    Invoke<Skill_2State>();
                                    break;
                                case 2:
                                    Invoke<Skill_3State>();
                                    break;
                            }
                        }


                    }




                }

            }

            //if (!Owner.b_SkillAttack_Anim_1)
            //{
               
            //}

  




        }

        protected override void End()
        {

        }
    }


    private class HitState : State<Mon_Orc_Wizard>
    {
        protected override void Begin()
        {
            Owner.SetAnim("Demo_Hit");
            //  RandomTime = Random.Range(1, 2);
            TimeTic = 0;
            Owner.MoveDir = Vector2.zero;
        }

        private float TimeTic = 0;
        protected override void Update()
        {
         


            TimeTic += Time.deltaTime;
            //Debug.Log("스턴::" + TimeTic);
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

    private class DieState : State<Mon_Orc_Wizard>
    {
        protected override void Begin()
        {
            Owner.SetAnim("Demo_Die");
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

    private class Skill_2State : State<Mon_Orc_Wizard>
    {
        protected override void Begin()
        {
           // Debug.Log("Skill_2");
            Owner.b_Skill[1] = false;
            Owner.SetAnim("Demo_Spell_Wizard_1");
            Owner.MoveDir = Vector2.zero;
            Owner.StartCoroutine(Owner.SkillCoolTimmer(1));
        }

        protected override void Update()
        {
          
            if (Owner.m_Anim.GetCurrentAnimatorStateInfo(0).IsName("Demo_Spell_Wizard_1"))
            {


            }
            else
            {
                Invoke<IdleState>();
            }


        }

        protected override void End()
        {

        }

    }

    private class Skill_3State : State<Mon_Orc_Wizard>
    {
        protected override void Begin()
        {
           // Debug.Log("Skill_3");
            Owner.b_Skill[2] = false;
            Owner.SetAnim("Demo_Spell_Wizard_2");
            Owner.MoveDir = Vector2.zero;
            Owner.StartCoroutine(Owner.SkillCoolTimmer(2));
        }

        protected override void Update()
        {
         
            if (Owner.m_Anim.GetCurrentAnimatorStateInfo(0).IsName("Demo_Spell_Wizard_2"))
            {
            }
            else
            {
                Invoke<IdleState>();
            }


        }

        protected override void End()
        {
        }

    }



}
