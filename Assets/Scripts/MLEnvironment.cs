using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MLEnvironment : MonoBehaviour
{
    //----------------------------------------------------------------------------------------------------------------------------------------
    [Header("Environment Information")]
    [SerializeField] public SpriteRenderer BackgroundSR;

    [Header("Training Information")]
    [SerializeField] public float EpisodeTime = 30.0f;
    [SerializeField] public float RemainingTime = 0;

    [Header("Spawn Zones")]
    [SerializeField] public Transform SurvivorSpawnZone;
    [SerializeField] public Transform ObstacleSpawnZone;
    [SerializeField] public Transform ZombieSpawnZone;
    [SerializeField] public Transform PelletSpawnZone;

    [Header("Spawn Information")]
    [SerializeField] public float SurvivorAvoidDistance;
    [SerializeField] public float ObstacleAvoidDistance;
    [SerializeField] public float ZombieAvoidDistance;
    [SerializeField] public float PelletAvoidDistance;
    [SerializeField] public float SpawnDistanceBuffer = 1.0f;

    [Header("Survivor Information")]
    [SerializeField] public Survivor Survivor;
    [SerializeField] public float SurvivorCumulativeReward;

    [Header("Gun Information")]
    [SerializeField] public Gun Gun;
    [SerializeField] public float GunCumulativeReward;

    [Header("Obstacle Information")]
    [SerializeField] private GameObject ObstaclePrefab;
    [SerializeField] private Transform ObstacleAnchor;
    [SerializeField] public List<GameObject> SpawnedObstacleList;
    [SerializeField] private Vector2 ObstacleNumRange;

    [Header("Zombie Information")]
    [SerializeField] private GameObject ZombiePrefab;
    [SerializeField] private Transform ZombieAnchor;
    [SerializeField] public List<GameObject> SpawnedZombieList;
    [SerializeField] private Vector2 ZombieNumRange;

    [Header("Pellet Information")]
    [SerializeField] private GameObject PelletPrefab;
    [SerializeField] private Transform PelletAnchor;
    [SerializeField] public List<GameObject> SpawnedPelletList;
    [SerializeField] private int InitialPelletNum;

    [Header("Bullet Information")]
    [SerializeField] private GameObject BulletPrefab;
    [SerializeField] private Transform BulletAnchor;
    [SerializeField] public List<GameObject> SpawnedBulletList;
    //----------------------------------------------------------------------------------------------------------------------------------------

    public void Awake()
    {
        SetAvoidDistances();
    }


    //----------------------------------------------------------------------------------------------------------------------------------------
    //Spawn Position Helper Functions
    public void SetAvoidDistances()
    {
        SurvivorAvoidDistance = Survivor.gameObject.GetComponent<CircleCollider2D>().bounds.extents.x;

        GameObject Obstacle = Instantiate(ObstaclePrefab);
        ObstacleAvoidDistance = Obstacle.gameObject.GetComponent<BoxCollider2D>().bounds.size.x;
        Destroy(Obstacle);

        GameObject Zombie = Instantiate(ZombiePrefab);
        ZombieAvoidDistance = Zombie.gameObject.GetComponent<CircleCollider2D>().bounds.extents.x;
        Destroy(Zombie);

        GameObject pellet = Instantiate(PelletPrefab);  
        PelletAvoidDistance = pellet.gameObject.GetComponent<CircleCollider2D>().bounds.extents.x;
        Destroy(pellet);
    }

    public Vector3 GetRandomEnvironmentPosition(Transform spawnZone)
    {
        Vector3 bounds = spawnZone.localScale;
        float width = Mathf.Abs(bounds.x) / 2;
        float height = Mathf.Abs(bounds.y) / 2;
        return new Vector3(Random.Range(-width, width), Random.Range(-height, height), 0);
    }

    public Vector3 GetNonOverlappingPositionWithTarget(Transform spawnZone, Vector3 target, float minDistance)
    {
        Vector3 spawnPosition = GetRandomEnvironmentPosition(spawnZone);

        int maxIteration = 10;
        int counter = 0;
        while (Vector3.Distance(spawnPosition, target) <= minDistance && counter < maxIteration)
        {
            spawnPosition = GetRandomEnvironmentPosition(spawnZone);
            counter++;
        }

        return spawnPosition;
    }

    public bool OverlappingWithGameObjectList(List<GameObject> list, Vector3 checkPosition, float minDistance)
    {
        foreach (var i in list)
        {
            if (Vector3.Distance(checkPosition, i.transform.localPosition) <= minDistance) 
                return true;
        }
        return false;
    }

    public Vector3 GetNonOverlappingPositionWithGameObjectList(List<GameObject> list, Transform spawnZone, float minDistance)
    {
        Vector3 spawnPosition = GetRandomEnvironmentPosition(spawnZone);

        int maxIteration = 10;
        int counter = 0;
        while (OverlappingWithGameObjectList(list, spawnPosition, minDistance) && counter < maxIteration)
        {
            spawnPosition = GetRandomEnvironmentPosition(spawnZone);
            counter++;
        }

        return spawnPosition;
    }
    //----------------------------------------------------------------------------------------------------------------------------------------



    //----------------------------------------------------------------------------------------------------------------------------------------
    //Spawn Obstacle Functions
    public void SpawnObstacle()
    {
        GameObject Obstacle = Instantiate(ObstaclePrefab);
        Obstacle.transform.parent = ObstacleAnchor;
        Obstacle.transform.localPosition = GetNonOverlappingPositionWithTarget(ObstacleSpawnZone, Survivor.gameObject.transform.localPosition, SurvivorAvoidDistance + ObstacleAvoidDistance + SpawnDistanceBuffer);
        SpawnedObstacleList.Add(Obstacle);
    }

    public void SetUpSpawnedObstacleListOnEpisodeBegin()
    {
        Utils.DestroyAndRemoveAllFromList(SpawnedObstacleList);
        int spawnNum = Random.Range((int)ObstacleNumRange.x, (int)ObstacleNumRange.y + 1);
        for (int i = 0; i < spawnNum; i++) SpawnObstacle();
    }
    //----------------------------------------------------------------------------------------------------------------------------------------



    //----------------------------------------------------------------------------------------------------------------------------------------
    //Spawn Zombie Functions
    public void DestroyAndRemoveZombie(GameObject Zombie)
    {
        Utils.DetroyAndRemoveFromList(SpawnedZombieList, Zombie);
    }


    public void SpawnZombie()
    {
        GameObject Zombie = Instantiate(ZombiePrefab);
        Zombie.transform.parent = ZombieAnchor;
        Zombie.transform.localPosition = GetNonOverlappingPositionWithTarget(ZombieSpawnZone, Survivor.gameObject.transform.localPosition, SurvivorAvoidDistance + ZombieAvoidDistance + SpawnDistanceBuffer);
        Zombie.GetComponent<Zombie>().SetUp(this);
        SpawnedZombieList.Add(Zombie);
    }

    public void SetUpSpawnedZombieListOnEpisodeBegin()
    {
        Utils.DestroyAndRemoveAllFromList(SpawnedZombieList);
        int spawnNum = Random.Range((int)ZombieNumRange.x, (int)ZombieNumRange.y + 1);
        for (int i = 0; i < spawnNum; i++) SpawnZombie();
    }
    //----------------------------------------------------------------------------------------------------------------------------------------



    //----------------------------------------------------------------------------------------------------------------------------------------
    //Spawn Pellet Functions
    public void DestroyAndRemovePellet(GameObject Pellet)
    {
        Utils.DetroyAndRemoveFromList(SpawnedPelletList, Pellet);
    }

    public void SpawnPellet()
    {
        GameObject Pellet = Instantiate(PelletPrefab);
        Pellet.transform.parent = PelletAnchor;
        Pellet.transform.localPosition = GetNonOverlappingPositionWithGameObjectList(SpawnedObstacleList, PelletSpawnZone, PelletAvoidDistance + ObstacleAvoidDistance + SpawnDistanceBuffer);
        SpawnedPelletList.Add(Pellet);
    }

    public void SetUpSpawnedPelletListOnEpisodeBegin()
    {
        Utils.DestroyAndRemoveAllFromList(SpawnedPelletList);
        for (int i = 0; i < InitialPelletNum; i++) SpawnPellet();
    }
    //----------------------------------------------------------------------------------------------------------------------------------------



    //----------------------------------------------------------------------------------------------------------------------------------------
    //Spawn Bullet Functions
    public void DestroyAndRemoveBullet(GameObject Bullet)
    {
        Utils.DetroyAndRemoveFromList(SpawnedBulletList, Bullet);
    }

    public void SpawnBullet(Vector3 GlobalSpawnPosition, Vector3 GlobalEulerAngles)
    {
        GameObject Bullet = Instantiate(BulletPrefab);
        Bullet.transform.parent = BulletAnchor;
        Bullet.transform.position = GlobalSpawnPosition;
        Bullet.transform.eulerAngles = GlobalEulerAngles;
        Bullet.GetComponent<Bullet>().SetUp(this);
        SpawnedBulletList.Add(Bullet);
    }

    public void SetUpSpawnedBulletListOnEpisodeBegin()
    {
        Utils.DestroyAndRemoveAllFromList(SpawnedBulletList);
        for (int i = 0; i < InitialPelletNum; i++) SpawnPellet();
    }
    //----------------------------------------------------------------------------------------------------------------------------------------

    public void SetUpMLEnvironmentOnEpisodeBegin()
    {
        SetUpSpawnedObstacleListOnEpisodeBegin();
        SetUpSpawnedZombieListOnEpisodeBegin();
        SetUpSpawnedPelletListOnEpisodeBegin();
        RemainingTime = EpisodeTime;
    }

    public void FixedUpdate()
    {
        RemainingTime -= Time.deltaTime;
    }
}
