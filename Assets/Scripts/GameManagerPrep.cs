using UnityEngine;
using System.Collections.Generic;

public class GameManagerPrep : MonoBehaviour
{
    public static GameManagerPrep Instance;

    public GameObject[] antPrefabs; // index 0=niv1 ... 7=niv8
    public GameObject[] dragonFlyPrefabs; // index 0=niv1 ... 7=niv8

    void Awake() => Instance = this;

    void Start()
    {
        PathManager.Instance.GeneratePaths();
        PathManager.Instance.ClearAllAssignments();

        // Recharge les insectes sauvegardés de la phase précédente
        foreach (var data in GameData.Instance.mainGridInsects)
            SpawnInsect(data.level, new Vector2Int(data.cellX, data.cellY), data.antType);

        // Nouvelle fourmi de début de phase
        SpawnStartingAnt();

        PhaseUIPrep.Instance.UpdatePhase(GameData.Instance.currentPhase);
    }

    void SpawnStartingAnt()
    {
        var cell = GridManager.Instance.FindFreeCell();
        if (cell == null) return;
        SpawnInsect(1, cell.Value);
    }

    public void SpawnInsect(int level, Vector2Int cell, AntType antType = AntType.Ant)
    {
        if (!GridManager.Instance.IsValidCell(cell)) return;
        if (!GridManager.Instance.IsCellFree(cell))  return;

        var prefabs = antType == AntType.Ant ? antPrefabs : dragonFlyPrefabs;
        int idx     = Mathf.Clamp(level - 1, 0, prefabs.Length - 1);
        var go      = Instantiate(prefabs[idx], GridManager.Instance.GetWorldPosition(cell),
                    Quaternion.identity);

        var obj = go.GetComponent<GridObject>();
        obj.level       = level;
        obj.antType     = antType;
        obj.currentCell = cell;
        GridManager.Instance.RegisterObject(obj, cell);
    }

    // ─── BOUTON ENVOYER ──────────────────────────────────────────

    public void OnSendExpeditions()
    {
        // Vérifie qu'aucun insecte n'est dans la case d'inspection
        Vector2Int inspectCell = GridManager.InspectCellCoords;
        if (GridManager.Instance.GetObjectAt(inspectCell) != null)
        {
            PhaseUIPrep.Instance.ShowMessage("Remove the insect from the inspection cell before sending!");
            return;
        }

        // Vérifie qu'au moins un insecte est assigné
        bool anyAssigned = false;
        foreach (var path in PathManager.Instance.paths)
            if (path.assignedInsects.Count > 0) { anyAssigned = true; break; }

        if (!anyAssigned)
        {
            PhaseUIPrep.Instance.ShowMessage("Assign at least one insect to a path!");
            return;
        }

        SaveMainGrid();
        ResolveAllPaths();
        UnityEngine.SceneManagement.SceneManager.LoadScene("SceneResults");
    }

    void SaveMainGrid()
    {
        GameData.Instance.mainGridInsects.Clear();
        var gm = GridManager.Instance;
        for (int r = 0; r < gm.rows; r++)
            for (int c = 0; c < gm.columns; c++)
            {
                var cell = new Vector2Int(c, r);
                var obj  = gm.GetObjectAt(cell);
                if (obj != null)
                    GameData.Instance.mainGridInsects.Add(
                        new InsectData { level = obj.level, cellX = c, cellY = r, antType = obj.antType });
            }
    }

    // ─── RÉSOLUTION ──────────────────────────────────────────────

    void ResolveAllPaths()
    {
        GameData.Instance.pathResults = new PathResult[3];
        var paths = PathManager.Instance.paths;

        for (int i = 0; i < 3; i++)
        {
            var path   = paths[i];
            var result = new PathResult
            {
                type        = path.type,
                rewardLevel = path.rewardLevel,
                enemyLevel  = path.enemyLevel,
                trapX       = path.trapX
            };

            var sent = new List<InsectData>();
            foreach (var ins in path.assignedInsects)
                sent.Add(new InsectData
                {
                    level = ins.level,
                    cellX = ins.currentCell.x,
                    cellY = ins.currentCell.y,
                    antType = ins.antType
                });

            switch (path.type)
            {
                case PathType.Object: ResolveObject(path, sent, result); break;
                case PathType.Enemy:  ResolveEnemy (path, sent, result); break;
                case PathType.Trap:   ResolveTrap  (path, sent, result); break;
            }

            GameData.Instance.pathResults[i] = result;
        }
    }

    void ResolveObject(PathData path, List<InsectData> sent, PathResult result)
    {
        result.survivors.AddRange(sent);

        int totalPower = 0;
        foreach (var ins in sent) totalPower += ins.level;

        int   diff   = path.rewardLevel - totalPower;
        float chance = diff <= 0 ? 1f : Mathf.Pow(0.5f, diff);

        if (Random.value <= chance)
        {
            result.resultLog = $"Level {path.rewardLevel} object won! ({Mathf.RoundToInt(chance * 100)}%)";
            result.rewards.Add(new InsectData { level = path.rewardLevel, antType = path.rewardAntType });
        }
        else
        {
            result.resultLog = $"Level {path.rewardLevel} object missed. ({Mathf.RoundToInt(chance * 100)}%)";
        }
    }

    void ResolveEnemy(PathData path, List<InsectData> sent, PathResult result)
    {
        sent.Sort((a, b) => b.level.CompareTo(a.level));

        int enemyHP  = path.enemyLevel;
        var playerHP = new Dictionary<InsectData, int>();
        foreach (var ins in sent) playerHP[ins] = ins.level;

        foreach (var ins in sent)
        {
            if (enemyHP <= 0) break;
            enemyHP -= ins.level;
        }

        if (enemyHP > 0)
        {
            int dmgLeft = path.enemyLevel;
            var alive   = new List<InsectData>(sent);
            while (dmgLeft > 0 && alive.Count > 0)
            {
                var target = alive[Random.Range(0, alive.Count)];
                playerHP[target]--;
                dmgLeft--;
                if (playerHP[target] <= 0) alive.Remove(target);
            }
        }

        foreach (var ins in sent)
        {
            ins.level = Mathf.Max(1, playerHP[ins]);
            result.survivors.Add(ins);
        }

        if (enemyHP <= 0)
        {
            result.enemyDefeated = true;
            result.resultLog     = $"Level {path.enemyLevel} enemy defeated! It joins your grid.";
            result.rewards.Add(new InsectData { level = path.enemyLevel, antType = path.rewardAntType });
        }
        else
        {
            result.resultLog = $"Level {path.enemyLevel} enemy survived ({enemyHP} HP left).";
        }
    }

    void ResolveTrap(PathData path, List<InsectData> sent, PathResult result)
    {
        int totalLevel = 0;
        foreach (var ins in sent) totalLevel += ins.level;

        int   divisor = Mathf.Max(1, 9 - totalLevel);
        float chance  = Mathf.Clamp01(100f / divisor * path.trapX / 100f);

        int indemnes = 0;
        foreach (var ins in sent)
        {
            if (Random.value <= chance) indemnes++;
            else ins.level = Mathf.Max(1, ins.level - 1);
            result.survivors.Add(ins);
        }
        result.resultLog = $"Trap (x={path.trapX}): {Mathf.RoundToInt(chance * 100)}% chance. {indemnes}/{sent.Count} unharmed.";
    }
}