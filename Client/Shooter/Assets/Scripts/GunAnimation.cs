using System;
using UnityEngine;

public class GunAnimation : MonoBehaviour
{
    private const string shoot = "Shoot";
    

    [SerializeField] private Gun _gun;
    [SerializeField] private Animator _animator;
    private void Start() {
        _gun.shoot += Shoot;
    }

    private void Shoot() {
        _animator.SetTrigger(shoot);
    }

    private void OnDestroy() {
        _gun.shoot -= Shoot;
    }
}
