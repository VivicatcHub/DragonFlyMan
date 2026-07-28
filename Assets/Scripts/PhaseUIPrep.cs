using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PhaseUIPrep : MonoBehaviour
{
    public static PhaseUIPrep Instance;

    public TextMeshProUGUI phaseLabel;
    public TextMeshProUGUI messageText;
    public Button          sendButton;

    void Awake()
    {
        Instance = this;
        sendButton.onClick.AddListener(() => GameManagerPrep.Instance.OnSendExpeditions());
    }

    public void UpdatePhase(int phase)
    {
        if (phaseLabel) phaseLabel.text = $"Phase {phase}";
    }

    public void ShowMessage(string msg)
    {
        if (messageText) messageText.text = msg;
    }
}