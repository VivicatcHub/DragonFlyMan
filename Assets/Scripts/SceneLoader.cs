using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance;
    void Awake() => Instance = this;

    // Appelé quand le joueur clique "Envoyer"
    // Sauvegarde l'état courant dans GameData puis charge SceneResults
    public void GoToResults()
    {
        SaveMainGridToGameData();
        ResolveExpeditions();
        SceneManager.LoadScene("SceneResults");
    }

    // Appelé depuis SceneResults quand le joueur clique "Phase suivante"
    public void GoToPreparation()
    {
        GameData.Instance.currentPhase++;
        SceneManager.LoadScene("ScenePreparation");
    }

    // ─── SAUVEGARDE DE LA GRILLE PRINCIPALE ─────────────────────

    void SaveMainGridToGameData()
    {
        GameData.Instance.mainGridInsects.Clear();

        // Sauvegarde tous les objets de la grille principale
        var gm = GridManager.Instance;
        for (int r = 0; r < gm.rows; r++)
            for (int c = 0; c < gm.columns; c++)
            {
                var cell = new Vector2Int(c, r);
                var obj = gm.GetObjectAt(cell);
                if (obj != null)
                    GameData.Instance.mainGridInsects.Add(new InsectData
                    {
                        level = obj.level, cellX = cell.x, cellY = cell.y
                    });
            }
    }

    // ─── RÉSOLUTION DES EXPÉDITIONS ─────────────────────────────

    void ResolveExpeditions()
    {
        var paths = PathManager.Instance.paths;
        GameData.Instance.pathResults = new PathResult[3];

        for (int i = 0; i < 3; i++)
        {
            var path = paths[i];
            var result = new PathResult
            {
                type        = path.type,
                rewardLevel = path.rewardLevel,
                enemyLevel  = path.enemyLevel,
                trapX       = path.trapX
            };

            // Sauvegarde les insectes envoyés avec leur cellule dans le chemin
            var sentInsects = new List<InsectData>();
            foreach (var ins in path.assignedInsects)
                sentInsects.Add(new InsectData
                {
                    level = ins.level,
                    cellX = ins.currentCell.x,
                    cellY = ins.currentCell.y
                });

            ResolvePathIntoResult(path, sentInsects, result);
            GameData.Instance.pathResults[i] = result;
        }
    }

    void ResolvePathIntoResult(PathData path, List<InsectData> sent, PathResult result)
    {
        switch (path.type)
        {
            case PathType.Object: ResolveObject(path, sent, result); break;
            case PathType.Enemy:  ResolveEnemy(path, sent, result);  break;
            case PathType.Trap:   ResolveTrap(path, sent, result);   break;
        }
    }

    void ResolveObject(PathData path, List<InsectData> sent, PathResult result)
    {
        result.survivors.AddRange(sent);  // tous reviennent

        int totalPower = 0;
        foreach (var ins in sent) totalPower += ins.level;

        int diff = path.rewardLevel - totalPower;
        float chance = diff <= 0 ? 1f : Mathf.Pow(0.5f, diff);

        if (Random.value <= chance)
        {
            result.resultLog = $"Objet niv.{path.rewardLevel} gagné ! ({Mathf.RoundToInt(chance*100)}%)";
            result.rewards.Add(new InsectData { level = path.rewardLevel, cellX = -1, cellY = 3 });
        }
        else
        {
            result.resultLog = $"Objet niv.{path.rewardLevel} manqué. ({Mathf.RoundToInt(chance*100)}%)";
        }
    }

    void ResolveEnemy(PathData path, List<InsectData> sent, PathResult result)
    {
        sent.Sort((a, b) => b.level.CompareTo(a.level));

        int enemyHP = path.enemyLevel;
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
            var alive = new List<InsectData>(sent);
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
            result.resultLog = $"Ennemi niv.{path.enemyLevel} vaincu ! Il rejoint ta grille.";
            result.rewards.Add(new InsectData { level = path.enemyLevel, cellX = -1, cellY = 3 });
        }
        else
        {
            result.resultLog = $"Ennemi niv.{path.enemyLevel} survit ({enemyHP} PV restants).";
        }
    }

    void ResolveTrap(PathData path, List<InsectData> sent, PathResult result)
    {
        int totalLevel = 0;
        foreach (var ins in sent) totalLevel += ins.level;

        int divisor = Mathf.Max(1, 9 - totalLevel);
        float chance = Mathf.Clamp01(100f / divisor * path.trapX / 100f);

        int indemnes = 0;
        foreach (var ins in sent)
        {
            if (Random.value <= chance) indemnes++;
            else ins.level = Mathf.Max(1, ins.level - 1);
            result.survivors.Add(ins);
        }
        result.resultLog = $"Piège (x={path.trapX}) : {Mathf.RoundToInt(chance*100)}% chance. {indemnes}/{sent.Count} indemnes.";
    }
}