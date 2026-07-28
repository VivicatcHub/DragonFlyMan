using UnityEngine;
using System;
using System.Collections.Generic;

public class ExpeditionManager : MonoBehaviour
{
    public static ExpeditionManager Instance;

    // Tableau indexé 0→7 correspondant aux niveaux 1→8
    public GameObject[] antPrefabs; // index 0=niv1 ... 7=niv8
    public GameObject[] dragonFlyPrefabs; // index 0=niv1 ...

    void Awake() => Instance = this;

    public void ResolveAll(Action onComplete)
    {
        foreach (var path in PathManager.Instance.paths)
            ResolvePath(path);
        onComplete?.Invoke();
    }

    void ResolvePath(PathData path)
    {
        path.survivors.Clear();
        path.rewards.Clear();

        switch (path.type)
        {
            case PathType.Object: ResolveObject(path); break;
            case PathType.Enemy:  ResolveEnemy(path);  break;
            case PathType.Trap:   ResolveTrap(path);   break;
        }
    }

    void ResolveObject(PathData path)
    {
        // Nos insectes reviennent toujours
        path.survivors.AddRange(path.assignedInsects);

        int totalPower = 0;
        foreach (var ins in path.assignedInsects) totalPower += ins.level;

        int diff = path.rewardLevel - totalPower;
        float chance = diff <= 0 ? 1f : Mathf.Pow(0.5f, diff);

        if (UnityEngine.Random.value <= chance)
        {
            path.resultLog = $"Objet niv.{path.rewardLevel} gagné ! (chance: {Mathf.RoundToInt(chance*100)}%)";
            path.rewards.Add(SpawnInsect(path.rewardLevel, AntType.Ant)); // par défaut, récompense en fourmi ouvrière
        }
        else
        {
            path.resultLog = $"Objet niv.{path.rewardLevel} manqué. (chance: {Mathf.RoundToInt(chance*100)}%)";
        }
    }

    void ResolveEnemy(PathData path)
    {
        var playerTeam = new List<GridObject>(path.assignedInsects);
        playerTeam.Sort((a, b) => b.level.CompareTo(a.level));

        int enemyHP = path.enemyLevel;
        var playerHP = new Dictionary<GridObject, int>();
        foreach (var ins in playerTeam) playerHP[ins] = ins.level;

        // Nos insectes attaquent d'abord
        foreach (var ins in playerTeam)
        {
            if (enemyHP <= 0) break;
            enemyHP -= ins.level;
        }

        // L'ennemi riposte s'il est encore en vie
        if (enemyHP > 0)
        {
            int dmgLeft = path.enemyLevel;
            var alive = new List<GridObject>(playerTeam);
            while (dmgLeft > 0 && alive.Count > 0)
            {
                var target = alive[UnityEngine.Random.Range(0, alive.Count)];
                playerHP[target]--;
                dmgLeft--;
                if (playerHP[target] <= 0) alive.Remove(target);
            }
        }

        // Tous reviennent blessés (niveau = PV restants, min 1)
        foreach (var ins in playerTeam)
        {
            ins.level = Mathf.Max(1, playerHP[ins]);
            path.survivors.Add(ins);
        }

        if (enemyHP <= 0)
        {
            path.enemyDefeated = true;
            path.resultLog = $"Ennemi niv.{path.enemyLevel} vaincu ! Il rejoint ta grille.";
            path.rewards.Add(SpawnInsect(path.enemyLevel));
        }
        else
        {
            path.resultLog = $"Ennemi niv.{path.enemyLevel} survit ({enemyHP} PV restants).";
        }
    }

    void ResolveTrap(PathData path)
    {
        int totalLevel = 0;
        foreach (var ins in path.assignedInsects) totalLevel += ins.level;

        int divisor = Mathf.Max(1, 9 - totalLevel);
        float chance = Mathf.Clamp01(100f / divisor * path.trapX / 100f);

        int indemnes = 0;
        foreach (var ins in path.assignedInsects)
        {
            if (UnityEngine.Random.value <= chance)
                indemnes++;
            else
                ins.level = Mathf.Max(1, ins.level - 1);  // blessé
            path.survivors.Add(ins);
        }
        path.resultLog = $"Piège (x={path.trapX}) : {Mathf.RoundToInt(chance*100)}% chance. {indemnes}/{path.assignedInsects.Count} indemnes.";
    }

    // Crée un insecte hors-scène (pas encore placé dans la grille)
    GridObject SpawnInsect(int level, AntType antType = AntType.Ant)
    {
        var prefabs = antType == AntType.Ant ? antPrefabs : dragonFlyPrefabs;
        int idx = Mathf.Clamp(level - 1, 0, prefabs.Length - 1);
        var go = Instantiate(prefabs[idx], Vector3.zero, Quaternion.identity);
        var obj = go.GetComponent<GridObject>();
        obj.level = level;
        obj.antType = antType;
        // Désactive le DragObject pour éviter qu'il se snape tout seul dans Start()
        var drag = go.GetComponent<DragObject>();
        if (drag) drag.enabled = false;
        return obj;
    }
}