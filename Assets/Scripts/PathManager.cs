using UnityEngine;
using System.Collections.Generic;

public class PathManager : MonoBehaviour
{
    public static PathManager Instance;
    public PathData[] paths = new PathData[3];

    void Awake() => Instance = this;

    public void GeneratePaths()
    {
        int phase = GameData.Instance.currentPhase;

        var types = new List<PathType> { PathType.Object, PathType.Enemy, PathType.Trap };
        for (int i = types.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (types[i], types[j]) = (types[j], types[i]);
        }

        for (int i = 0; i < 3; i++)
        {
            paths[i] = new PathData { type = types[i] };

            switch (types[i])
            {
                case PathType.Object:
                    paths[i].rewardLevel   = Random.Range(1, 9);
                    paths[i].rewardAntType = (AntType)Random.Range(0, System.Enum.GetValues(typeof(AntType)).Length);
                    break;
                
                case PathType.Enemy:
                    paths[i].enemyLevel    = phase < 5 ? 1 : Random.Range(1, 9);
                    paths[i].rewardAntType = (AntType)Random.Range(0, System.Enum.GetValues(typeof(AntType)).Length);
                    break;

                case PathType.Trap:
                    // Avant phase 5 : pas de piège (on met un chemin objet à la place)
                    if (phase < 5)
                    {
                        paths[i].type = PathType.Object;
                        paths[i].rewardLevel = Random.Range(1, 4); // récompense modeste
                    }
                    else
                    {
                        paths[i].trapX = Random.Range(1, 4);
                    }
                    break;
            }
        }
    }

    public void AssignInsect(int pathIndex, GridObject insect)
    {
        if (pathIndex < 0 || pathIndex > 2) return;
        if (!paths[pathIndex].assignedInsects.Contains(insect))
            paths[pathIndex].assignedInsects.Add(insect);
    }

    public void UnassignInsect(int pathIndex, GridObject insect) =>
        paths[pathIndex].assignedInsects.Remove(insect);

    public void ClearAllAssignments()
    {
        foreach (var p in paths)
        {
            p.assignedInsects.Clear();
            p.survivors.Clear();
            p.rewards.Clear();
            p.resultLog = "";
        }
    }
}