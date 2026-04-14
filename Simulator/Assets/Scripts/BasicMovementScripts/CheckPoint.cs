using Unity.XR.Oculus.Input;
using UnityEngine;

public class CheckPoint : MonoBehaviour
{

    private int checkPointNumber;
    private string name;

    public bool isPassed = false;



    private void Start()
    {
        string name = gameObject.name;

        string[] splittedName = name.Split(' ');
        int splittedNameSize = name.Split(' ').Length;


        string input = splittedName[splittedNameSize - 1]; // something like "(12)" 

        input = input.Replace("(", "").Replace(")", ""); // "12"

        int number = int.Parse(input);

        checkPointNumber = number;
    }

    public int getCheckPointNumber() { return checkPointNumber; }
    public void setCheckPointNumber(int value) { checkPointNumber = value; }



    /*
    int getCheckPointNumber() { 



    }
    */







}