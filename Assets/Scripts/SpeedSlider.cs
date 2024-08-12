

using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class SpeedSlider : MonoBehaviour
{
    private Slider slider;
    [SerializeField]
    private Board board;
    [SerializeField]
    private PuzzleManager puzzleManager;
    [SerializeField]
    private TextMeshProUGUI speedText;

    void Awake() {
        slider = GetComponent<Slider>();
    }

    void OnEnable() {
        slider.onValueChanged.AddListener(board.ChangeMultiple);
        slider.onValueChanged.AddListener(puzzleManager.ChangeStepDelay);
        slider.onValueChanged.AddListener(ChangeSpeedText);
    }

    void OnDisable() {
        slider.onValueChanged.RemoveAllListeners();
    }

    void ChangeSpeedText(float val) {
        speedText.text = string.Format("Speed: {0:F2}", val);
    }
}