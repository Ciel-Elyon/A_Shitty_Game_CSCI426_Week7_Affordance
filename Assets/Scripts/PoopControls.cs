using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;



public class PoopControls : MonoBehaviour
{
    public Bird birdControls;

    public GameObject poopPrefab;

    // Credit eat sound: "Chewing, Carrot, A.wav" by InspectorJ (www.jshaw.co.uk) of Freesound.org
    public AudioSource eatSource;

    public AnimationCurve poopAnim;
    public float poopAnimLength;

    public AudioSource poopSource;

    public Image meterImage;
    public Image meterBackground;

    int poopMeter;

    public float meterHighlightLength = 0.2f;
    public float meterTopRatio = 0.778f;

    public int maxPoop = 10;
    public int poopPerFruit = 3;

    public static bool empty = false;

    Vector3 originalScale;

    Coroutine highlightRoutine;
    Coroutine poopRoutine;
    Coroutine emptyRoutine;
    
    Color whitebg = new Color(1.0f, 1.0f, 1.0f, 0.4f);
    Color greenbg = new Color(0.0f, 1.0f, 0.0f, 0.2f);
    Color redbg = new Color(1.0f, 0.0f, 0.0f, 0.2f);
    Color yellowbg = new Color(1.0f, 1.0f, 0.0f, 0.2f);

    // Start is called before the first frame update
    void Start()
    {
        originalScale = transform.localScale;
        poopMeter = maxPoop;
        meterBackground.color = whitebg;
        UpdateMeter();
    }

    // Update is called once per frame
    void Update()
    {
        if (!birdControls.isDead) {
            if (Input.GetMouseButtonDown(0) && TimeController.timerGoing) {
                if (poopMeter > 0) {
                    Poop();
                    if (poopMeter == 0) {
                        if (emptyRoutine != null) {
                            StopCoroutine(emptyRoutine);
                        }
                        emptyRoutine = StartCoroutine(MeterEmpty(0.3f, 0.3f));
                    }
                }
                
            }
            if (poopMeter == 0) {
                empty = true;
            }
            else {
                empty = false;
            }

            UpdateMeter();
        }
    }

    void Poop() {
        poopMeter--;
        HighlightMeter(false);
        Instantiate(poopPrefab, transform.position, Quaternion.identity);
        // Play audio/visual effects
        if (poopRoutine != null) {
            StopCoroutine(poopRoutine);
        }
        poopRoutine = StartCoroutine(PoopAnim());
        poopSource.Play();
    }

    IEnumerator PoopAnim() {
        float timer = 0.0f;
        while (timer < poopAnimLength) {
            timer += Time.deltaTime;
            if (timer > poopAnimLength) {
                timer = poopAnimLength;
            }
            transform.localScale = new Vector3(originalScale.x * poopAnim.Evaluate(timer), originalScale.y * poopAnim.Evaluate(timer), originalScale.z);
            yield return null;
        }
        transform.localScale = originalScale;
        yield break;
    }


    void Eat(GameObject fruit) {
        if (emptyRoutine != null) {
            StopCoroutine(emptyRoutine);
        }
        poopMeter += poopPerFruit;
        HighlightMeter(true);
        if (poopMeter > maxPoop) {
            poopMeter = maxPoop;
        }
        eatSource.Play();
        Destroy(fruit);
        // Play audio/visual effects
    }

    /*
    void Die() {
        // Death animation, Stop game, gameover
    }*/

    void UpdateMeter() {
        float target = meterTopRatio * poopMeter / maxPoop;
        if (meterImage.fillAmount < target) {
            meterImage.fillAmount += Mathf.Max(0.005f, (target - meterImage.fillAmount) / 20.0f);
            if (meterImage.fillAmount > target) {
                meterImage.fillAmount = target;
            }
        }
        else if (meterImage.fillAmount > target) {
            meterImage.fillAmount -= Mathf.Max(0.005f, (meterImage.fillAmount - target) / 20.0f);
            if (meterImage.fillAmount < target) {
                meterImage.fillAmount = target;
            }
        }

        //meterImage.fillAmount = meterTopRatio * poopMeter / maxPoop;
    }

    
    void HighlightMeter(bool increase) {
        if (highlightRoutine != null) {
            StopCoroutine(highlightRoutine);
        }
        highlightRoutine = StartCoroutine(HighlightCoroutine(increase));
    }

    IEnumerator MeterEmpty(float offTime, float onTime) {
        while (true) {
            yield return new WaitForSeconds(offTime);
            meterBackground.color = redbg;
            yield return new WaitForSeconds(onTime);
            meterBackground.color = yellowbg;
        }
    }

    IEnumerator HighlightCoroutine(bool increase) {
        if (increase) {
            meterBackground.color = greenbg;
        }
        else {
            meterBackground.color = redbg;
        }
        yield return new WaitForSeconds(meterHighlightLength);
        meterBackground.color = whitebg;
        yield break;
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.gameObject.CompareTag("Fruit")) {
            Eat(collision.gameObject);
        }
        /*
        if (collision.gameObject.CompareTag("Weapon")) {
            Die();
        }*/
    }
}
