using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class PassengerNavMesh : MonoBehaviour
{
    private NavMeshAgent navMeshAgent;
    private GameObject bus;
    private GameObject busFrontDoor;
    private BusStop busStop;
    private bool isBusReadyToGetIn = false;
    private bool isWalking = false;
    private Vector3 targetPosition;
    void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        if (isBusReadyToGetIn)
        {
            transform.GetComponentInChildren<Animator>().SetBool("isWalking", true);
            navMeshAgent.SetDestination(busFrontDoor.transform.position);
            float distance = Vector3.Distance(transform.position, busFrontDoor.transform.position);
            if (distance < 1.5f)
            {
                busStop.RemovePassenger(gameObject);
                Destroy(gameObject);
            }
        }

        if (isWalking)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, 2 * Time.deltaTime);
        }
    }

    
    public void MovePassenger(GameObject bus)
    {
        this.bus = bus;
        busFrontDoor = bus.transform.Find("irizar I6/FrontDoor").gameObject;
        isBusReadyToGetIn = true;
    }

    public void SetBusStop(BusStop busStop)
    {
        this.busStop = busStop;
    }

    public void GetOutFromBus(Vector3 targetPosition)
    {
        this.targetPosition = targetPosition;
        StartCoroutine(RotateRight());
        isWalking = true;
        

        
    }

    private IEnumerator RotateRight()
    {
        Quaternion targetRotation = Quaternion.Euler(0, 90, 0) * transform.rotation;

        while (Quaternion.Angle(transform.rotation, targetRotation) > 0.1f)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5);
            yield return null;
        }

    }
}
