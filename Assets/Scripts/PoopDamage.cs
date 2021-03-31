using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoopDamage : MonoBehaviour
{
    private CircleCollider2D poopExplodeCollider;
    // Start is called before the first frame update
    void Start()
    {
        poopExplodeCollider = GetComponent<CircleCollider2D>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Monster"))
        {
            //    Debug.Log("Attack22"+ obj);
            Vector2 Knockdir = other.transform.position - this.transform.position;
            other.transform.root.GetComponent<Mon_Bass>().Damaged(100, Knockdir.normalized * 1.5f, 0.2f);
        }
    }


}
