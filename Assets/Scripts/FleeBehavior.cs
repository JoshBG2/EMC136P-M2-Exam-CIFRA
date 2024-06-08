using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class FleeBehavior : MonoBehaviour
{

    public Transform target; // The threat to flee from
    private NavMeshAgent agent;
    public float fleeDistance = 5f;

    public static bool isFleeing;
    void Start()
    {
        target = FindObjectOfType<NavmeshPlayerController>().transform;
        agent = GetComponent<NavMeshAgent>();

        if (IsTargetClose())
        {
            // method for the flee behavior
            FleeFromTheTarget();
        }
    }

    void Update()
    {
        if (IsTargetClose())
        {
            FleeFromTheTarget();
        }
    }

    private bool IsTargetClose()
    {
        float distanceTarget = Vector3.Distance(transform.position, target.position);
        isFleeing = distanceTarget <= fleeDistance;
        return isFleeing;
    }

    private void FleeFromTheTarget()
    {
        if (target != null)
        {
            Vector3 fleeDirection = transform.position - target.position;
            Vector3 fleePosition = transform.position + fleeDirection.normalized * 10f; // Adjust this value to whatever you want

            NavMesh.SamplePosition(fleePosition, out NavMeshHit navHit, 10, NavMesh.AllAreas);
            agent.SetDestination(navHit.position);
        }
    }
}
