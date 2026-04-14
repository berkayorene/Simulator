using UnityEngine;

public class GameData : MonoBehaviour
{
    public static GameData Instance;

    public string sonKayitDosyaYolu;
    public string sonHavaDosyaYolu;

    public LevelData aktifLevelData;

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
