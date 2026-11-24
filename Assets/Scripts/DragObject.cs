using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(GridObject))]
public class DragObject : MonoBehaviour
{
    public GameObject mergedPrefab;

    private GridObject gridObject;
    private Camera mainCam;
    private bool isDragging;
    private Vector2Int originalCell;
    private Vector3 dragOffset;
    public float dragZ = -1f;

    void Awake()
    {
        gridObject = GetComponent<GridObject>();
        mainCam = Camera.main;
    }

    void Start()
    {
        // Ne fait le snap que si l'objet n'est pas déjà enregistré
        if (GridManager.Instance.GetObjectAt(gridObject.currentCell) != this.gridObject)
        {
            Vector2Int cell = GridManager.Instance.GetCellCoords(transform.position);
            gridObject.currentCell = cell;
            GridManager.Instance.RegisterObject(gridObject, cell);
            transform.position = GridManager.Instance.GetWorldPosition(cell);
        }
    }

    void OnMouseDown()
    {
        isDragging = true;
        originalCell = gridObject.currentCell;

        int prevPath = GridManager.Instance.PathIndexOf(originalCell);
        if (prevPath >= 0)
            PathManager.Instance.UnassignInsect(prevPath, gridObject);

        GridManager.Instance.FreeCell(originalCell);
        dragOffset = transform.position - GetMouseWorldPos();
    }

    void OnMouseDrag()
    {
        if (!isDragging) return;
        Vector3 pos = GetMouseWorldPos() + dragOffset;
        pos.z = dragZ;
        transform.position = pos;
    }

    void OnMouseUp()
    {
        if (!isDragging) return;
        isDragging = false;

        Vector2Int targetCell = GridManager.Instance.GetCellCoords(transform.position);

        if (!GridManager.Instance.IsValidCell(targetCell))
        {
            ReturnToOrigin(); return;
        }

        if (GridManager.Instance.IsCellFree(targetCell))
        {
            GridManager.Instance.PlaceObject(gridObject, targetCell);

            // Si déposé dans un chemin, enregistrer dans PathManager
            int pathIdx = GridManager.Instance.PathIndexOf(targetCell);
            if (pathIdx >= 0)
                PathManager.Instance.AssignInsect(pathIdx, gridObject);
            return;
        }

        // Tentative de merge (uniquement en grille principale)
        GridObject other = GridManager.Instance.GetObjectAt(targetCell);
        if (other != null
            && other.level   == gridObject.level
            && other.antType == gridObject.antType
            && other.level  < 8
            && !GridManager.Instance.IsPathCell(targetCell))
        {
            DoMerge(other, targetCell);
        }
        else
        {
            ReturnToOrigin();
        }
    }

    void ReturnToOrigin()
    {
        GridManager.Instance.PlaceObject(gridObject, originalCell);
        int pathIdx = GridManager.Instance.PathIndexOf(originalCell);
        if (pathIdx >= 0)
            PathManager.Instance.AssignInsect(pathIdx, gridObject);
    }

    void DoMerge(GridObject other, Vector2Int targetCell)
    {
        int newLevel = gridObject.level + 1;
        Vector3 spawnPos = GridManager.Instance.GetWorldPosition(targetCell);

        GridManager.Instance.FreeCell(originalCell);
        GridManager.Instance.FreeCell(targetCell);
        Destroy(other.gameObject);
        Destroy(gameObject);

        if (mergedPrefab != null)
        {
            GameObject newObj = Instantiate(mergedPrefab, spawnPos, Quaternion.identity);
            GridObject newGridObj = newObj.GetComponent<GridObject>();
            newGridObj.level = newLevel;
            GridManager.Instance.RegisterObject(newGridObj, targetCell);
        }
    }

    Vector3 GetMouseWorldPos()
    {
        Vector2 mouseScreen = UnityEngine.InputSystem.Mouse.current.position.ReadValue();
        return mainCam.ScreenToWorldPoint(
            new Vector3(mouseScreen.x, mouseScreen.y, Mathf.Abs(mainCam.transform.position.z))
        );
    }
}