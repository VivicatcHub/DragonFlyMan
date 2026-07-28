using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Text;

public class PhaseUIResults : MonoBehaviour
{
    public static PhaseUIResults Instance;

    public TextMeshProUGUI phaseLabel;
    public TextMeshProUGUI resultsText;
    public Button          nextPhaseButton;

    void Awake()
    {
        Instance = this;
        nextPhaseButton.onClick.AddListener(() => GameManagerResults.Instance.OnNextPhase());
    }

    public void UpdatePhase(int phase)
    {
        if (phaseLabel) phaseLabel.text = $"Phase {phase} — Résultats";
    }

    public void ShowResults(PathResult[] results)
    {
        if (resultsText == null || results == null) return;

        var sb    = new StringBuilder();
        string[] names = { "Chemin 1", "Chemin 2", "Chemin 3" };

        for (int i = 0; i < results.Length; i++)
        {
            if (results[i] == null)
            {
                sb.AppendLine($"<b>{names[i]}</b> : aucun insecte envoyé.\n");
                continue;
            }
            sb.AppendLine($"<b>{names[i]}</b> [{results[i].type}]");
            sb.AppendLine(results[i].resultLog != "" ? results[i].resultLog : "Aucun insecte envoyé.");
            sb.AppendLine($"  Survivants : {results[i].survivors.Count}");
            sb.AppendLine();
        }

        resultsText.text = sb.ToString();
    }

    public void ShowMessage(string msg)
    {
        if (nextPhaseButton) nextPhaseButton.GetComponentInChildren<TextMeshProUGUI>().text = msg;
    }
}