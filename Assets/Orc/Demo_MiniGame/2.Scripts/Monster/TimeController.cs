using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimeController : MonoBehaviour
{

    public Text timeCounter;
    private TimeSpan timePlaying;
    public static bool timerGoing = false;
    public bool gameWinning;

    public GameObject gameoverPanel;
    public GameObject thumbsUp;
    public GameObject skull;

    public Slider slide;

    [Space]
    [SerializeField]
    private float gameTime = 40f;

    private float elapsedTime;
    // Start is called before the first frame update
    void Start()
    {
        Time.timeScale = 1.0f;
        timeCounter.text = "Time: 40.00";
        timerGoing = false;
        gameWinning = false;

        BeginTimer();
    }

    public void BeginTimer()
    {
        timerGoing = true;
        elapsedTime = gameTime;

        StartCoroutine(UpdateTimer());
    }

    public void EndTimer()
    {
        timerGoing = false;
        gameWinning = true;

        Time.timeScale = 0.0f;
        //set game winning screen here
        thumbsUp.SetActive(true);
        skull.SetActive(false);
        gameoverPanel.SetActive(true);
    }

    private IEnumerator UpdateTimer()
    {
        while(timerGoing)
        {
            if(elapsedTime >= 0)
            {
                elapsedTime -= Time.deltaTime;
                slide.value = 1.0f - elapsedTime / gameTime;
                timePlaying = TimeSpan.FromSeconds(elapsedTime);
                string timePlayingStr = "Time: " + timePlaying.ToString("ss'.'ff");
                timeCounter.text = timePlayingStr;
                yield return null;
            }
            else
            {
                EndTimer();
                Debug.Log("Game End");
            }
        }
    }

   
}
