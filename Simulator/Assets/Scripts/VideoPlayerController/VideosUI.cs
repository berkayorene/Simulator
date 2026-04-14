using System;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class VideosUI : MonoBehaviour
{
    [SerializeField] private VideoPlayerController videoPlayerController;

    [SerializeField] private Transform topic_Names_Parent;
    [SerializeField] private Transform video_Names_Parent;
    [SerializeField] private GameObject lesson_Button_Prefab;

    private string streamingAssetsPath;


    void Start()
    {
        streamingAssetsPath = Application.streamingAssetsPath;
        ShowTopics();
    }



    void Update()
    {

    }

    private void ShowTopics()
    {

        string[] folders = Directory.GetDirectories(streamingAssetsPath);

        foreach (string folderPath in folders)
        {
            string folderName = Path.GetFileName(folderPath);
            GameObject btnObj = Instantiate(lesson_Button_Prefab, topic_Names_Parent.transform);

            btnObj.GetComponentInChildren<TMP_Text>().text = folderName;

            btnObj.GetComponent<Button>().onClick.AddListener(() =>
            {
                topic_Names_Parent.gameObject.SetActive(false);
                ShowVideos(folderName);
            });
        }


    }

    private void ShowVideos(string folderName)
    {
        string folderPath = Path.Combine(Application.streamingAssetsPath, folderName);

        string stringPartToRemove = ".mp4";

        if (Directory.Exists(folderPath))
        {
            string[] files = Directory.GetFiles(folderPath, "*.mp4");
            foreach (string file in files)
            {
                string videoName = Path.GetFileName(file);
                GameObject btnObj = Instantiate(lesson_Button_Prefab, video_Names_Parent.transform);
                string videoPath = Path.Combine(folderPath, videoName);
                btnObj.GetComponentInChildren<TMP_Text>().text = videoName.Replace(stringPartToRemove, "");

                btnObj.GetComponent<Button>().onClick.AddListener(() =>
                {
                    video_Names_Parent.gameObject.SetActive(false);
                    PlayVideo(videoPath);
                });
            }
        }
    }

    private void PlayVideo(string videoPath)
    {
        videoPlayerController.PlayVideo(videoPath);
    }


}
