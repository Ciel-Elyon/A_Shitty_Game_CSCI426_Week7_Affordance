using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PlayerController :MonoBehaviour
{
    public bool IsSit = false;
    public int currentJumpCount = 0; 
    public bool isGrounded = false;
    public bool OnceJumpRayCheck = false;

    public bool Is_DownJump_GroundCheck = false;   // 다운 점프를 하는데 아래 블록인지 그라운드인지 알려주는 불값
    protected float m_MoveX;
    public Rigidbody2D m_rigidbody;
    protected CapsuleCollider2D m_CapsulleCollider;
    protected Animator m_Anim;
    public SpriteRenderer[] m_SpriteRenderer;
    protected GameObject m_Model;

    [Header("[Setting]")]
    public float MoveSpeed = 6;
    public int JumpCount = 2;
    public float jumpForce = 15f;
    public float Damage = 10;


    private void Awake()
    {
      
        m_Model = this.transform.Find("model").gameObject;
        m_SpriteRenderer = m_Model.GetComponentsInChildren<SpriteRenderer>();

    }
    protected void AnimUpdate()
    {


        if (!m_Anim.GetCurrentAnimatorStateInfo(0).IsName("Attack"))
        {
            if (Input.GetKey(KeyCode.Mouse0))
            {


                m_Anim.Play("Attack");
            }
            else
            {

                if (m_MoveX == 0)
                {
                    if (!OnceJumpRayCheck)
                        m_Anim.Play("Idle");

                }
                else
                {

                    m_Anim.Play("Run");
                }

            }



        }

    }




    protected void Filp(bool bLeft)
    {


        transform.localScale = new Vector3(bLeft ? 1 : -1, 1, 1);

    }


    protected void prefromJump()
    {
        m_Anim.Play("Jump");

        m_rigidbody.velocity = new Vector2(0, 0);

        m_rigidbody.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);

        OnceJumpRayCheck = true;
        isGrounded = false;


        currentJumpCount++;

    }

    protected void DownJump()
    {
        if (!isGrounded)
            return;


        if (!Is_DownJump_GroundCheck)
        {
            m_Anim.Play("Jump");

            m_rigidbody.AddForce(-Vector2.up * 10);
            isGrounded = false;

            m_CapsulleCollider.enabled = false;

            StartCoroutine(GroundCapsulleColliderTimmerFuc());

        }


    }

    IEnumerator GroundCapsulleColliderTimmerFuc()
    {
        yield return new WaitForSeconds(0.3f);
        m_CapsulleCollider.enabled = true;
    }


    //////바닥 체크 레이케스트 
    Vector2 RayDir = Vector2.down;


    float PretmpY;
    float GroundCheckUpdateTic = 0;
    float GroundCheckUpdateTime = 0.01f;
    protected void GroundCheckUpdate()
    {
        if (!OnceJumpRayCheck)
            return;

        GroundCheckUpdateTic += Time.deltaTime;



        if (GroundCheckUpdateTic > GroundCheckUpdateTime)
        {
            GroundCheckUpdateTic = 0;



            if (PretmpY == 0)
            {
                PretmpY = transform.position.y;
                return;
            }



            float reY = transform.position.y - PretmpY;  //    -1  - 0 = -1 ,  -2 -   -1 = -3

            if (reY <= 0)
            {

                if (isGrounded)
                {

                    LandingEvent();
                    OnceJumpRayCheck = false;

                }
                else
                {

             

                }


            }


            PretmpY = transform.position.y;

        }




    }

    public bool b_FireDamage = false;
    public void FireDamage(float Damage, float Time)
    {

        if (!b_FireDamage)
            StartCoroutine(StartFireDamage(Damage, Time));

    }
    IEnumerator StartFireDamage(float Damage, float Time)
    {
        float timetic = 0;
        b_FireDamage = true;


        Vector3 tmpcolor = new Vector3(1, 0.30f, 0.10f);

        SetCharacterColor( tmpcolor, 1.0f);
        while (true)
        {
            if (timetic > Time)
                break;


            timetic++;


            Damaged(Damage, new Vector2(0, 0));


            yield return new WaitForSeconds(0.1f);
        }

        SetCharacterColor(new Vector3(1, 1, 1), 1.0f);


        b_FireDamage = false;

    }


    public void SetCharacterColor(Vector3 colorvalue, float alphavlaue)
    {

        for (int i = 0; i < m_SpriteRenderer.Length; i++)
        {
            m_SpriteRenderer[i].color = new Color(colorvalue.x, colorvalue.y, colorvalue.z, alphavlaue);

        }

    }


    protected abstract void LandingEvent();
    public abstract void Damaged(float m_damged,Vector2 dir);

    public abstract void DefaulAttack_Collider(GameObject obj);
    public abstract void Skill_1Attack_Collider(GameObject obj);
    public abstract void Skill_2Attack_Collider(GameObject obj);
    public abstract void Skill_3Attack_Collider(GameObject obj);
    public abstract void Skill_4Attack_Collider(GameObject obj);



    public virtual void DefaultAttack_Anim_1_Enter() { }
    public virtual void DefaultAttack_Anim_1_Exit() { }

    public virtual void SkillAttack_Anim_1_Enter() { }
    public virtual void SkillAttack_Anim_1_Exit() { }

    public virtual void SkillAttack_Anim_2_Enter() { }
    public virtual void SkillAttack_Anim_2_Exit() { }


    public virtual void SkillAttack_Anim_3_Enter() { }
    public virtual void SkillAttack_Anim_3_Exit() { }

    public virtual void SkillAttack_Anim_4_Enter() { }
    public virtual void SkillAttack_Anim_4_Exit() { }

}
