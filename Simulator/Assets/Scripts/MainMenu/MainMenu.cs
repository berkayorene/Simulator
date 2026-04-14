using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{

    public GameObject settingsPanel;
    public void RoadnCar()
    {
        // Load the RoadnCar scene
        SceneManager.LoadScene("LevelMenu");
    }

    public void VideoPlayer()
    {
        // Load the VideoPlayer scene
        SceneManager.LoadScene("Video Player");
    }
    // Settings butonuna atanacak
    public void OpenSettings()
    {
        settingsPanel.SetActive(true);
    }

    // Geri butonuna atanacak
    public void CloseSettings()
    {
        settingsPanel.SetActive(false);
    }
    public void link()
    {
        // Open the link in the default web browser
        Application.OpenURL("https://www.youtube.com/watch?v=K_NNPfMqduo");
    }
    public void Quit()
    {
        // Quit the application
        Application.Quit();
#if UNITY_EDITOR
        // If running in the editor, stop playing the scene
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }


}
