using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Controller : MonoBehaviour
{
    [SerializeField] private PlayerCharacter _player;
    [SerializeField] private PlayerGun _gun;
    [SerializeField] private float _mouseSensetivity = 2f;
    private MultiplayerManager _multiplayerManager;
    bool _isEscOn = false;
    bool _isCrouching = false;

    private void Start() {
        _multiplayerManager = MultiplayerManager.Instance;
    }

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

            bool isShoot = Input.GetMouseButton(0);

            bool space = Input.GetKeyDown(KeyCode.Space);

            _isCrouching = Input.GetKey(KeyCode.LeftControl);

            _player.SetInput(h, v, mouseX * _mouseSensetivity);
            _player.RotateX(mouseY * -_mouseSensetivity);
            if (space) _player.Jump();
            if (isShoot && _gun.TryShoot(out ShootInfo shootInfo)) SendShoot(ref shootInfo);
            SendMove();
        } else {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

    }

    private void SendShoot(ref ShootInfo shootInfo) {
        shootInfo.key = _multiplayerManager.GetSessionID();
        string json = JsonUtility.ToJson(shootInfo);
        _multiplayerManager.SendMessаge("shoot", json);
    }

    private void SendMove() {
        _player.GetMoveInfo(out Vector3 position, out Vector3 velocity, out float rotateX, out float rotateY, out bool isCrouching);
        Dictionary<string, object> data = new Dictionary<string, object>()
        {
            {"pX", position.x},
            {"pY", position.y},
            {"pZ", position.z},
            {"vX", velocity.x},
            {"vY", velocity.y},
            {"vZ", velocity.z},
            {"rX", rotateX},
            {"rY", rotateY},
            {"crouch", isCrouching }
        };
        _multiplayerManager.SendMessаge("move", data);
    }


}
[System.Serializable]
public struct ShootInfo
{
    public string key;
    public float pX;
    public float pY;
    public float pZ;
    public float dX;
    public float dY;
    public float dZ;
}