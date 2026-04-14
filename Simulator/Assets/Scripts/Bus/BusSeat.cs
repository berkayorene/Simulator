using UnityEngine;

public class BusSeat : MonoBehaviour
{
    [SerializeField] private bool isOccupied;
    [SerializeField] private Vector3 localPosition;
    private int index;


    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public bool GetIsOccupied()
    {
        return isOccupied;
    }

    public void SetIsOccupied(bool isOccupied)
    {
        this.isOccupied = isOccupied;
    }

    public Vector3 GetLocalPosition()
    {
        return localPosition;
    }

    public void Init(int index, Vector3 position)
    {
        isOccupied = Random.Range(0, 2) == 1 ? true : false;
        localPosition = position;
        this.index = index;
    }

    public int GetIndex() { 
        return index;
    }
}
