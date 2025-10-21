using TMPro;
using UnityEngine;

public class EnemyCharacter : MonoBehaviour
{
    public Vector3 TargetPosition { get; private set; } = Vector3.zero;
    private float _velocityMagnitude = 0;
    private void Start() {
        TargetPosition = transform.position;
    }
    private void Update() {
        if (_velocityMagnitude > .1f) {
            float maxDistance = _velocityMagnitude * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, TargetPosition, maxDistance);
        } else {
            transform.position = TargetPosition;
        }
    }

    public void SetMovement(in Vector3 position, in Vector3 velocity, in float averageinterval) {
        TargetPosition = position + (velocity * averageinterval);
        _velocityMagnitude = velocity.magnitude;
    }

}
