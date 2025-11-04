using TMPro;
using UnityEngine;

public class LossCounter : MonoBehaviour
{
    [SerializeField] private TMP_Text _text;
    private int _enemyloss;
    private int _playerLoss;

    public void SetEnemyLoss(int value) {
        _enemyloss = value;
        UpdateText();
    }
    public void SetPlayerLoss(int value) {
        _playerLoss = value;
        UpdateText();
    }

    private void UpdateText() {
        _text.text = $"{_playerLoss} : {_enemyloss}";
    }

}
