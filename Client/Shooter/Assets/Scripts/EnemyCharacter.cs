using TMPro;
using UnityEngine;

public class EnemyCharacter : Character
{
    [SerializeField] private Transform _head;

    public Vector3 _targetPosition = Vector3.zero;
    private float _velocityMagnitude = 0;
    
    private void Update() {
        if (_velocityMagnitude > .1f) {
            float maxDistance = _velocityMagnitude * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, _targetPosition, maxDistance);
        } else {
            transform.position = _targetPosition;
        }
    }
    public void SetSpeed(float value) => speed = value;

    public void SetMovement(in Vector3 position, in Vector3 velocity, in float averageinterval) {
        _targetPosition = position + (velocity * averageinterval);
        _velocityMagnitude = velocity.magnitude;

        this.velocity = velocity;
    }
    public void SetRotateX(float value) {
        _head.localEulerAngles = new Vector3(value, 0, 0);
    }
    public void SetRotateY(float value) {
        transform.localEulerAngles = new Vector3(0, value, 0);
    }

}
