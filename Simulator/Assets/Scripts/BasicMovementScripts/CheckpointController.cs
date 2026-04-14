using System.Collections.Generic;
using UnityEngine;

public class CheckPointController : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] private CheckPoint[] checkPoints;
    private GameObject[] checkPointObjects;

    private int currentCheckpointIndex = 0;

    private int totalCheckPoints;

    private ProgressBar progressBar;
    private float progress = 0f;


    private void Start()
    {
        totalCheckPoints = checkPoints.Length;
    }

    public void TryPassCheckpoint(CheckPoint cp)
    {
        if (checkPoints[currentCheckpointIndex].getCheckPointNumber() == cp.getCheckPointNumber())

        {
            if (!cp.isPassed)
            {
                cp.isPassed = true;
                Debug.Log($"Checkpoint {cp.getCheckPointNumber()} passed!");

                progressBar = gameObject.GetComponent<ProgressBar>();




                progress += (1.0f / (float)totalCheckPoints);


                progressBar.SetProgress(progress);


                currentCheckpointIndex++;

                if (currentCheckpointIndex >= totalCheckPoints)
                {
                    Debug.Log("🎉 All checkpoints passed! Race complete.");
                    progress = 1.0f;
                    progressBar.SetProgress(progress);
                }
            }
        }
        else
        {
            Debug.LogWarning($"Wrong checkpoint! You're supposed to go to checkpoint {checkPoints[currentCheckpointIndex].getCheckPointNumber()}");
        }
    }







    // Update is called once per frame





}
