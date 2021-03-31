using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bird : MonoBehaviour
{
    //public TimeController gameTimer;
    public Rigidbody2D rb2d;
    public GameObject mouseSymbol;
    public GameObject gameoverPanel;
    public ParticleSystem blood;
    public AudioSource deathSource;
    public bool isDead = false;
    bool alreadyDead = false;
    private Animator anim;

    public GameObject thumbsUp;
    public GameObject skull;
    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (isDead && !alreadyDead) {
            Die();
        }
    }

    void FixedUpdate() {
        if (!isDead) {
            Vector3 pointPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            pointPos.z = 0.0f;

            rb2d.MovePosition(pointPos);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.CompareTag("Monster") || collision.gameObject.CompareTag("Weapon"))
        {
            isDead = true;
        }
    }

    void Die() {
        TimeController.timerGoing = false;
        alreadyDead = true;
        anim.SetTrigger("Die");
        blood.Play();
        isDead = true;
        mouseSymbol.SetActive(false);
        thumbsUp.SetActive(false);
        skull.SetActive(true);
        gameoverPanel.SetActive(true);
        deathSource.Play();
    }

}
