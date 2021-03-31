using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireballScript : MonoBehaviour
{

    public float movingSpeed = 4f;
    public float SmoothDamp = 1f;
    public float destroyTime = 3f;

    public float Damage = 0;
    public Vector3 TargetDir;

    public GameObject TartgetObj;

    private Rigidbody2D m_rigidbody;

    bool m_bUpdateCheck = true;
    void Start()
    {
        m_rigidbody = this.transform.GetComponent<Rigidbody2D>();
        StartCoroutine(destroyObj());
      //  Debug.Log("start!!");
    }

    public void  Fire(GameObject Tartget, float damage)
    {
        Player_Fireball(Tartget, damage);

    }

  
    public void Player_Fireball(GameObject Tartget,float damage)
    {

        TartgetObj = Tartget;
        if (TartgetObj == null)
            return;

        Damage = damage;
        TargetDir = TartgetObj.transform.position - this.transform.position;
        this.transform.right = TargetDir;
      
     
    }



    IEnumerator destroyObj()
    {
        yield return new WaitForSeconds(destroyTime);
        Destroy(this.gameObject);
      //  Debug.Log("파괴!!" + this.gameObject);
    }

    private float UpdateTic = 0;

    void Update()
    {

        //타겟 동기화는 0.5초에 한번씩 해준다. 

        smoothMove();
    }

    private Vector2 Velocity = Vector2.zero;
    void smoothMove()
    {
        if (TartgetObj == null)
            return;
        //회전
        TargetDir = TartgetObj.transform.position - this.transform.position;
        this.transform.right = Vector2.SmoothDamp(this.transform.right, TargetDir, ref Velocity, SmoothDamp);
        this.transform.Translate(Vector2.right * Time.deltaTime * movingSpeed);

    }
    public GameObject Fire_explosionPrefab;
    Collider2D[] colliderpoint = new Collider2D[1]; 
    void OnTriggerEnter2D(Collider2D other)
    {
  

            if (other.CompareTag("Player"))
            {


            Bird tmp_Player = other.GetComponent<Bird>();

            if(tmp_Player != null)
            {
                Vector2 dir = new Vector2(0, 0);
                tmp_Player.isDead = true; ;
            }
           else
            {
                Tree tree = other.GetComponent<Tree>();
                tree.hitPoints--;
            }
                    Instantiate(Fire_explosionPrefab, other.transform.position, Quaternion.identity);

                    Destroy(this.gameObject);
                  //    Debug.Log(this.gameObject);

            }
 
    }


}
