using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;

public class Patrolling : MonoBehaviour
{
    [Header("Patrol Settings")]
    public List<Transform> patrolPoints;
    public float waitTime = 0f;
    public float rotationSpeed = 3f;
    public bool isCircular = true;

    [Header("Root Motion")]
    public bool useCustomRootMotion = false;

    private NavMeshAgent agent;
    private Animator animator;

    private int currentPointIndex = 0;
    private int direction = 1;
    private bool isPatrolling = false;
    private bool isWaiting = false;
    private bool isRotating = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        agent.autoBraking = true;

        agent.updatePosition = !useCustomRootMotion;
        agent.updateRotation = !useCustomRootMotion;
    }

    void Update()
    {
        if (patrolPoints == null || patrolPoints.Count < 2) return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            isPatrolling = !isPatrolling;

            if (isPatrolling)
            {

                GoToNextPoint();
            }
            else
            {
                agent.ResetPath();
                animator.SetBool("isWalking", false);
            }
        }

        if (isPatrolling && !isWaiting && !isRotating && !agent.pathPending && agent.remainingDistance < 0.5f)
        {
            StartCoroutine(RotateTowardsNextPoint());
        }

        animator.SetBool("isWalking", agent.velocity.magnitude > 0.05f);
    }

    IEnumerator RotateTowardsNextPoint()
    {
        isWaiting = true;
        isRotating = true;

        agent.isStopped = true;
        animator.SetBool("isWalking", false);

        // ➕ Nokta sırası ayarlanıyor
        if (isCircular)
        {
            currentPointIndex = (currentPointIndex + 1) % patrolPoints.Count;
        }
        else
        {
            currentPointIndex += direction;

            if (currentPointIndex >= patrolPoints.Count)
            {
                currentPointIndex = patrolPoints.Count - 2;
                direction = -1;
            }
            else if (currentPointIndex < 0)
            {
                currentPointIndex = 1;
                direction = 1;
            }
        }

        Transform nextPoint = patrolPoints[currentPointIndex];
        Vector3 lookDir = (nextPoint.position - transform.position).normalized;
        Quaternion targetRot = Quaternion.LookRotation(new Vector3(lookDir.x, 0, lookDir.z));

        while (Quaternion.Angle(transform.rotation, targetRot) > 1f)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
            yield return null;
        }

        yield return new WaitForSeconds(waitTime);

        isRotating = false;
        isWaiting = false;

        agent.destination = nextPoint.position;
        agent.isStopped = false;
        animator.SetBool("isWalking", true);
    }

    void GoToNextPoint()
    {
        if (patrolPoints == null || patrolPoints.Count == 0) return;

        agent.destination = patrolPoints[currentPointIndex].position;
        agent.isStopped = false;
        animator.SetBool("isWalking", true);
    }

    void OnAnimatorMove()
    {
        if (!animator) return;

        if (useCustomRootMotion)
        {
            Vector3 deltaPosition = animator.deltaPosition;
            Quaternion deltaRotation = animator.deltaRotation;

            transform.position += deltaPosition * 2f;
            transform.rotation *= Quaternion.Slerp(Quaternion.identity, deltaRotation, 2f);
        }
        else
        {
            transform.position += animator.deltaPosition;
            transform.rotation *= animator.deltaRotation;
        }
    }
}
