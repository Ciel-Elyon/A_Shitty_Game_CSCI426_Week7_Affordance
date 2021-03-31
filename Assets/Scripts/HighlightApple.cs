using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HighlightApple : MonoBehaviour
{
    public SpriteRenderer arrowSprite;

    public float redTime;
    public float yellowTime;

    bool highlighted;
    Coroutine highlightRoutine;

    // Start is called before the first frame update
    void Start()
    {
        highlighted = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (PoopControls.empty && !highlighted) {
            arrowSprite.enabled = true;
            highlightRoutine = StartCoroutine(HighlightAnim());
            highlighted = true;
        }
        if (!PoopControls.empty && highlighted) {
            arrowSprite.enabled = false;
            StopCoroutine(highlightRoutine);
            highlighted = false;
        }
    }

    IEnumerator HighlightAnim() {
        while (true) {
            yield return new WaitForSeconds(redTime);
            arrowSprite.color = Color.red;
            yield return new WaitForSeconds(yellowTime);
            arrowSprite.color = Color.yellow;
        }
    }


}
