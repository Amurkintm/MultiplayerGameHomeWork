using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller : MonoBehaviour
{
    [SerializeField] private float _restartDelay = 3f;
    [SerializeField] private PlayerCharacter _player;
    [SerializeField] private PlayerGun _gun;
    [SerializeField] private float _mouseSensetivity = 2f;
    private MultiplayerManager _multiplayerManager;
    private bool _hold = false;
    bool _isEscOn = false;

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
            if (_hold) return;

            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");

            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");

            bool isShoot = Input.GetMouseButton(0);

            bool space = Input.GetKeyDown(KeyCode.Space);

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
        _multiplayerManager.Send("shoot", json);
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
        _multiplayerManager.Send("move", data);
    }

    public void Restart(int spawnIndex) {
        _multiplayerManager._spawnPoints.GetPoint(spawnIndex, out Vector3 position, out Vector3 rotation);
        StartCoroutine(Hold());

        _player.transform.position = position;
        rotation.x = 0;
        rotation.z = 0;
        _player.transform.eulerAngles = rotation;
        _player.SetInput(0,0,0);

        Dictionary<string, object> data = new Dictionary<string, object>()
        {
            {"pX", position.x},
            {"pY", position.y},
            {"pZ", position.z},
            {"vX", 0},
            {"vY", 0},
            {"vZ", 0},
            {"rX", 0},
            {"rY", rotation.y}
        };
        _multiplayerManager.Send("move", data);
    }

    private IEnumerator Hold() {
        _hold = true;
        yield return new WaitForSecondsRealtime(_restartDelay);
        _hold = false;
    }
}
[Serializable]
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