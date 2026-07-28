using System.Collections.Generic;

public enum PathType { Object, Enemy, Trap }

[System.Serializable]
public class PathData
{
    public PathType type;
    public int rewardLevel;
    public int enemyLevel;
    public bool enemyDefeated;
    public int trapX;
    public List<GridObject> assignedInsects = new();
    public List<GridObject> survivors = new();
    public List<GridObject> rewards = new();   // nouveaux insectes gagnés
    public string resultLog = "";
    public AntType rewardAntType = AntType.Ant;
}