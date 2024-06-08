using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BehaviorTreeAI : MonoBehaviour
{
    private enum AIActionState
    {
        Patrolling,
        Chasing,
        Attacking,
        Searching
    }

    [SerializeField]private AIActionState currentState;
    
    private NavMeshAgent navMeshAgent;

    private Transform player;
    private Vector3 patrolPoint;
    private float chaseRange = 10f;
    private int rayCount = 5;
    private float coneAngle = 60f;
    private float attackRange = 1f;
    // private float attackDamage = 1f;
    private float searchDuration = 5f;
    private float searchTimer = 0f;
    private Vector3 lastKnownPlayerPosition;

    private void Start()
    {
        currentState = AIActionState.Patrolling;
        navMeshAgent = GetComponent<NavMeshAgent>();
        player = FindAnyObjectByType<NavmeshPlayerController>().transform;
        patrolPoint = GetRandomPoint(); 
        navMeshAgent.SetDestination(patrolPoint);

    }

    private Vector3 GetRandomPoint()
    {
        Vector3 randomDirection = UnityEngine.Random.insideUnitSphere * 10f;
        randomDirection += transform.position;

        NavMeshHit navHit;
        NavMesh.SamplePosition(randomDirection, out navHit, 10f, -1);

        var destination = navHit.position;
        return destination;
    }

    private void Update()
    {
        switch (currentState)
        {
            // Can make different behaviors to different scripts and call those script behaviors
            case AIActionState.Patrolling:
                PatrollingBehavior(); 
                break;
            case AIActionState.Chasing:
                ChasingBehavior();
                break;
            case AIActionState.Attacking:
                AttachkingBehavior();
                break;
            case AIActionState.Searching:
                SearchingBehavior();
                break;
            default:
                break;
        }
    }
     private void PatrollingBehavior()
    {
        if (navMeshAgent.remainingDistance < 0.05f)
        {
            patrolPoint = GetRandomPoint();
            navMeshAgent.SetDestination(patrolPoint);
        }

        if (CanSeePlayer("Player") && navMeshAgent.remainingDistance < chaseRange)
        {
            currentState = AIActionState.Chasing;
        }

        // talk/interact to another ai, investigate
    }
   
    private void ChasingBehavior()
    {
        navMeshAgent.SetDestination(player.position);

        if (Vector3.Distance(transform.position, player.position) < attackRange) 
        {
            currentState = AIActionState.Attacking;
        }

        if (!CanSeePlayer("Player"))
        {
            lastKnownPlayerPosition = player.position;
            currentState = AIActionState.Searching;
            searchTimer = 0f;
        }

        //Flanking, calling for backup
    }

    private void AttachkingBehavior()
    {
        if (CanSeePlayer("Player"))
        {
            NavmeshPlayerController playerController = FindObjectOfType<NavmeshPlayerController>();
            if (playerController != null)
            {
                //Take Damage Function on Player Controller
            }
        }
        if (!CanSeePlayer("Player"))
        {
            currentState = AIActionState.Searching;
            searchTimer = 0f;
        }
    }

    private void SearchingBehavior()
    {
        searchTimer += Time.deltaTime;

        // Continue searching for a specified duration
        if (searchTimer < searchDuration)
        {
            // e.g scanning the area by rotating agent or checking hiding spots or check players last position
            navMeshAgent.SetDestination(lastKnownPlayerPosition);

            if (CanSeePlayer("Player") && navMeshAgent.remainingDistance < chaseRange)
            {
                currentState = AIActionState.Chasing;
                return;
            }

        }
        else
        {
            currentState = AIActionState.Patrolling;
        }

        //revisit the last know location, communicate with another AI
    }

    private bool CanSeePlayer(string tag)
    {
        float halfConeAngle = coneAngle / 2f;
        Quaternion startRotation = Quaternion.AngleAxis(-halfConeAngle, transform.up);
        Vector3 raycastDirection = transform.forward;

        for (int i = 0; i < rayCount; i++)
        {
            Quaternion rotation = Quaternion.AngleAxis(i * coneAngle / (rayCount - 1), transform.up);
            Vector3 direction = rotation * startRotation * raycastDirection;

            // RAYCAST IF STATEMENT
            RaycastHit hit;
            if (Physics.Raycast(transform.position, direction, out hit, chaseRange))
            {
                if (hit.collider.CompareTag(tag))
                {
                    Debug.DrawRay(transform.position, direction * hit.distance, Color.red);
                    return true;
                }
            }
            Debug.DrawRay(transform.position, direction * chaseRange, Color.blue);
        }
        return false;
    }
}
