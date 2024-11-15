using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class Gun : Agent
{
    //----------------------------------------------------------------------------------------------------------------------------------------
    [Header("Environment")]
    [SerializeField] private MLEnvironment MLEnvironment;

    [Header("Gun Variables")]
    [SerializeField] public float angleRotation;

    [Header("Gun Stats")]
    [SerializeField] public float rotationSpeed = 360f;

    [SerializeField] public Transform BulletSpawnPoint;
    [SerializeField] public float ShootCoolDown = 2.0f;
    [SerializeField] public float ShootCoolDownTimer;

    [Header("Reward And Punishment")]
    [SerializeField] public float HitAllZombieReward;
    [SerializeField] public float FailedPunishment;

    [SerializeField] public float HitZombieReward;
    [SerializeField] public float MissedZombiePunishment;
    //----------------------------------------------------------------------------------------------------------------------------------------



    //----------------------------------------------------------------------------------------------------------------------------------------
    //ML Agent Functions
    public override void Initialize()
    {

    }

    public override void OnEpisodeBegin()
    {
        transform.localEulerAngles = Vector3.zero;
        ShootCoolDownTimer = 0;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(this.transform.localEulerAngles.y);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        var discreteActions = actions.DiscreteActions;

        angleRotation += (discreteActions[0] - 1) * rotationSpeed * Time.fixedDeltaTime;
        transform.localEulerAngles = new Vector3(0, 0, angleRotation);

        if (discreteActions[1] == 1)
            if (ShootCoolDownTimer <= 0)
            {
                MLEnvironment.SpawnBullet(BulletSpawnPoint.position, transform.eulerAngles);
                ShootCoolDownTimer = ShootCoolDown;
            }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActions = actionsOut.DiscreteActions;

        //Rotating Gun
        discreteActions[0] = 1;
        if (Input.GetKey(KeyCode.Q)) discreteActions[0] += 1;
        if (Input.GetKey(KeyCode.E)) discreteActions[0] -= 1;

        //Shooting Gun
        discreteActions[1] = 0;
        if (Input.GetKey(KeyCode.Space)) discreteActions[1] = 1;   

    }
    //----------------------------------------------------------------------------------------------------------------------------------------

    private void FixedUpdate()
    {
        ShootCoolDownTimer -= Time.fixedDeltaTime;
        if (ShootCoolDownTimer < 0) ShootCoolDownTimer = 0;

        MLEnvironment.GunCumulativeReward = GetCumulativeReward();
    }

    private void LateUpdate()
    {
        transform.localPosition = MLEnvironment.Survivor.transform.localPosition;
    }
}
