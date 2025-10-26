using UnityEngine;

public class CheckFly : MonoBehaviour
{
    public bool IsFly { get; private set; }
    [SerializeField] private LayerMask _layerMask;
    [SerializeField] private float _radius;
    [SerializeField] private float _coyoteTime = .15f;
    private float _flyTimer = 0;
    private void Update() {
        if (Physics.CheckSphere(transform.position, _radius, _layerMask)) {
            IsFly = false;
            _flyTimer = 0;
        } else {
            _flyTimer += Time.deltaTime;
            if(_flyTimer > _coyoteTime) IsFly = true;
        }
    }
#if UNITY_EDITOR
    private void OnDrawGizmosSelected() {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _radius);
    }
#endif
}
