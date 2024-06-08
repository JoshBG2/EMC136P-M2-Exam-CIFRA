using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleController : MonoBehaviour
{
    public float wallSpeed = 5f;  
    private Vector3 direction;

    void Start()
    {
        // Wall Direction Randmozier
        direction = Random.value > 0.5f ? Vector3.left : Vector3.right;
    }

    // Wall Movement
    void Update()
    {
        transform.Translate(direction * wallSpeed * Time.deltaTime);
    }

    // Wall Direction Switch
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("OuterWall"))
        {
            direction = -direction;
        }
    }
}
