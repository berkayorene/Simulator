using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public GameObject pausePanel;

    public GameObject settingsPanel;
    private bool isPaused = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            if (isPaused)
                ResumeSimulator();
            else
                PauseGame();
        }
    }

    public void ResumeSimulator()
    {
        pausePanel.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Time.timeScale = 1f;
        isPaused = false;
    }

    public void PauseGame()
    {
        pausePanel.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Time.timeScale = 0f;
        isPaused = true;
    }

    public void RestartLevel()
    {
        // UI panellerini kapat
        if (pausePanel != null) pausePanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);

        // Zamanı normale döndür
        Time.timeScale = 1f;

        // Sahneyi yeniden yükle
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }


    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("AnaMenü");
    }

    public void OpenSettings()
    {
        settingsPanel.SetActive(true);
        pausePanel.SetActive(false);
    }

    public void BackToPauseMenu()
    {
        settingsPanel.SetActive(false);  // Settings kapanır
        pausePanel.SetActive(true);      // Pause menüsü geri açılır
    }


}

