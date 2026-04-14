using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BusStopController : MonoBehaviour
{

    private GameObject stop;
    private bool isBusNotMoving = false;
    private bool isBusOnStop = false;
    private bool areBusDoorsOpen = false;
    [SerializeField] private BusSeatController busSeatController;
    private BusStop busStop;
    BusSeat[] busSeats;
    [SerializeField] private BusCOMController busCOMController;

    
    [SerializeField] private GameObject frontDoor;
    [SerializeField] private GameObject backDoor;

    [SerializeField] private GameObject passenger;
    [SerializeField] private GameObject RCCP_Canvas;
    private GameObject notOnBusStopUI;

    void Start()
    {
        notOnBusStopUI = RCCP_Canvas.transform.Find("Not On Bus Stop").gameObject;
    }

    void Update()
    {
        CheckIfBusNotMoving();

        if (stop != null)
        {
            if (isBusOnStop && !stop.GetComponent<BusStop>().GetisBusOpenedDoors())
            {
                if (Input.GetKeyDown(KeyCode.R) && isBusNotMoving)
                {
                    HandlePassengersGetInAndOut();
                    stop.GetComponent<BusStop>().SetisBusOpenedDoors(true);
                }
            }
        }

        if (!isBusOnStop && Input.GetKeyDown(KeyCode.R))
        {
            StartCoroutine(ShowNotOnBusStopUI());
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<BusStop>())
        {
            stop = other.gameObject;
            busStop = stop.GetComponent<BusStop>();
            busStop.SetIsBusOnStop(true);
            isBusOnStop = true;
        }

    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<BusStop>())
        {
            stop = other.gameObject;
            busStop = stop.GetComponent<BusStop>();
            busStop.SetIsBusOnStop(false);
            isBusOnStop = false;
            
        }
    }

    private IEnumerator ShowNotOnBusStopUI()
    {
        notOnBusStopUI.SetActive(true);
        yield return new WaitForSeconds(2);
        notOnBusStopUI.SetActive(false);
    }


    private void CheckIfBusNotMoving()
    {
        float velocity = gameObject.GetComponent<Rigidbody>().linearVelocity.magnitude;
        if(velocity < 0.1f)
        {
            isBusNotMoving = true;
        }
        else
        {
            isBusNotMoving = false;
        }
       
    }

    private void HandlePassengersGetInAndOut()
    {
        areBusDoorsOpen = true; // make it false after 
        busSeats = busSeatController.GetBusSeats();
        // get out
        int occupiedSeats = busSeatController.GetHowManySeatOccupied();
        int peopleToGetOutBus = Random.Range(0, occupiedSeats + 1);
        Debug.Log(peopleToGetOutBus);
        for (int i = 0; i < peopleToGetOutBus; i++)
        {
            Debug.Log(i);
            BusSeat seat = busSeats[GetRandomSeatIndex(busSeatController.GetOccupiedSeatsIndexes())];
            seat.SetIsOccupied(false);
        }
        StartCoroutine(InstantiatePassengers(backDoor.transform.position, peopleToGetOutBus));

        // get in
        Debug.Log("Durakta bekleyen bindikten önce: " + busStop.GetPeopleToGetInBus());
        int peopleToGetInBus = busStop.GetPeopleToGetInBus();
        //InstantiatePassengers(frontDoor.transform.position, peopleToGetInBus, true);
        int unoccupiedSeats = busSeatController.GetHowManySeatUnoccupied();
        for (int i = 0; i < Mathf.Min(peopleToGetInBus, unoccupiedSeats); i++)
        {

            busStop.ChangePeopleToGetInBus(-1);
            BusSeat seat = busSeats[GetRandomSeatIndex(busSeatController.GetEmptySeatsIndexes())];
            seat.SetIsOccupied(true);
        }
        Debug.Log("Durakta bekleyen bindikten sonra: " + busStop.GetPeopleToGetInBus());

        CalculateCOM();
    }

    private int GetRandomSeatIndex(List<int> seats)
    {
        int randomIndex = Random.Range(0, seats.Count);
        return seats[randomIndex];
    }

    private void CalculateCOM()
    {
        Vector3 sum = Vector3.zero;
        int count = 0;

        foreach (var seat in busSeatController.seats)
        {
            if (seat.GetIsOccupied())
            {
                sum += seat.GetLocalPosition();
                count++;
            }
        }


        Vector3 com = sum / count;

        //busSeatController.GetComponent<Rigidbody>().centerOfMass = com;
        busCOMController.UpdateCOM(com, count);


    }

    public bool GetAreBusDoorsOpen()
    {
        return areBusDoorsOpen;
    }

    public void SetAreBusDoorsOpen(bool areBusDoorsOpen)
    {
        this.areBusDoorsOpen = areBusDoorsOpen;
    }

    private IEnumerator InstantiatePassengers(Vector3 position, int numberOfPassengers)
    {
        for (int i = 0; i < numberOfPassengers; i++)
        {
            GameObject pass = Instantiate(passenger, position + new Vector3(1, 0, 0), passenger.transform.rotation);
            pass.GetComponent<PassengerNavMesh>().SetBusStop(GetComponent<BusStop>());
            pass.GetComponent<PassengerNavMesh>().GetOutFromBus(pass.transform.position + new Vector3(0, 0, -10));
            StartCoroutine(DestroyPassengerAfterDelay(pass, 3f));

            yield return new WaitForSeconds(1f);
        }
    }

    private IEnumerator DestroyPassengerAfterDelay(GameObject passenger, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (passenger != null)
        {
            Destroy(passenger);
        }
    }

}
