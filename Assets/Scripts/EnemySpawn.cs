using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class EnemySpawn : MonoBehaviour
{
    public static EnemySpawn es;

    public GameObject smokePrefab;

    public Bird bird;

    private float delayTimer = 0.0f;

    public List<GameObject> MonsterList = new List<GameObject>();
    [SerializeField]
    private GameObject[] enemyPrefabs;
    [SerializeField]
    private GameObject[] sniperPrefabs;

    [SerializeField]
    private Transform[] spawnPoints;

    [SerializeField]
    private float minDelay = .8f;

    [SerializeField]
    private float maxDelay = 1f;
    // Start is called before the first frame update

    private void Awake()
    {
        es = this;


        Physics2D.IgnoreLayerCollision(0, 1);
    }
    void Start()
    {
        minDelay = .8f;
        delayTimer = 0.0f;
        StartCoroutine(SpawnEnemies());
    }

    // Update is called once per frame
    void Update()
    {
        delayTimer += Time.deltaTime;

        if(delayTimer > 6)
        {
            StartCoroutine(DecreaseMinDelay());
        }
    }

    IEnumerator SpawnEnemies()
    {
        while(!bird.isDead)
        {

            float delay = Random.Range(minDelay, maxDelay);

            yield return new WaitForSeconds(delay);

            if(EnemySpawn.es.MonsterList.Count <= 20)
            {
                int spawnindex = Random.Range(0, spawnPoints.Length);
                int spawnType = Random.Range(0, enemyPrefabs.Length);
                int ssType = Random.Range(0, sniperPrefabs.Length);

                Transform spawnPoint = spawnPoints[spawnindex];
                GameObject enemySpawned;
                if (spawnindex == 0)
                    enemySpawned = Instantiate(enemyPrefabs[spawnType], spawnPoint.position, Quaternion.identity);
                else
                    enemySpawned = Instantiate(sniperPrefabs[ssType], spawnPoint.position, Quaternion.identity);

                Instantiate(smokePrefab, spawnPoint.position, Quaternion.identity);
                MonsterList.Add(enemySpawned);
            }

            
            


        }
    }

    IEnumerator DecreaseMinDelay()
    {
        while(minDelay > 0.2f)
        {
            minDelay -= 0.1f;
            yield return new WaitForSeconds(.8f);
           
        }
    }
}
