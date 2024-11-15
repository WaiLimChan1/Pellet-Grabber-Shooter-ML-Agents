using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("Environment")]
    [SerializeField] private MLEnvironment MLEnvironment;

    [Header("Zombie Stats")]
    [SerializeField] public float speed = 50.0f;

    public void SetUp(MLEnvironment MLEnvironment)
    {
        this.MLEnvironment = MLEnvironment;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        GameObject collidedWithGO = other.gameObject;
        if (collidedWithGO.tag == "Wall")
        {
            MLEnvironment.DestroyAndRemoveBullet(this.gameObject);
            MLEnvironment.Gun.AddReward(MLEnvironment.Gun.MissedZombiePunishment);
        }
        else if (collidedWithGO.tag == "Obstacle")
        {
            MLEnvironment.DestroyAndRemoveBullet(this.gameObject);
            MLEnvironment.Gun.AddReward(MLEnvironment.Gun.MissedZombiePunishment);
        }
        else if (collidedWithGO.tag == "Zombie")
        {
            MLEnvironment.DestroyAndRemoveBullet(this.gameObject);
            MLEnvironment.DestroyAndRemoveZombie(collidedWithGO);

            MLEnvironment.Gun.AddReward(MLEnvironment.Gun.HitZombieReward);
            if (MLEnvironment.SpawnedZombieList.Count == 0)
            {
                MLEnvironment.Gun.AddReward(MLEnvironment.Gun.HitAllZombieReward);
                //MLEnvironment.Survivor.OverloadedEndEpisode(Color.green, 0, "Killed All Zombies");
            }
        }
    }

    private void FixedUpdate()
    {
        transform.localPosition += transform.right * speed * Time.fixedDeltaTime;
    }
}
