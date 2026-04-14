using StarterAssets;
using System;
using System.Collections;
using UnityEngine;

public class PlayerRaycast : MonoBehaviour
{
    private GameObject playerCamera;
    [SerializeField] private LayerMask raycastLayerToHit;
    [SerializeField] private GameObject carCamera;
    private GameObject playerBody;
    private GameObject carDashBoard;
    private GameObject absHolder;
    [SerializeField] private GameObject rccpCanvas;
    private float raycastDistance = 5f;

    private bool isPlayerInTheCar = false;
    private GameObject car;
    RCCP_CarController carController;

    Coroutine carRoutine;

    void Start()
    {
        playerCamera = transform.parent.Find("MainCamera").gameObject;
        playerBody = transform.Find("Capsule").gameObject;
        carDashBoard = rccpCanvas.transform.Find("Dashboard").gameObject;
        absHolder = rccpCanvas.transform.Find("Holder Abs").gameObject;
    }

    void Update()
    {
        if (!isPlayerInTheCar)
        {
            CheckVehicle();
        }

        if (isPlayerInTheCar)
        {
            if (Input.GetKeyDown(KeyCode.X))
            {
                GetOutCar();
            }
        }
    }

    void CheckVehicle()
    {
        if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out RaycastHit hit, raycastDistance, raycastLayerToHit))
        {
            if (Input.GetKeyDown(KeyCode.X))
            {
                car = hit.collider.gameObject.GetComponentInParent<RCCP_CarController>().gameObject;
                carController = car.GetComponentInParent<RCCP_CarController>();
                GetInCar(hit.collider.gameObject);
            }
        }
    }

    private void GetInCar(GameObject car)
    {
        if (carRoutine != null)
            StopCoroutine(carRoutine);

        carRoutine = StartCoroutine(SetPlayerInCar(true));
    }

    private void GetOutCar()
    {
        if (carRoutine != null)
            StopCoroutine(carRoutine);

        carRoutine = StartCoroutine(SetPlayerInCar(false));
    }

    IEnumerator SetPlayerInCar(bool isPlayerInTheCar)
    {
        yield return new WaitForSeconds(0.5f);
        gameObject.GetComponent<FirstPersonController>().SetIsPlayerInTheCar(isPlayerInTheCar);
        this.isPlayerInTheCar = isPlayerInTheCar;
        playerBody.SetActive(!isPlayerInTheCar);
        playerCamera.SetActive(!isPlayerInTheCar);
        carCamera.SetActive(isPlayerInTheCar);
        carController.enabled = isPlayerInTheCar;
        carDashBoard.gameObject.SetActive(isPlayerInTheCar);
        absHolder.gameObject.SetActive(isPlayerInTheCar);

        if (!isPlayerInTheCar)
        {
            playerBody.transform.parent.position = car.transform.position + new Vector3(-5, 0, 0);
            gameObject.GetComponent<CharacterController>().enabled = true;
        }
        else
        {
            gameObject.GetComponent<CharacterController>().enabled = false;
        }
    }



}
