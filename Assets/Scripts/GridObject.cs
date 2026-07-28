using UnityEngine;

public enum AntType { Ant, DragonFly }  // ajoute autant de types que tu veux

public class GridObject : MonoBehaviour
{
    public int level = 1;
    public AntType antType = AntType.Ant;
    public Vector2Int currentCell;

    public void Init(Vector2Int cell)
    {
        currentCell = cell;
        transform.position = GridManager.Instance.GetWorldPosition(cell);
    }
}