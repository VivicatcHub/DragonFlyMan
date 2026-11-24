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
        if (phaseLabel) phaseLabel.text = $"Phase {phase} — Results";
    }

    public void ShowResults(PathResult[] results)
    {
        if (resultsText == null || results == null) return;

        var sb    = new StringBuilder();
        string[] names = { "Path 1", "Path 2", "Path 3" };

        for (int i = 0; i < results.Length; i++)
        {
            if (results[i] == null)
            {
                sb.AppendLine($"<b>{names[i]}</b>: no insect sent.\n");
                continue;
            }
            sb.AppendLine($"<b>{names[i]}</b> [{results[i].type}]");
            sb.AppendLine(results[i].resultLog != "" ? results[i].resultLog : "No insect sent.");
            sb.AppendLine($"  Survivors: {results[i].survivors.Count}");
            sb.AppendLine();
        }

        resultsText.text = sb.ToString();
    }

    public void ShowMessage(string msg)
    {
        if (nextPhaseButton) nextPhaseButton.GetComponentInChildren<TextMeshProUGUI>().text = msg;
    }
}