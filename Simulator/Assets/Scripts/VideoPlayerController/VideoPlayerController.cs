using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class VideoPlayerController : MonoBehaviour
{
    [SerializeField] private RawImage rawImage;
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private AudioSource audioSource;

    [SerializeField] private Slider audioPlayerSlider;
    [SerializeField] private Slider videoPlayerSlider;
    private bool isDragging = false;

    [SerializeField] private GameObject screenUI;
    [SerializeField] private GameObject screen;
    [SerializeField] private GameObject videoTime;
    [SerializeField] private GameObject playBackSpeed;
    [SerializeField] private GameObject playBackSpeedIncreaseBtn;
    [SerializeField] private GameObject playBackSpeedDecreaseBtn;

    private Vector3 lastMousePosition;
    private float lastMouseMoveTime;
    private float screenUIVisibilityTime = 3;


    void Start()
    {
        videoPlayerSlider.onValueChanged.AddListener(HandleTimeSliderValueChanged);

        lastMousePosition = Input.mousePosition;
        lastMouseMoveTime = Time.time;
    }



    void Update()
    {
        UpdateSlider();
        UpdateTime();

        if (videoPlayer && screen.activeInHierarchy) { 
            HandleUIScreenVisibility();
        }
    }

    public void PlayVideo(string videoPath)
    {
        screen.SetActive(true);
        screenUI.SetActive(true);
        string fullPath = System.IO.Path.Combine(Application.streamingAssetsPath, videoPath);
        fullPath = fullPath.Replace("\\", "/");
        string videoUrl = "file://" + fullPath;
        videoPlayer.source = VideoSource.Url;
        videoPlayer.url = videoUrl;
        videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
        videoPlayer.SetTargetAudioSource(0, audioSource);

        videoPlayer.Prepare();

        videoPlayer.prepareCompleted += (vp) => {
            rawImage.texture = videoPlayer.texture;
            videoPlayer.Play();
            audioSource.Play();
        };
    }

    public void TogglePlayPauseVideo()
    {
        if (videoPlayer.isPlaying)
        {
            videoPlayer.Pause();
            audioSource.Pause();
        }
        else
        {
            videoPlayer.Play();
            audioSource.Play();
        }


    }

    private void UpdateSlider()
    {
        if (videoPlayer.isPlaying && !isDragging)
        {
            videoPlayerSlider.value = (float)videoPlayer.time;

            if (videoPlayerSlider.maxValue != (float)videoPlayer.length)
            {
                videoPlayerSlider.maxValue = (float)videoPlayer.length;
            }
        }
    }

    public void HandleAudioSliderValueChanged(float value)
    {
        Debug.Log("Value: " + value + " Volume: " + audioSource.volume);
        audioSource.volume = audioPlayerSlider.value;
        videoPlayer.SetDirectAudioVolume(0, audioPlayerSlider.value);
    }
    public void HandleTimeSliderValueChanged(float value)
    {
        // Ses sonuna kadar açýk olucaðý için handle yuvarlýðý sonda kalsýn 
        if (isDragging && videoPlayer) {
            videoPlayer.time = value;
        }
    }

    public void BeginDrag()
    {
        isDragging = true;
    }

    public void EndDrag()
    {
        isDragging = false;
    }

    public void ToggleMute()
    {
        if (videoPlayer)
        {
            bool isMuted = videoPlayer.GetDirectAudioMute(0);
            videoPlayer.SetDirectAudioMute(0, !isMuted);
        }
    }

    public void IncreasePlayBackSpeed()
    {
        if (videoPlayer)
        {
            if (videoPlayer.playbackSpeed != 2)
            {
                videoPlayer.playbackSpeed += 0.25f;
                playBackSpeed.GetComponentInChildren<TMP_Text>().text = videoPlayer.playbackSpeed.ToString();

            }
        }
    }

    public void DecreasePlayBackSpeed()
    {
        if (videoPlayer)
        {
            if (videoPlayer.playbackSpeed != 0.25f)
            {
                videoPlayer.playbackSpeed -= 0.25f;
                playBackSpeed.GetComponentInChildren<TMP_Text>().text = videoPlayer.playbackSpeed.ToString();

            }
        }
    }

    public void SetPlayBackSpeedToDefault()
    {
        if (videoPlayer)
        {

            videoPlayer.playbackSpeed = 1f;
            playBackSpeed.GetComponentInChildren<TMP_Text>().text = videoPlayer.playbackSpeed.ToString();


        }
    }

    private void UpdateTime()
    {
        if (videoPlayer)
        {
            int minutes = (int)videoPlayer.time / 60;
            int seconds = (int)videoPlayer.time % 60;
            string timeFormatted = string.Format("{0}:{1:00}", minutes, seconds);

            int minutesTotal = (int)videoPlayer.length / 60;
            int secondsTotal = (int)videoPlayer.length % 60;
            string timeFormattedTotal = string.Format("{0}:{1:00}", minutesTotal, secondsTotal);

            videoTime.GetComponentInChildren<TMP_Text>().text = timeFormatted + "/" + timeFormattedTotal;
        }
    }

    private void HandleUIScreenVisibility()
    {
        if(Input.mousePosition != lastMousePosition)
        {
            lastMouseMoveTime = Time.time;
            lastMousePosition = Input.mousePosition;
            screenUI.SetActive(true);
        }

        if (Time.time - lastMouseMoveTime > screenUIVisibilityTime) { 
            screenUI.SetActive(false);
        }
    }



}
