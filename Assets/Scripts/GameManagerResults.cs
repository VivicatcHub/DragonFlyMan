using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class GameManagerResults : MonoBehaviour
{
    public static GameManagerResults Instance;

    public GameObject[] antPrefabs; // index 0=niv1 ... 7=niv8
    public GameObject[] dragonFlyPrefabs; // index 0=niv1 ... 7=niv8

    void Awake() => Instance = this;

    void Start()
    {
        PhaseUIResults.Instance.UpdatePhase(GameData.Instance.currentPhase);
        PhaseUIResults.Instance.ShowResults(GameData.Instance.pathResults);

        // Recharge la grille principale
        foreach (var data in GameData.Instance.mainGridInsects)
            SpawnInsect(data.level, new Vector2Int(data.cellX, data.cellY), data.antType);

        // Place les survivants dans leurs cases chemin
        for (int i = 0; i < 3; i++)
        {
            var result = GameData.Instance.pathResults[i];
            if (result == null) continue;

            foreach (var ins in result.survivors)
                SpawnInsect(ins.level, new Vector2Int(ins.cellX, ins.cellY), ins.antType);

            // Récompenses en case 3
            foreach (var reward in result.rewards)
            {
                var rewardCell = GridManager.Instance.GetRewardCell(i);
                SpawnInsect(reward.level, rewardCell, reward.antType);
            }
        }
    }

    void SpawnInsect(int level, Vector2Int cell, AntType antType = AntType.Ant)
    {
        if (!GridManager.Instance.IsValidCellForResults(cell)) return;
        if (!GridManager.Instance.IsCellFreeForResults(cell))  return;

        var prefabs = antType == AntType.Ant ? antPrefabs : dragonFlyPrefabs;
        int idx     = Mathf.Clamp(level - 1, 0, prefabs.Length - 1);
        var go      = Instantiate(prefabs[idx], GridManager.Instance.GetWorldPosition(cell),
                    Quaternion.identity);

        var obj = go.GetComponent<GridObject>();
        obj.level       = level;
        obj.antType     = antType;
        obj.currentCell = cell;
        GridManager.Instance.RegisterObjectForResults(obj, cell);

        var drag = go.GetComponent<DragObject>();
        if (drag)
            drag.enabled = !GridManager.Instance.IsPathCell(cell);
    }

    // ─── BOUTON PHASE SUIVANTE ───────────────────────────────────

    public void OnNextPhase()
    {
        // Vérifie que tous les chemins sont vides
        for (int i = 0; i < 3; i++)
        {
            var pathObjs = GridManager.Instance.GetObjectsInPath(i);
            if (pathObjs.Count > 0)
            {
                PhaseUIResults.Instance.ShowMessage(
                    "Collect all your insects from the paths before continuing!");
                return;
            }
        }

        CollectToMainGrid();
        GameData.Instance.currentPhase++;
        UnityEngine.SceneManagement.SceneManager.LoadScene("ScenePreparation");
    }

    void CollectToMainGrid()
    {
        GameData.Instance.mainGridInsects.Clear();
        var gm = GridManager.Instance;

        // Insectes déjà en grille principale
        var occupied = new HashSet<Vector2Int>();
        for (int r = 0; r < gm.rows; r++)
            for (int c = 0; c < gm.columns; c++)
            {
                var cell = new Vector2Int(c, r);
                var obj  = gm.GetObjectAt(cell);
                if (obj != null)
                {
                    GameData.Instance.mainGridInsects.Add(
                        new InsectData { level = obj.level, cellX = c, cellY = r, antType = obj.antType });
                    occupied.Add(cell);
                }
            }

        // Survivants + récompenses des chemins → cherche une case libre
        for (int i = 0; i < 3; i++)
        {
            var pathObjs = gm.GetObjectsInPath(i);
            foreach (var obj in pathObjs)
            {
                bool placed = false;
                for (int r = 0; r < gm.rows && !placed; r++)
                    for (int c = 0; c < gm.columns && !placed; c++)
                    {
                        var cell = new Vector2Int(c, r);
                        if (!occupied.Contains(cell))
                        {
                            GameData.Instance.mainGridInsects.Add(
                                new InsectData { level = obj.level, cellX = c, cellY = r, antType = obj.antType });
                            occupied.Add(cell);
                            placed = true;
                        }
                    }

                if (!placed)
                    Debug.LogWarning($"Grid full, level {obj.level} insect lost.");
            }
        }
    }
}