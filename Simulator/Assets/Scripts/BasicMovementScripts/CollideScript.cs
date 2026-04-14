using System;
using UnityEngine;

public class CollideScript : MonoBehaviour
{

    [SerializeField] private CheckPointController checkPointController;


    private void Start()
    {
        
    }

    /*
    void Start()
    {

        if (checkPointController == null)
        {
            checkPointController = FindObjectOfType<CheckPointController>();
        }



    }
    */
    

    private void OnTriggerEnter(Collider other)
    {

        print("ENTERING");

        CheckPoint cp = other.GetComponent<CheckPoint>();

        if (cp != null && checkPointController != null)
        {
            checkPointController.TryPassCheckpoint(cp);
        }


    }

    private void OnTriggerStay(Collider other)
    {
       // print("COLLIDING A BIT");


    }

    private void OnTriggerExit(Collider other)
    {
        print("EXIT");


    }



    




}
