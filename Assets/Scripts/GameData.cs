using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class InsectData
{
    public int     level;
    public int     cellX;
    public int     cellY;
    public AntType antType = AntType.Ant;
}

[System.Serializable]
public class PathResult
{
    public PathType type;
    public int rewardLevel;
    public int enemyLevel;
    public bool enemyDefeated;
    public int trapX;
    public string resultLog;
    public List<InsectData> survivors = new();
    public List<InsectData> rewards   = new();
}

public class GameData : MonoBehaviour
{
    public static GameData Instance;

    public int currentPhase = 1;
    public List<InsectData> mainGridInsects = new();
    public PathResult[]     pathResults     = new PathResult[3];
    public AntType rewardAntType = AntType.Ant;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}