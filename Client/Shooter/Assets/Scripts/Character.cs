using UnityEngine;

public abstract class Character : MonoBehaviour
{
    [field: SerializeField] public float speed { get; protected set; } = 2f;
    public Vector3 velocity { get; protected set; }

    [SerializeField] protected CapsuleCollider _collider;
    [SerializeField] protected Vector3 _colliderCenterCrouch = new Vector3(0, .65f, 0);
    [SerializeField] protected float _colliderHeightCrouch = 1.1f;
    protected Vector3 _colliderCenterStand;
    protected float _colliderHeightStand;
    public bool IsCrouching { get; protected set; } = false;

    public void Crouching(bool isCrouching) {
        if (IsCrouching == isCrouching) return;

        if (isCrouching) {
            _collider.center = _colliderCenterCrouch;
            _collider.height = _colliderHeightCrouch;
        } else {
            _collider.center = _colliderCenterStand;
            _collider.height = _colliderHeightStand;
        }

        IsCrouching = isCrouching;
    }
}
