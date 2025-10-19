using Colyseus;
using Colyseus.Schema;
using System;
using System.Collections.Generic;
using UnityEngine;

public class MultiplayerManager : ColyseusManager<MultiplayerManager>
{
    [Header("Network Settings")]
    [SerializeField] private float _networkUpdateRate = 0.04f; // 25 FPS
    [SerializeField] private float _positionSyncRate = 0.1f; // 10 FPS для синхронизации позиции
    [SerializeField] private bool _enableInterpolation = true;

    [Header("Prefabs")]
    [SerializeField] private GameObject _playerPrefab;
    [SerializeField] private EnemyController _enemyPrefab;

    private ColyseusRoom<State> _room;
    private Dictionary<string, EnemyController> _enemies = new Dictionary<string, EnemyController>();
    private PlayerCharacter _localPlayer;
    private float _lastNetworkTime;
    private float _lastPositionSyncTime;
    private string _sessionId;
    private bool _isConnected = false;

    // События для UI и других систем
    public event Action<string> OnConnected;
    public event Action<string> OnDisconnected;
    public event Action<string, EnemyController> OnEnemyCreated;
    public event Action<string> OnEnemyRemoved;

    protected override void Awake() {
        base.Awake();

        // Инициализация клиента с настройками
        Instance.InitializeClient();

        // Настройка клиента
        //ConfigureClient();

        Connect();
    }

    private async void Connect() {
        try {
            Debug.Log("Connecting to server...");
            _room = await Instance.client.JoinOrCreate<State>("state_handler");
            _sessionId = _room.SessionId;
            _isConnected = true;
            Debug.Log($"Connected successfully! Session ID: {_sessionId}");

            // Подписка на события комнаты
            _room.OnLeave += OnRoomLeave;     // Заменяет OnClose
            _room.OnError += OnRoomError;     // Заменяет OnError
            _room.OnStateChange += OnStateChange;

            OnConnected?.Invoke(_sessionId);
        }
        catch (Exception ex) {
            Debug.LogError($"Failed to connect: {ex.Message}");
            _isConnected = false;
            OnDisconnected?.Invoke($"Failed to connect: {ex.Message}");
        }
    }

    // Обработчики событий комнаты
    private void OnRoomLeave(int code) {
        Debug.Log($"Left room with code: {code}");
        _isConnected = false;
        OnDisconnected?.Invoke("Left room");
    }

    private void OnRoomError(int code, string message) {
        Debug.LogError($"Room error {code}: {message}");
        _isConnected = false;
        OnDisconnected?.Invoke($"Error: {message}");
    }

    private void Update() {
        if (!_isConnected || _room == null) return;

        // Синхронизация с серверной частотой обновлений - отправка управления
        if (Time.time - _lastNetworkTime >= _networkUpdateRate) {
            SendInputUpdate();
            _lastNetworkTime = Time.time;
        }

        // Менее частая синхронизация позиции (для коррекции)
        if (Time.time - _lastPositionSyncTime >= _positionSyncRate) {
            SendPositionSync();
            _lastPositionSyncTime = Time.time;
        }
    }

    private void SendInputUpdate() {
        if (_localPlayer == null) return;

        // Получаем текущий ввод от игрока
        _localPlayer.GetMoveInfo(out Vector3 position);
        bool isMoving = _localPlayer.IsMoving();

        Dictionary<string, object> data = new Dictionary<string, object>()
        {
            {"x", position.x},
            {"y", position.z},
            {"moving", isMoving},
            {"clientTime", Time.time}
        };

        SendMessage("move", data);
    }

    private void SendPositionSync() {
        if (_localPlayer == null) return;

        // Отправляем точную позицию для коррекции на сервере
        _localPlayer.GetMoveInfo(out Vector3 position);

        Dictionary<string, object> data = new Dictionary<string, object>()
        {
            {"x", position.x},
            {"y", position.z},
            {"sync", true}, // Флаг что это синхронизация позиции
            {"clientTime", Time.time}
        };

        SendMessage("position_sync", data);
    }

