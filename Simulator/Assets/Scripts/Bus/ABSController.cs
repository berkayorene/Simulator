using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ABSController : MonoBehaviour
{
    [SerializeField] private Button button;
    private bool isActive = true;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetBtnText() {
        isActive = !isActive;
        if (isActive)
        {
            
            button.GetComponentInChildren<TMP_Text>().text = button.name + " active";

        }
        else
        {
            button.GetComponentInChildren<TMP_Text>().text = button.name + " inactive";

        }
        
    }
}
