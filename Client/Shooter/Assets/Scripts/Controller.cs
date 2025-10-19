using System.Collections.Generic;
using UnityEngine;

public class Controller : MonoBehaviour
{
    [SerializeField] private PlayerCharacter _player;
    void Update()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        _player.SetInput(h, v);
        SendMove();
    }

    private void SendMove() {
        _player.GetMoveInfo(out Vector3 position);

        Dictionary<string, object> data = new Dictionary<string, object>()
        {
            {"x", position.x},
            {"y", position.z}
        };
        MultiplayerManager.Instance.SendMessàge("move", data);
    }


}
