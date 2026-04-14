using UnityEngine;

public class CPInstance : MonoBehaviour
{
    [SerializeField] private int cpID;
    public int CpID
    {
        get { return cpID; }
        set { cpID = value; }
    }
}