    private void OnStateChange(State state, bool isFirstState) {
        if (isFirstState) {
            Debug.Log("Initial game state received");

            // Создаем всех существующих игроков
            state.players.ForEach((key, player) =>
            {
                if (key == _sessionId)
                    CreatePlayer(player);
                else
                    CreateEnemy(key, player);
            });

            // Подписка на изменения игроков
            _room.State.players.OnAdd += CreateEnemy;
            _room.State.players.OnRemove += RemoveEnemy;
            _room.State.players.OnChange += OnPlayerChange;
        }
    }

    private void CreatePlayer(Player player) {
        try {
            var position = new Vector3(player.x, 0, player.y);
            GameObject playerObj = Instantiate(_playerPrefab, position, Quaternion.identity);
            _localPlayer = playerObj.GetComponent<PlayerCharacter>();

            if (_localPlayer == null) {
                Debug.LogError("Player prefab doesn't have PlayerCharacter component!");
                return;
            }

            // Сохраняем начальную позицию с сервера
            _localPlayer.SetServerPosition(position, player.moving);

            Debug.Log($"Local player created at position: {position}");
        }
        catch (Exception ex) {
            Debug.LogError($"Error creating player: {ex.Message}");
        }
    }

    private void CreateEnemy(string key, Player player) {
        try {
            if (_enemies.ContainsKey(key)) {
                Debug.LogWarning($"Enemy with key {key} already exists!");
                return;
            }

            var position = new Vector3(player.x, 0, player.y);
            var enemy = Instantiate(_enemyPrefab, position, Quaternion.identity);

            // Настройка интерполяции
            if (!_enableInterpolation) {
                var enemyController = enemy.GetComponent<EnemyController>();
                if (enemyController != null) {
                    // Можно отключить интерполяцию если нужно
                }
            }

            // Создаем начальные изменения для врага
            var changes = new List<DataChange>
            {
                new DataChange { Field = "x", Value = player.x },
                new DataChange { Field = "y", Value = player.y },
                new DataChange { Field = "moving", Value = player.moving }
            };

            enemy.OnChange(changes);
            _enemies[key] = enemy;
            player.OnChange += enemy.OnChange;

            Debug.Log($"Enemy created: {key} at position: {position}");

            OnEnemyCreated?.Invoke(key, enemy);
        }
        catch (Exception ex) {
            Debug.LogError($"Error creating enemy: {ex.Message}");
        }
    }

    private void OnPlayerChange(string key, Player player) {
        // Обновляем позицию локального игрока при получении данных с сервера
        if (key == _sessionId && _localPlayer != null) {
            Vector3 serverPosition = new Vector3(player.x, 0, player.y);
            // Передаем состояние движения, если поле добавлено в схему
            _localPlayer.SetServerPosition(serverPosition, player.moving);
        }
    }

    private void RemoveEnemy(string key, Player player) {
        try {
            if (_enemies.ContainsKey(key)) {
                Destroy(_enemies[key].gameObject);
                _enemies.Remove(key);
                Debug.Log($"Enemy removed: {key}");

                OnEnemyRemoved?.Invoke(key);
            }
        }
        catch (Exception ex) {
            Debug.LogError($"Error removing enemy: {ex.Message}");
        }
    }

    // Обработчики сообщений от сервера
    private void OnWelcomeMessage(object data) {
        Debug.Log("Welcome message from server: " + data);
    }

    private void OnPongMessage(float timestamp) {
        float ping = Time.time - timestamp;
        Debug.Log($"Ping: {ping * 1000}ms");
    }

   

    private void Cleanup() {
        foreach (var enemy in _enemies.Values) {
            if (enemy != null)
                Destroy(enemy.gameObject);
        }
        _enemies.Clear();

        if (_localPlayer != null) {
            Destroy(_localPlayer.gameObject);
            _localPlayer = null;
        }
    }

    protected override void OnDestroy() {
        if (_room != null) {
            _room.Leave();
            _room = null;
        }

        Cleanup();
        base.OnDestroy();
    }

    public void SendMessage(string key, Dictionary<string, object> data) {
        if (_room != null && _isConnected) {
            try {
                _room.Send(key, data);
            }
            catch (System.Exception ex) {
                Debug.LogError($"Failed to send message {key}: {ex.Message}");
            }
        }
    }

    // Public methods for other scripts
    public bool IsConnected() => _isConnected;
    public string GetSessionId() => _sessionId;
    public int GetEnemyCount() => _enemies.Count;
    public PlayerCharacter GetLocalPlayer() => _localPlayer;
}