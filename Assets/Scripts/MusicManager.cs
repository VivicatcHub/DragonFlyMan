using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;

    void Awake()
    {
        // Survit aux changements de scène comme GameData
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}