using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletScript : MonoBehaviour
{

    public float movingSpeed = 500;

    public float destroyTime = 3f;


    public float Damage = 0;
    private Vector2 selfPos;
    private Quaternion selfRo;

   // public Vector3 TargetPos;

    private Rigidbody2D m_rigidbody;

    bool m_bUpdateCheck = true;
    void Awake()
    {
        m_rigidbody = GetComponent<Rigidbody2D>();


        //if (!photonView.isMine)
        //    return;
        //StartCoroutine("destroyObj");
    }

    public void  Fire(Vector3 targetPos, float damage)
    {
        SetFire(targetPos, damage);
    }

 
    private void SetFire(Vector3 targetPos, float damage)
    {
        Damage = damage;
        Vector2 tmpPos = targetPos - transform.position;
        m_rigidbody.AddForce(tmpPos.normalized * movingSpeed);
    }



    IEnumerator destroyObj()
    {
        yield return new WaitForSeconds(destroyTime);

        Debug.Log("없어짐!!!총알!!");
        Destroy(gameObject);

    }

    //Vector3 v1;
    void Update()
    {
        if (m_bUpdateCheck == false) return;

        this.transform.right = m_rigidbody.velocity;

    }


    Collider2D[] colliderpoint = new Collider2D[1]; 
    void OnTriggerEnter2D(Collider2D other)
    {


        if (m_bUpdateCheck == false) return;

        if (other.CompareTag("Player"))
        {
            Bird tmp_Player = other.GetComponent<Bird>();

            if(tmp_Player != null)
            {
                Vector2 dir = new Vector2(0, 0);
                tmp_Player.isDead = true;
            }
            else
            {
                Tree tree = other.GetComponent<Tree>();
                tree.hitPoints--;
            }
            

       
            Destroy(gameObject);
        }
        else if (other.CompareTag("Ground"))
        {

            Destroy(gameObject);

        }

    }






 
 
}
