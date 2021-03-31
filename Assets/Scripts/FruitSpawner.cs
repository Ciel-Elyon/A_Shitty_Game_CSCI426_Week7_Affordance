using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FruitSpawner : MonoBehaviour
{
    public Bird birdControls;
    public GameObject fruitPrefab;
    public Vector2 minPos;
    public Vector2 maxPos;

    public float spawnFrequency;
    float spawnTimer;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!birdControls.isDead) {
            spawnTimer += Time.deltaTime;
            if (spawnTimer >= spawnFrequency) {
                Spawn();
                spawnTimer = 0.0f;
            }
        }
    }

    void Spawn() {
        Vector3 spawnPos = new Vector3(Random.Range(minPos.x, maxPos.x), Random.Range(minPos.y, maxPos.y), 0.0f);
        Instantiate(fruitPrefab, spawnPos, Quaternion.identity);
    }
}
