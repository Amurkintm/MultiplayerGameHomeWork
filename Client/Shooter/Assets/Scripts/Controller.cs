using System.Collections.Generic;
using UnityEngine;

public class Controller : MonoBehaviour
{
    [SerializeField] private PlayerCharacter _player;
    [SerializeField] private float _sendRate = 0.1f; 

    private float _lastSendTime;
    private float _lastH;
    private float _lastV;
    private bool _isMoving;
    private Vector3 _lastSentPosition;

    void Update() {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        _player.SetInput(h, v);

        bool wasMoving = _isMoving;
        _isMoving = Mathf.Abs(h) > 0.01f || Mathf.Abs(v) > 0.01f;

        _player.GetMoveInfo(out Vector3 currentPosition);
        float positionChanged = Vector3.Distance(_lastSentPosition, currentPosition);

        bool directionChanged = Mathf.Abs(h - _lastH) > 0.1f || Mathf.Abs(v - _lastV) > 0.1f;
        bool shouldSend = false;

        // Условия отправки:
        if (Time.time - _lastSendTime >= _sendRate && _isMoving)
            shouldSend = true;
        else if (wasMoving && !_isMoving) // Остановка
            shouldSend = true;
        else if (directionChanged && _isMoving) // Изменение направления
            shouldSend = true;
        else if (positionChanged > 0.3f) // Значительное изменение позиции
            shouldSend = true;

        if (shouldSend) {
            SendMove();
            _lastSendTime = Time.time;
            _lastH = h;
            _lastV = v;
            _lastSentPosition = currentPosition;
        }
    }

    private void SendMove() {
        _player.GetMoveInfo(out Vector3 position);

        Dictionary<string, object> data = new Dictionary<string, object>()
        {
            {"x", position.x},
            {"y", position.z},
            {"moving", _isMoving}
        };
        MultiplayerManager.Instance.SendMessage("move", data);
    }
}