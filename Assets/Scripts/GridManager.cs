using UnityEngine;
using System.Collections.Generic;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance;

    [Header("Grille principale")]
    public int columns = 3;
    public int rows = 3;
    public float cellSize = 1.2f;
    public Vector2 gridOrigin = Vector2.zero;

    [Header("Grilles de chemin (1x4, au-dessus)")]
    public Vector2 path0Origin;
    public Vector2 path1Origin;
    public Vector2 path2Origin;
    // La case d'index 3 (row=3) est la case d'arrivée récompense

    private readonly Vector2Int path0Offset = new(100, 0);
    private readonly Vector2Int path1Offset = new(200, 0);
    private readonly Vector2Int path2Offset = new(300, 0);

    public int pathRows = 4;
    public bool isResultsScene = false; // coché dans SceneResults

    public Dictionary<Vector2Int, GridObject> allCells = new();

    [Header("Case d'inspection")]
    public Vector2 inspectCellOrigin;
    public static readonly Vector2Int InspectCellCoords = new(400, 0);

    void Awake() => Instance = this;

    // ─── COORDONNÉES ────────────────────────────────────────────

    public Vector2Int GetCellCoords(Vector3 worldPos)
    {
        float dMain    = DistToRegion(worldPos, gridOrigin,       columns, rows);
        float dP0      = DistToRegion(worldPos, path0Origin,      1, pathRows);
        float dP1      = DistToRegion(worldPos, path1Origin,      1, pathRows);
        float dP2      = DistToRegion(worldPos, path2Origin,      1, pathRows);
        float dInspect = DistToRegion(worldPos, inspectCellOrigin,1, 1);
        float minD     = Mathf.Min(dMain, dP0, dP1, dP2, dInspect);

        if (minD == dP0)      return WorldToCell(worldPos, path0Origin,       path0Offset);
        if (minD == dP1)      return WorldToCell(worldPos, path1Origin,       path1Offset);
        if (minD == dP2)      return WorldToCell(worldPos, path2Origin,       path2Offset);
        if (minD == dInspect) return WorldToCell(worldPos, inspectCellOrigin, InspectCellCoords);
        return WorldToCell(worldPos, gridOrigin, Vector2Int.zero);
    }

    Vector2Int WorldToCell(Vector3 wp, Vector2 origin, Vector2Int offset)
    {
        int col = Mathf.RoundToInt((wp.x - origin.x) / cellSize);
        int row = Mathf.RoundToInt((wp.y - origin.y) / cellSize);
        return new Vector2Int(col + offset.x, row + offset.y);
    }

    float DistToRegion(Vector3 wp, Vector2 origin, int cols, int rows_)
    {
        float cx = origin.x + (cols - 1) * cellSize * 0.5f;
        float cy = origin.y + (rows_ - 1) * cellSize * 0.5f;
        return Vector2.Distance(new Vector2(wp.x, wp.y), new Vector2(cx, cy));
    }

    public Vector3 GetWorldPosition(Vector2Int cell)
    {
        if (cell.x == InspectCellCoords.x)
            return new Vector3(inspectCellOrigin.x, inspectCellOrigin.y, 0f);

        Vector2    origin = OriginFor(cell);
        Vector2Int offset = OffsetFor(cell);
        Vector2Int local  = cell - offset;
        return new Vector3(origin.x + local.x * cellSize, origin.y + local.y * cellSize, 0f);
    }

    Vector2 OriginFor(Vector2Int cell)
    {
        if (cell.x >= 300) return path2Origin;
        if (cell.x >= 200) return path1Origin;
        if (cell.x >= 100) return path0Origin;
        return gridOrigin;
    }

    Vector2Int OffsetFor(Vector2Int cell)
    {
        if (cell.x >= 300) return path2Offset;
        if (cell.x >= 200) return path1Offset;
        if (cell.x >= 100) return path0Offset;
        return Vector2Int.zero;
    }

    // ─── VALIDITÉ ET BLOCAGE ────────────────────────────────────

    public bool IsValidCell(Vector2Int cell)
    {
        // Case d'inspection
        if (cell == InspectCellCoords) return true;

        // Grilles chemin
        if (cell.x >= 100)
        {
            int localX = cell.x % 100;
            if (localX != 0) return false;
            if (cell.y < 0 || cell.y >= pathRows) return false;
            if (cell.y == 3) return isResultsScene;
            return !isResultsScene;
        }

        // Grille principale
        return cell.x >= 0 && cell.x < columns && cell.y >= 0 && cell.y < rows;
    }

    public bool IsPathCell(Vector2Int cell) => cell.x >= 100;

    public int PathIndexOf(Vector2Int cell)
    {
        if (cell.x >= 300) return 2;
        if (cell.x >= 200) return 1;
        if (cell.x >= 100) return 0;
        return -1;
    }

    // ─── API GRILLE ──────────────────────────────────────────────

    public bool IsCellFree(Vector2Int cell) =>
        IsValidCell(cell) && !allCells.ContainsKey(cell);

    public GridObject GetObjectAt(Vector2Int cell) =>
        allCells.TryGetValue(cell, out var o) ? o : null;

    public void PlaceObject(GridObject obj, Vector2Int cell)
    {
        if (!IsValidCell(cell)) return;
        allCells[cell] = obj;
        obj.currentCell = cell;
        obj.transform.position = GetWorldPosition(cell);
    }

    public void FreeCell(Vector2Int cell) => allCells.Remove(cell);

    public void RegisterObject(GridObject obj, Vector2Int cell)
    {
        if (IsValidCell(cell)) allCells[cell] = obj;
    }

    public Vector2Int? FindFreeCell()
    {
        for (int r = 0; r < rows; r++)
            for (int c = 0; c < columns; c++)
            {
                var cell = new Vector2Int(c, r);
                if (IsCellFree(cell)) return cell;
            }
        return null;
    }

    // Case récompense d'un chemin (row 3)
    public Vector2Int GetRewardCell(int pathIndex)
    {
        Vector2Int offset = pathIndex == 0 ? path0Offset :
                            pathIndex == 1 ? path1Offset : path2Offset;
        return new Vector2Int(offset.x, 3);
    }

    public List<GridObject> GetObjectsInPath(int pathIndex)
    {
        Vector2Int offset = pathIndex == 0 ? path0Offset :
                            pathIndex == 1 ? path1Offset : path2Offset;
        var result = new List<GridObject>();
        for (int r = 0; r < pathRows; r++)
        {
            var cell = new Vector2Int(offset.x, r);
            if (allCells.TryGetValue(cell, out var obj))
                result.Add(obj);
        }
        return result;
    }

    // ─── GIZMOS ──────────────────────────────────────────────────

    void OnDrawGizmos()
    {
        DrawRegion(gridOrigin, columns, rows, new Color(1, 1, 1, 0.3f));
        DrawPathRegion(path0Origin);
        DrawPathRegion(path1Origin);
        DrawPathRegion(path2Origin);

        // Case d'inspection en jaune
        Gizmos.color = new Color(1f, 0.9f, 0.1f, 0.5f);
        var center = new Vector3(inspectCellOrigin.x, inspectCellOrigin.y, 0);
        Gizmos.DrawWireCube(center, Vector3.one * cellSize * 0.95f);
    }

    void DrawRegion(Vector2 origin, int cols, int rows_, Color color)
    {
        Gizmos.color = color;
        for (int c = 0; c < cols; c++)
            for (int r = 0; r < rows_; r++)
            {
                var center = new Vector3(origin.x + c * cellSize, origin.y + r * cellSize, 0);
                Gizmos.DrawWireCube(center, Vector3.one * cellSize * 0.95f);
            }
    }

    void DrawPathRegion(Vector2 origin)
    {
        for (int r = 0; r < pathRows; r++)
        {
            // Cases 0-2 en bleu, case 3 (récompense) en jaune
            Gizmos.color = r < 3 ? new Color(0.3f, 0.8f, 1f, 0.4f)
                                 : new Color(1f, 0.9f, 0.2f, 0.4f);
            var center = new Vector3(origin.x, origin.y + r * cellSize, 0);
            Gizmos.DrawWireCube(center, Vector3.one * cellSize * 0.95f);
        }
    }

    // Version sans restriction de phase — pour SceneResults
    public bool IsValidCellForResults(Vector2Int cell)
    {
        if (cell.x >= 100)
        {
            int localX = cell.x % 100;
            return localX == 0 && cell.y >= 0 && cell.y < pathRows;
        }
        return cell.x >= 0 && cell.x < columns && cell.y >= 0 && cell.y < rows;
    }

    public bool IsCellFreeForResults(Vector2Int cell) =>
        IsValidCellForResults(cell) && !allCells.ContainsKey(cell);

    public void RegisterObjectForResults(GridObject obj, Vector2Int cell)
    {
        allCells[cell] = obj;
        obj.currentCell = cell;
        obj.transform.position = GetWorldPosition(cell);
    }
}