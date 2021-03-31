using System.Collections;
using System.Collections.Generic;
//using UnityEditorInternal;
using UnityEngine;
using UnityEngine.SocialPlatforms;

public class Tree : MonoBehaviour
{
    public GameObject gameoverPanel;
    public ParticleSystem woodParticles;
    public bool isDead = false;
    bool alreadyDead = false;

    public Bird bird;
    public Animator explosion;
    public AudioSource explodeSource;

    public float warnLength;
    public SpriteRenderer warningRenderer;
    Coroutine warningRoutine;

    [SerializeField]
    private Animator anim;

    [SerializeField]
    [Range(1, 20)]
    public int hitPoints = 10;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(Flash(0.3f, 0.3f));
    }

    // Update is called once per frame
    void Update()
    {
        if (isDead && !alreadyDead)
        {
            Die();
        }
    }


    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Monster") || collision.gameObject.CompareTag("Weapon")) {
            if (hitPoints <= 0)
                isDead = true;
            else {
                hitPoints--;
                Warning();
            }
        }
    }

    void Warning() {
        if (warningRoutine != null) {
            StopCoroutine(warningRoutine);
        }
        warningRoutine = StartCoroutine(Warn(warnLength));
    }

    IEnumerator Warn(float length) {
        warningRenderer.enabled = true;
        yield return new WaitForSeconds(length);
        warningRenderer.enabled = false;
    }

    IEnumerator Flash(float redTime, float yellowTime) {
        while (true) {
            yield return new WaitForSeconds(redTime);
            warningRenderer.color = Color.red;
            yield return new WaitForSeconds(yellowTime);
            warningRenderer.color = Color.yellow;
        }
    }

    void Die()
    {
        alreadyDead = true;
        anim.SetTrigger("Fall");
        woodParticles.Play();
        bird.isDead = true;
        explosion.Play("Explosion");
        explodeSource.Play();
        /*
        isDead = true;
        gameoverPanel.SetActive(true);
        */
        // Play sound, start particle effect
    }

}
