using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Controller : MonoBehaviour
{
    [SerializeField] private PlayerCharacter _player;
    [SerializeField] private float _mouseSensetivity = 2f;
    bool _isEscOn = false;
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            _isEscOn = !_isEscOn;
        }
        if (!_isEscOn) {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");

            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");

            bool space = Input.GetKeyDown(KeyCode.Space);

            _player.SetInput(h, v, mouseX * _mouseSensetivity);
            _player.RotateX(mouseY * -_mouseSensetivity);
            if (space) _player.Jump();
            SendMove();
        } else {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

    }

    private void SendMove() {
        _player.GetMoveInfo(out Vector3 position, out Vector3 velocity, out float rotateX, out float rotateY);

        Dictionary<string, object> data = new Dictionary<string, object>()
        {
            {"pX", position.x},
            {"pY", position.y},
            {"pZ", position.z},
            {"vX", velocity.x},
            {"vY", velocity.y},
            {"vZ", velocity.z},
            {"rX", rotateX},
            {"rY", rotateY}
        };
        MultiplayerManager.Instance.SendMessàge("move", data);
    }


}
