using TMPro;
using UnityEngine;

public class EnemyCharacter : Character
{
    [SerializeField] private Transform _head;

    private bool _isCrouching = false; // Добавлено
    
    public Vector3 targetPosition { get; private set; } = Vector3.zero;
    private float _velocityMagnitude = 0;
    
    private void Update() {
        if (_velocityMagnitude > .1f) {
            float maxDistance = _velocityMagnitude * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, maxDistance);
        } else {
            transform.position = targetPosition;
        }
    }
    public void SetSpeed(float value) => speed = value;

    public void SetMovement(in Vector3 position, in Vector3 velocity, in float averageinterval) {
        targetPosition = position + (velocity * averageinterval);
        _velocityMagnitude = velocity.magnitude;

        this.velocity = velocity;
    }
    public void SetRotateX(float value) {
        _head.localEulerAngles = new Vector3(value, 0, 0);
    }
    public void SetRotateY(float value) {
        transform.localEulerAngles = new Vector3(0, value, 0);
    }

    // Метод для установки состояния приседания (вызывается из EnemyController)
    public void SetCrouching(bool isCrouching) { // Добавлено
        _isCrouching = isCrouching;
    }
    // Метод для получения состояния приседания (вызывается из CharacterAnimation)
    //public bool GetIsCrouching() { // Добавлено
    //    return _isCrouching;
    //}

}
