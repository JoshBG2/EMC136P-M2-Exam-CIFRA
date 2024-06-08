using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NavmeshPlayerController : MonoBehaviour
{
    private enum AIActionState
    {
        Controlled,
        Mining
    }
    
    [SerializeField]
    private NavMeshAgent agent;
    
    [SerializeField]
    private AIActionState currentState;

    private LineRenderer lineRenderer;
    private Transform crystal;

    // Initialized Raycast Variables
    private int rayCount = 12;
    private float coneAngle = 180f;

    // Initialize Mining Variables
    private float detectRange = 4f;
    private float miningRange = 3f;
    private float miningSpeed = 5f;
    private bool mineCrystal = true;
    private float miningCooldown = 2f;

    private AudioSource audioSource;

    [SerializeField]
    private AudioClip miningSfx;

    void Start()
    {
        // Setup AI Initial State
        currentState = AIActionState.Controlled;

        // Setup Line Renderer
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;
        lineRenderer.positionCount = 2;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default")); // Use a default material
        lineRenderer.startColor = Color.white;
        lineRenderer.endColor = Color.white;
        lineRenderer.enabled = false;

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = miningSfx;
        audioSource.playOnAwake = false;
        audioSource.volume = 1.0f; 
    }

    private void Update()
    {
        // Behavior Tree Switch Cases
        switch (currentState)
        {
            case AIActionState.Controlled:
                ControlledBehavior();
                break;
            case AIActionState.Mining:
                MiningBehavior();
                break;
            default:
                break;
        }
    }

    private void ControlledBehavior()
    {
        if (Input.GetMouseButton(1))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                agent.SetDestination(hit.point);
                lineRenderer.SetPosition(0, transform.position);
                lineRenderer.SetPosition(1, hit.point);
                lineRenderer.enabled = true;
            }
        }

        ToggleLineRenderer();
        
        if (CanSeeCrystal("Crystal") && agent.remainingDistance < detectRange)
        {
            lineRenderer.enabled = false;
            Vector3 destination = crystal.position - (crystal.position - transform.position).normalized * 1.25f;
            agent.SetDestination(destination);

            if (Vector3.Distance(transform.position, destination) <= miningRange)
            {
                currentState = AIActionState.Mining;
            }
        }   
    }

    private void MiningBehavior()
    {
        if (CanSeeCrystal("Crystal") && mineCrystal)
        {
            CrystalNavmeshController crystalController = crystal.GetComponent<CrystalNavmeshController>();
            if (crystalController != null)
            {
                crystalController.MineDamage(miningSpeed);
                StartCoroutine(StartMiningCooldown());

                audioSource.Play();
            }
        }
        if (!CanSeeCrystal("Crystal"))
        {
            currentState = AIActionState.Controlled;
        }

        ToggleLineRenderer();
    }

    private IEnumerator StartMiningCooldown()
    {
        mineCrystal = false;
        yield return new WaitForSeconds(miningCooldown);
        mineCrystal = true;
    }


    private bool CanSeeCrystal(string tag)
    {
        float halfConeAngle = coneAngle / 2f;
        Quaternion startRotation = Quaternion.AngleAxis(-halfConeAngle, transform.up);
        Vector3 raycastDirection = transform.forward;

        for (int i = 0; i < rayCount; i++)
        {
            Quaternion rotation = Quaternion.AngleAxis(i * coneAngle / (rayCount - 1), transform.up);
            Vector3 direction = rotation * startRotation * raycastDirection;

            RaycastHit hit;
            if (Physics.Raycast(transform.position, direction, out hit, detectRange))
            {
                if (hit.collider.CompareTag(tag))
                {
                    Debug.DrawRay(transform.position, direction * hit.distance, Color.red);
                    crystal = hit.transform; // Assign the detected crystal
                    return true;
                }
            }
            Debug.DrawRay(transform.position, direction * detectRange, Color.blue);
        }
        return false;
    }

    private void ToggleLineRenderer()
    {
        // Enable Line Renderer
        if (lineRenderer.enabled)
        {
            lineRenderer.SetPosition(0, transform.position);
        }

        // Disable Line Renderer upon reaching destination
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            if (!agent.hasPath || agent.velocity.sqrMagnitude == 0f)
            {
                lineRenderer.enabled = false;
            }
        }
    }
}
