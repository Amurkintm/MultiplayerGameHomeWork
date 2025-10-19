using UnityEngine;

public class PlayerCharacter : MonoBehaviour
{
    [SerializeField] private float _speed = 2f;
    [SerializeField] private float _positionErrorCorrectionSpeed = 4f;
    [SerializeField] private float _maxAllowedError = 2f;

    private float _inputH;
    private float _inputV;
    private Vector3 _serverPosition;
    private bool _needsCorrection;
    private bool _isMoving;
    private Vector3 _smoothVelocity;

    public void SetInput(float h, float v) {
        _inputH = h;
        _inputV = v;
        _isMoving = Mathf.Abs(h) > 0.01f || Mathf.Abs(v) > 0.01f;
    }

    private void Update() {
        Move();
        CorrectPositionIfNeeded();
    }

    private void Move() {
        if (_isMoving) {
            Vector3 direction = new Vector3(_inputH, 0, _inputV).normalized;
            Vector3 movement = direction * Time.deltaTime * _speed;
            transform.position += movement;
        }

        // Более плавная коррекция
        if (_needsCorrection) {
            float distanceToServer = Vector3.Distance(transform.position, _serverPosition);
            float correctionFactor = _positionErrorCorrectionSpeed * Time.deltaTime;

            // Увеличиваем скорость коррекции при большом расхождении
            if (distanceToServer > 1f) {
                correctionFactor *= 2f;
            }

            transform.position = Vector3.SmoothDamp(transform.position, _serverPosition,
                ref _smoothVelocity, 0.3f);

            if (distanceToServer < 0.05f) {
                _needsCorrection = false;
                _smoothVelocity = Vector3.zero;
            }
        }
    }

    public void SetServerPosition(Vector3 serverPosition, bool isMoving) {
        _serverPosition = serverPosition;

        float errorDistance = Vector3.Distance(transform.position, serverPosition);

        // Корректируем только если ошибка значительная
        if (errorDistance > _maxAllowedError) {
            _needsCorrection = true;
        } else if (errorDistance > 0.1f && !_isMoving) {
            // При остановке корректируем даже маленькие ошибки
            _needsCorrection = true;
        }

        _isMoving = isMoving;
    }

    private void CorrectPositionIfNeeded() {
        if (_needsCorrection && Vector3.Distance(transform.position, _serverPosition) > 0.05f) {
            transform.position = Vector3.SmoothDamp(transform.position, _serverPosition,
                ref _smoothVelocity, 0.3f);
        } else {
            _needsCorrection = false;
        }
    }

    public void GetMoveInfo(out Vector3 position) {
        position = transform.position;
    }

    public bool IsMoving() {
        return _isMoving;
    }
}