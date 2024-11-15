using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Unity.VisualScripting;

public class Survivor : Agent
{
    //----------------------------------------------------------------------------------------------------------------------------------------
    [Header("Environment")]
    [SerializeField] private MLEnvironment MLEnvironment;

    [Header("Survivor Stats")]
    [SerializeField] public float movementSpeed = 10f;

    [Header("Collision Values")]
    [SerializeField] private bool AlreadyHit;

    [Header("Same Location")]
    [SerializeField] public float SameLocationRadius;
    [SerializeField] public Vector3 lastRecordedSameLocation;
    [SerializeField] public float RecordSameLocationTime;
    [SerializeField] public float RecordSameLocationTimer;

    [Header("Reward And Punishment")]
    [SerializeField] public float CollectPelletReward;
    [SerializeField] public float CollectedAllPelletReward;

    [SerializeField] public float stayedInSameLocationPunishment;
    [SerializeField] public float HitWallPunishment;
    [SerializeField] public float HitObstaclePunishment;
    [SerializeField] public float HitZombiePunishment;
    [SerializeField] public float RanOutOfTimePunishment;
    //----------------------------------------------------------------------------------------------------------------------------------------



    //----------------------------------------------------------------------------------------------------------------------------------------
    //ML Agent Functions
    public override void Initialize()
    {

    }

    public override void OnEpisodeBegin()
    {
        transform.localPosition = MLEnvironment.GetRandomEnvironmentPosition(MLEnvironment.SurvivorSpawnZone);
        MLEnvironment.SetUpMLEnvironmentOnEpisodeBegin();

        AlreadyHit = false;

        lastRecordedSameLocation = transform.localPosition;
        RecordSameLocationTimer = RecordSameLocationTime;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(this.transform.localPosition);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        var discreteAction = actions.DiscreteActions;
        Vector3 changeVector = new Vector3(discreteAction[0] - 1, discreteAction[1] - 1, 0).normalized * movementSpeed * Time.fixedDeltaTime;
        transform.localPosition += changeVector;

    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteAction = actionsOut.DiscreteActions;

        //Move left or right
        discreteAction[0] = 1;
        if (Input.GetKey(KeyCode.D)) discreteAction[0] += 1;
        if (Input.GetKey(KeyCode.A)) discreteAction[0] -= 1;

        //Move up or down
        discreteAction[1] = 1;
        if (Input.GetKey(KeyCode.W)) discreteAction[1] += 1;
        if (Input.GetKey(KeyCode.S)) discreteAction[1] -= 1;
    }
    //----------------------------------------------------------------------------------------------------------------------------------------



    //----------------------------------------------------------------------------------------------------------------------------------------
    //Rewards and Punishments
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (AlreadyHit) return;
        if (collision.gameObject.tag == "Wall")
        {
            OverloadedEndEpisode(Color.red, HitWallPunishment, "Hit Wall");
        }
        else if (collision.gameObject.tag == "Obstacle")
        {
            //OverloadedEndEpisode(Color.red, HitObstaclePunishment, "Hit Obstacle");
        }
        else if (collision.gameObject.tag == "Zombie")
        {
            OverloadedEndEpisode(Color.red, HitZombiePunishment, "Hit Zombie", MLEnvironment.Gun.FailedPunishment);
            AlreadyHit = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Pellet")
        {
            AddReward(CollectPelletReward);
            MLEnvironment.DestroyAndRemovePellet(collision.gameObject);

            if (MLEnvironment.SpawnedPelletList.Count == 0)
                OverloadedEndEpisode(Color.green, CollectedAllPelletReward + MLEnvironment.RemainingTime, "Got All Pellets");
        }
    }

    private void RanOutOfTime()
    {
        if (MLEnvironment.RemainingTime <= 0) OverloadedEndEpisode(Color.red, RanOutOfTimePunishment, "Ran out of time", MLEnvironment.Gun.FailedPunishment);
    }

    public void OverloadedEndEpisode(Color resultColor, float reward, string message = "", float gunReward = 0)
    {
        MLEnvironment.BackgroundSR.material.color = resultColor;

        AddReward(reward);

        Debug.Log(MLEnvironment.gameObject.name + " " + "Reward: " + GetCumulativeReward() + " " + message);

        EndEpisode();

        //MLEnvironment.Gun.AddReward(gunReward);
        //Debug.Log(MLEnvironment.gameObject.name + " " + "Gun Reward: " + MLEnvironment.Gun.GetCumulativeReward() + " " + message);
        //MLEnvironment.Gun.EndEpisode();
    }
    //----------------------------------------------------------------------------------------------------------------------------------------

    private void SameLocationTimerLogic()
    {
        RecordSameLocationTimer -= Time.fixedDeltaTime;
        if (RecordSameLocationTimer <= 0)
        {
            if (Vector3.Distance(transform.localPosition, lastRecordedSameLocation) <= SameLocationRadius)
            {
                //OverloadedEndEpisode(Color.red, stayedInSameLocationPunishment, "Stuck In One Position");
            }
            lastRecordedSameLocation = transform.localPosition;
            RecordSameLocationTimer = RecordSameLocationTime;
        }
    }

    public void FixedUpdate()
    {
        RanOutOfTime();
        AlreadyHit = false;
        SameLocationTimerLogic();

        MLEnvironment.SurvivorCumulativeReward = GetCumulativeReward();
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.R)) OverloadedEndEpisode(Color.red, 0, "Manually Ended Epsiode");
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.black;
        Gizmos.DrawWireSphere(MLEnvironment.gameObject.transform.TransformPoint(lastRecordedSameLocation), SameLocationRadius);
    }
}
