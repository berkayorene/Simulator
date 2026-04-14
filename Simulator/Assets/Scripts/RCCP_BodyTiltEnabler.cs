using UnityEngine;

public class RCCP_BodyTiltEnabler : MonoBehaviour
{
    [SerializeField] private GameObject body;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //body = GetComponentInChildren<RCCP_BodyTilt>().gameObject;


        body.SetActive(true);

    }

    // Update is called once per frame
    void Update()
    {

    }
}
