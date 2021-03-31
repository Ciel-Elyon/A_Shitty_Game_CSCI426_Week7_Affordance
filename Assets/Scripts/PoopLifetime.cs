using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoopLifetime : MonoBehaviour
{
    public float lifetime = 6.0f;
    float timer;
    public GameObject poopExplosion;
    public AudioSource explodeSource;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        if (timer > lifetime) {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Ground"))
        {
            poopExplosion.SetActive(true);
            explodeSource.Play();
            GetComponent<SpriteRenderer>().enabled = false;
        }
    }
}
