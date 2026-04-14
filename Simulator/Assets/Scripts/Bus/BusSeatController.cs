using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class BusSeatController : MonoBehaviour
{
    public int rows = 5;
    public int columns = 4;
    public BusSeat[] seats;

    private int middleIndex;
    private float corridorWidth;
    private float seatWidth ;
    private float seatLenght;
    [SerializeField] private Transform previousSeatTransform;
    private Vector3 previousSeatPosition;
    private int previousColumnIndex = 0;

    private int occupiedSeatNumber = 0;
    private int unoccupiedSeatNumber = 0;

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public void OnValidate()
    {
        Init();
        seats = new BusSeat[rows * columns];
        for (int i = 0; i < seats.Length; i++)
        {
            seats[i] = new BusSeat();
            seats[i].Init(i, CalculateSeatPosition(i));
        }
        
        
    }

    private Vector3 CalculateSeatPosition(int index)
    {
        
        
        Vector3 position = previousSeatPosition;

        
        int columnIndex = previousColumnIndex + 1;

        if(columnIndex == middleIndex)
        {
            position.x += corridorWidth;
        }else if(columnIndex == columns)
        {
            columnIndex = 0;
            position.z -= seatLenght;
            position.x -= (seatWidth * (columns -2) + corridorWidth);
        }
        else
        {
            position.x += seatWidth;
        }
        previousSeatPosition = position;
        previousColumnIndex = columnIndex;
        return position;
    }

    private void Init()
    {
        corridorWidth = 1;
        seatWidth = 0.5f;
        seatLenght = 1.5f;

        previousColumnIndex = -1;
        middleIndex = columns / 2;
        previousSeatPosition = previousSeatTransform.transform.localPosition;
    }

    public int GetHowManySeatOccupied()
    {
        occupiedSeatNumber = 0;
        foreach (BusSeat seat in seats)
        {
            if (seat.GetIsOccupied())
            {
                occupiedSeatNumber++;
            }
        }
        return occupiedSeatNumber;
    }

    public int GetHowManySeatUnoccupied()
    {
        unoccupiedSeatNumber =  0;
        foreach (BusSeat seat in seats)
        {
            if (!seat.GetIsOccupied())
            {
                unoccupiedSeatNumber++;
            }
        }
        return unoccupiedSeatNumber;
    }

    public BusSeat[] GetBusSeats() { 
        return seats;
    }

    public List<int> GetEmptySeatsIndexes()
    {
        List<int> emptySeats = new List<int>();
        for (int i = 0; i < seats.Length; i++)
        {
            BusSeat seat = seats[i];

            if (!seat.GetIsOccupied())
            {
                emptySeats.Add(seat.GetIndex());
            }
        }

        return emptySeats;
    }

    public List<int> GetOccupiedSeatsIndexes()
    {
        List<int> occupiedSeats = new List<int>();
        for (int i = 0; i < seats.Length; i++)
        {
            BusSeat seat = seats[i];

            if (seat.GetIsOccupied())
            {
                occupiedSeats.Add(seat.GetIndex());
            }
        }

        return occupiedSeats;
    }

    
}
