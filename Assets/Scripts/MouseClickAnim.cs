using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseClickAnim : MonoBehaviour
{
    public SpriteRenderer ClickSprite;

    public float minShowLength;

    public float onTime;
    public float offTime;

    bool hasClicked;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(ClickAnim());
        StartCoroutine(HideSelf());
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0)) {
            hasClicked = true;
        }
    }

    IEnumerator ClickAnim() {
        while (true) {
            yield return new WaitForSeconds(offTime);
            ClickSprite.enabled = true;
            yield return new WaitForSeconds(onTime);
            ClickSprite.enabled = false;
        }
    }

    IEnumerator HideSelf() {
        yield return new WaitForSeconds(minShowLength);
        while (!hasClicked) {
            yield return null;
        }
        gameObject.SetActive(false);
        yield break;
    }
}
