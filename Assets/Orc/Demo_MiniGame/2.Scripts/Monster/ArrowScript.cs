using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowScript : MonoBehaviour
{

    public float movingSpeed = 4f;

    public float destroyTime = 3f;


    private Vector2 selfPos;
    private Quaternion selfRo;

    public float Damage = 0;

    private Rigidbody2D m_rigidbody;

    bool m_bUpdateCheck = true;
    void Awake()
    {
        m_rigidbody = GetComponent<Rigidbody2D>();

    }

    public void  Fire(Vector3 targetPos,float angle ,float damage)
    {
 
      
        if (angle>50)
            SetFire( targetPos, damage);
      else
            SetFire_2( targetPos, damage);
    }

   
    private void SetFire(Vector3 targetPos ,float damage)
    {
        Damage = damage;
        Vector3 velocity = GetVelocity(transform.position, targetPos, 45f);
        SetVelocity(velocity * 1.0f);  //중력값 3기준. 1.8f 
    }

  
    private void SetFire_2(Vector3 targetPos, float damage)
    {
        Damage = damage;
        // Debug.Log("50미만");
        Vector3 tmpPos = new Vector2(transform.position.x, transform.position.y - 2);
        Vector2 dir = targetPos - tmpPos;
        m_rigidbody.AddForce(dir * 120);
    }

    void SetVelocity(Vector3 velocity)
    {
       // Debug.Log("ddddddddd:" + velocity);
        GetComponent<Rigidbody2D>().velocity = velocity;
    }

    IEnumerator destroyObj()
    {
      

        yield return new WaitForSeconds(destroyTime);

        Destroy(gameObject);

    }


    Vector3 v1;
    void Update()
    {
        if (m_bUpdateCheck == false) return;

        this.transform.right = m_rigidbody.velocity;

        //Debug.Log("m_rigidbody.velocity:" + m_rigidbody.velocity);
    }



    void smoothMove()
    {

        transform.position = Vector3.Lerp(transform.position, selfPos, Time.deltaTime * 8);
        transform.rotation = Quaternion.Lerp(transform.rotation, selfRo, Time.deltaTime * 8);


    }

    Vector3 GetVelocity(Vector3 currentPos, Vector3 targetPos, float initialAngle)
    {
        float gravity = Physics.gravity.magnitude;
        float angle = initialAngle * Mathf.Deg2Rad;

        Vector3 planarTarget = new Vector3(targetPos.x, 0, targetPos.z);
        Vector3 planarPosition = new Vector3(currentPos.x, 0, currentPos.z);

        float distance = Vector3.Distance(planarTarget, planarPosition);
        float yOffset = currentPos.y - targetPos.y;

        float initialVelocity = (1 / Mathf.Cos(angle)) * Mathf.Sqrt((0.5f * gravity * Mathf.Pow(distance, 2)) / (distance * Mathf.Tan(angle) + yOffset));

        Vector3 velocity = new Vector3(0f, initialVelocity * Mathf.Sin(angle), initialVelocity * Mathf.Cos(angle)); ;

        float angleBetweenObjects = Vector3.Angle(Vector3.forward, planarTarget - planarPosition) * (targetPos.x > currentPos.x ? 1 : -1);
        Vector3 finalVelocity = Quaternion.AngleAxis(angleBetweenObjects, Vector3.up) * velocity;

        return finalVelocity;
    }



    Collider2D[] colliderpoint = new Collider2D[1]; 
    void OnTriggerEnter2D(Collider2D other)
    {

        if (m_bUpdateCheck == false) return;

    
  


            if (other.CompareTag("Player"))
            {

                m_bUpdateCheck = false;
                m_rigidbody.isKinematic = true;
                m_rigidbody.velocity = new Vector2(0, 0);
                this.transform.SetParent(other.transform);
               
     
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




            StartCoroutine("destroyObj");


            }
            else if (other.CompareTag("Ground"))
            {
                m_bUpdateCheck = false;
                m_rigidbody.isKinematic = true;
                m_rigidbody.velocity = new Vector2(0, 0);
                this.transform.SetParent(other.transform.parent);  // 적의 touchSensor가 사라지기때문에 그 위에 생기도록한다. 
              
                    StartCoroutine("destroyObj");

            }

    }






}
