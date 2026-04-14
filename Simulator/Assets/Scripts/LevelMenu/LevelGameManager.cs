using UnityEngine;

public class LevelGameManager : MonoBehaviour
{
    public static LevelGameManager Instance { get; private set; }

    public LevelData currentLevelData; // mevcutLevelVerisi -> currentLevelData

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}