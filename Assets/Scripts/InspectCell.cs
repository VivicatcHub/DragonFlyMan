using UnityEngine;
using TMPro;

public class InspectCell : MonoBehaviour
{
    public static InspectCell Instance;

    public TextMeshPro infoText; // TextMeshPro monde (pas UI)
    public Vector2 cellPosition; // position monde de la case

    private GridObject currentInsect;

    void Awake() => Instance = this;

    void Update()
    {
        // Vérifie en permanence si un insecte est dans la case
        Vector2Int cell = GridManager.Instance.GetCellCoords(cellPosition);
        GridObject obj  = GridManager.Instance.GetObjectAt(cell);

        if (obj != currentInsect)
        {
            currentInsect = obj;
            UpdateDisplay();
        }
    }

    void UpdateDisplay()
    {
        if (currentInsect == null)
        {
            infoText.text = "";
            return;
        }

        string typeName = currentInsect.antType.ToString();
        infoText.text   = $"{typeName}\nLevel {currentInsect.level}";
    }
}