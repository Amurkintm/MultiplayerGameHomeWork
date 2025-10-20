using UnityEngine;
[RequireComponent(typeof(Rigidbody))]
public class PlayerCharacter : MonoBehaviour
{
    [SerializeField] private Rigidbody _rigidbody;
    [SerializeField] private float _speed = 2f;
    private float _inputH;
    private float _inputV;
    private void Start() {
        _rigidbody=GetComponent<Rigidbody>();
    }
    public void SetInput(float h, float v) {
        _inputH = h;
        _inputV = v;
    }
    private void Update() {        
        Move();
    }
    private void Move() {
        //Vector3 direction = new Vector3(_inputH, 0, _inputV).normalized;
        //transform.position += direction * Time.deltaTime * _speed;

        Vector3 velocity = (transform.forward * _inputV + transform.right * _inputH).normalized * _speed;
        _rigidbody.linearVelocity = velocity;
    }

    public void GetMoveInfo(out Vector3 position, out Vector3 velocity) {
        position = transform.position;
        velocity = _rigidbody.linearVelocity;
    }

}
