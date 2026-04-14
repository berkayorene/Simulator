using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class BusStop : MonoBehaviour
{
    int peopleToGetInBus = 0;
    int peopleToGetOutBus = 0;
    private GameObject bus;
    private BusStopController busStopController;
    [SerializeField] private GameObject passenger;
    private List<GameObject> passengers = new List<GameObject>();
    private bool isBusOnStop = false;
    private bool isBusOpenedDoors = false;
    void Start()
    {
        peopleToGetInBus = Random.Range(0, 10);
        
        InstantiatePassengers(transform.position, peopleToGetInBus);
    }

    void Update()
    {
        if (isBusOnStop && busStopController) {
            if (busStopController.GetAreBusDoorsOpen())
            {
                if (passengers.Count > 0)
                {
                    passengers[0].GetComponent<PassengerNavMesh>().MovePassenger(bus);
                }
                busStopController.SetAreBusDoorsOpen(false);
            }
        }

        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.transform.parent.transform.parent)
        {
            if (isBusOnStop && other.gameObject.transform.parent.transform.parent)
            {

                if (other.gameObject.transform.parent.transform.parent.GetComponent<RCCP_CarController>())
                {
                    bus = other.gameObject.transform.parent.transform.parent.gameObject;
                    busStopController = bus.gameObject.GetComponent<BusStopController>();
                }
            }
        }
        
    }

    public int GetPeopleToGetInBus()
    {
        return peopleToGetInBus;
    }

    public void ChangePeopleToGetInBus(int value)
    {
        peopleToGetInBus += value;
    }

    private void InstantiatePassengers(Vector3 position, int numberOfPassengers)
    {
        
        for (int i = 0; i < numberOfPassengers; i++)
        {
            GameObject pass = Instantiate(passenger, position + new Vector3(-2, 0, 2 * i), passenger.transform.rotation * Quaternion.Euler(0, -180f, 0));
            pass.GetComponent<PassengerNavMesh>().SetBusStop(gameObject.GetComponent<BusStop>());
            pass.GetComponentInChildren<Animator>().SetBool("isWalking", false);
            passengers.Add(pass);
        }
        
    }

    public void SetIsBusOnStop(bool value)
    {
        isBusOnStop = value;
    }

    public bool GetIsBusOnStop()
    {
        return isBusOnStop;
    }

    public void SetisBusOpenedDoors(bool value)
    {
        isBusOpenedDoors = value;
    }

    public bool GetisBusOpenedDoors()
    {
        return isBusOpenedDoors;
    }

    public void RemovePassenger(GameObject passenger)
    {
        passengers.Remove(passenger);
        if(passengers.Count > 0)
        {
            passengers[0].GetComponent<PassengerNavMesh>().MovePassenger(bus);
        }
    }
}
