using Colyseus.Schema;
using System;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [SerializeField] private float _interpolationSpeed = 6f;
    [SerializeField] private float _predictionTime = 0.12f;
    [SerializeField] private float _maxCorrectionDistance = 2f;
    [SerializeField] private float _smoothCorrectionSpeed = 4f;

    private Vector3 _targetPosition;
    private Vector3 _displayPosition;
    private Vector3 _velocity;
    private Vector3 _correctionVelocity;
    private Queue<PositionUpdate> _positionHistory = new Queue<PositionUpdate>();
    private float _lastUpdateTime;
    private bool _isMoving = false;
    private float _stoppedTime = 0f;
    private bool _needsSmoothCorrection = false;
    private Vector3 _correctionStartPosition;
    private float _correctionProgress = 0f;

    private struct PositionUpdate
    {
        public float timestamp;
        public Vector3 position;
        public bool moving;
        public Vector3 serverPosition; // Точная позиция с сервера

        public PositionUpdate(float time, Vector3 pos, bool moving, Vector3 serverPos) {
            timestamp = time;
            position = pos;
            this.moving = moving;
            serverPosition = serverPos;
        }
    }

    internal void OnChange(List<DataChange> changes) {
        Vector3 newPosition = transform.position;
        Vector3 serverPosition = newPosition;
        bool isMoving = _isMoving;

        foreach (var dataChange in changes) {
            switch (dataChange.Field) {
                case "x":
                    newPosition.x = (float)dataChange.Value;
                    serverPosition.x = (float)dataChange.Value;
                    break;
                case "y":
                    newPosition.z = (float)dataChange.Value;
                    serverPosition.z = (float)dataChange.Value;
                    break;
                case "moving":
                    isMoving = (bool)dataChange.Value;
                    break;
                default:
                    Debug.LogWarning("Не обрабатывается изменение поля: " + dataChange.Field);
                    break;
            }
        }

        // Проверяем необходимость плавной коррекции
        float distanceToServer = Vector3.Distance(_displayPosition, serverPosition);
        if (distanceToServer > _maxCorrectionDistance) {
            StartSmoothCorrection(serverPosition);
        }

        // Добавляем новую позицию в историю
        float currentTime = Time.time;
        _positionHistory.Enqueue(new PositionUpdate(currentTime, newPosition, isMoving, serverPosition));

        // Ограничиваем размер истории
        while (_positionHistory.Count > 8) {
            _positionHistory.Dequeue();
        }

        // Обновляем состояние движения
        _isMoving = isMoving;

        if (_isMoving) {
            CalculateVelocity();
            _targetPosition = PredictPosition(_predictionTime);
            _stoppedTime = 0f;
        } else {
            _velocity = Vector3.zero;
            _targetPosition = serverPosition; // Используем точную серверную позицию при остановке
            _stoppedTime = currentTime;
        }

        _lastUpdateTime = currentTime;
    }

    private void StartSmoothCorrection(Vector3 targetPosition) {
        _needsSmoothCorrection = true;
        _correctionStartPosition = _displayPosition;
        _correctionProgress = 0f;
        _targetPosition = targetPosition;
    }

    private void CalculateVelocity() {
        if (_positionHistory.Count < 2) return;

        var positions = _positionHistory.ToArray();

        // Используем несколько последних позиций для более стабильного расчета скорости
        int sampleCount = Mathf.Min(3, positions.Length);
        Vector3 totalMovement = Vector3.zero;
        float totalTime = 0f;

        for (int i = positions.Length - 1; i > positions.Length - sampleCount; i--) {
            if (i > 0 && positions[i].moving) {
                float timeDiff = positions[i].timestamp - positions[i - 1].timestamp;
                if (timeDiff > 0.001f) {
                    totalMovement += (positions[i].position - positions[i - 1].position);
                    totalTime += timeDiff;
                }
            }
        }

        if (totalTime > 0.001f) {
            _velocity = totalMovement / totalTime;

            // Ограничиваем максимальную скорость для стабильности
            float maxSpeed = 10f;
            if (_velocity.magnitude > maxSpeed) {
                _velocity = _velocity.normalized * maxSpeed;
            }
        }
    }

    private Vector3 PredictPosition(float predictionTime) {
        if (_positionHistory.Count == 0) return transform.position;

        Vector3 latestPosition = _positionHistory.ToArray()[_positionHistory.Count - 1].position;

        if (!_isMoving) return latestPosition;

        Vector3 prediction = latestPosition + _velocity * predictionTime;

        // Добавляем плавное замедление при маленькой скорости
        if (_velocity.magnitude < 0.5f) {
            prediction = Vector3.Lerp(latestPosition, prediction, 0.7f);
        }

        return prediction;
    }

    private void Update() {
        if (_needsSmoothCorrection) {
            // Плавная коррекция при большом расхождении
            _correctionProgress += _smoothCorrectionSpeed * Time.deltaTime;
            _displayPosition = Vector3.Lerp(_correctionStartPosition, _targetPosition, _correctionProgress);

            if (_correctionProgress >= 1f) {
                _needsSmoothCorrection = false;
                _displayPosition = _targetPosition;
            }

            transform.position = _displayPosition;
        } else if (!_isMoving) {
            // При остановке используем точную позицию
            _displayPosition = _targetPosition;
            transform.position = _displayPosition;
            _velocity = Vector3.zero;
        } else {
            // Плавная интерполяция к предсказанной позиции
            float distanceToTarget = Vector3.Distance(_displayPosition, _targetPosition);

            // Динамическая скорость интерполяции в зависимости от расстояния
            float dynamicInterpolationSpeed = _interpolationSpeed;
            if (distanceToTarget > 1f) {
                dynamicInterpolationSpeed *= 1.5f; // Ускоряем интерполяцию при большом расхождении
            } else if (distanceToTarget < 0.1f) {
                dynamicInterpolationSpeed *= 0.5f; // Замедляем при маленьком расхождении
            }

            _displayPosition = Vector3.Lerp(_displayPosition, _targetPosition,
                dynamicInterpolationSpeed * Time.deltaTime);

            transform.position = _displayPosition;

            // Постоянное обновление предсказания
            float timeSinceLastUpdate = Time.time - _lastUpdateTime;
            if (timeSinceLastUpdate > 0.016f && _isMoving) {
                _targetPosition = PredictPosition(_predictionTime + timeSinceLastUpdate * 0.5f);
            }
        }
    }

    // Визуализация для отладки
    private void OnDrawGizmos() {
        if (!Application.isPlaying) return;

        // Предсказанная позиция (зеленая)
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(_targetPosition, 0.3f);

        // Отображаемая позиция (синяя)
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(_displayPosition, 0.2f);

        // Вектор скорости (красный)
        if (_isMoving && _velocity.magnitude > 0.1f) {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(_displayPosition, _velocity.normalized * 1f);
        }
    }
}