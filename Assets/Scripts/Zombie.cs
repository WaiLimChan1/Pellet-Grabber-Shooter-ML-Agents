using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Zombie : MonoBehaviour
{
    [Header("Environment")]
    [SerializeField] private MLEnvironment MLEnvironment;

    [Header("Zombie Stats")]
    [SerializeField] public float movementSpeed = 2.0f;

    public void SetUp(MLEnvironment MLEnvironment)
    {
        this.MLEnvironment = MLEnvironment;
    }

    public void FixedUpdate()
    {
        if (MLEnvironment.Survivor == null) return;
        Vector3 direction = (MLEnvironment.Survivor.gameObject.transform.localPosition - transform.localPosition).normalized;
        Vector3 changeVector = direction * movementSpeed * Time.fixedDeltaTime;
        transform.localPosition += changeVector;
    }
}
