using FishNet;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using FishNet.Transporting;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Серверный менеджер игры. Управляет состояниями матча (лобби, игра, результаты),
/// отслеживает количество подключённых игроков и синхронизирует данные с клиентами.
/// </summary>
public class GameManager : NetworkBehaviour
{
    [Header("Match Settings")]
    [SerializeField] private int _requiredPlayers = 2;
    [SerializeField] private float _matchDuration = 60f;
    [SerializeField] private float _resultsDisplayTime = 5f;


    public readonly SyncVar<GameState> CurrentState = new SyncVar<GameState>(new SyncTypeSettings(
        WritePermission.ServerOnly, ReadPermission.Observers, 0f, Channel.Reliable)); 
    public readonly SyncVar<int> ConnectedPlayers = new SyncVar<int>();
    public readonly SyncVar<float> MatchTimer = new SyncVar<float>();
    // Счёт игроков (ключ — NetworkConnection.clientId, значение — количество очков)
    public readonly SyncDictionary<int, int> PlayerScores = new SyncDictionary<int, int>();

    public enum GameState
    {
        WaitingForPlayers,
        InProgress,
        ShowingResults
    }

    public int RequiredPlayers => _requiredPlayers;
    public float ResultDisplayTime => _resultsDisplayTime;

    private void Awake()
    {
        CurrentState.OnChange += OnCurrentStateChanged;
        ConnectedPlayers.OnChange += OnConnectedPlayersChanged;
        MatchTimer.OnChange += OnMatchTimerChanged;
        PlayerScores.OnChange += OnPlayerScoresChanged;
    }

    public override void OnStartNetwork()
    {
        base.OnStartNetwork();
        NetworkManager.RegisterInstance(this);
    }

    public override void OnStartServer()
    {
        base.OnStartServer();

        ServerManager.OnRemoteConnectionState += OnPlayerConnectionStateChanged;

        // Инициализируем начальные значения
        CurrentState.Value = GameState.WaitingForPlayers;
        ConnectedPlayers.Value = ServerManager.Clients.Count;
        MatchTimer.Value = _matchDuration;

        Debug.Log("[GameManager] Server started. Waiting for players...");
    }

    public override void OnStopServer()
    {
        base.OnStopServer();

        if (ServerManager != null)
            ServerManager.OnRemoteConnectionState -= OnPlayerConnectionStateChanged;
    }

    public override void OnStopNetwork()
    {
        base.OnStopNetwork();

        if (NetworkManager != null)
            NetworkManager.UnregisterInstance<GameManager>();
    }


    // Обработчика SyncVar

    private void OnCurrentStateChanged(GameState oldValue, GameState newValue, bool asServer)
    {
        Debug.Log($"[GameManager] Game state changed: {oldValue} -> {newValue} (asServer: {asServer})");
        // UI будет реагировать на это изменение через собственные подписки
    }

    private void OnConnectedPlayersChanged(int oldValue, int newValue, bool asServer)
    {
        Debug.Log($"[GameManager] Connected players: {oldValue} -> {newValue}");
    }

    private void OnMatchTimerChanged(float oldValue, float newValue, bool asServer)
    {
        // Таймер обновляется — UI будет реагировать на изменение
    }

    private void OnPlayerScoresChanged(SyncDictionaryOperation op, int key, int value, bool asServer)
    {
        Debug.Log($"[GameManager] Player scores changed: {op} for player {key} = {value}");
    }


    // Обработка подключений/отключений

    private void OnPlayerConnectionStateChanged(NetworkConnection conn, RemoteConnectionStateArgs args)
    {
        if (!IsServerInitialized) return;

        ConnectedPlayers.Value = ServerManager.Clients.Count;

        if (args.ConnectionState == RemoteConnectionState.Started)
        {
            Debug.Log($"[GameManager] Player connected. Total: {ConnectedPlayers.Value}/{_requiredPlayers}");

            if (!PlayerScores.ContainsKey(conn.ClientId))
                PlayerScores.Add(conn.ClientId, 0);
        }
        else if (args.ConnectionState == RemoteConnectionState.Stopped)
        {
            Debug.Log($"[GameManager] Player disconnected. Total: {ConnectedPlayers.Value}/{_requiredPlayers}");

            if (PlayerScores.ContainsKey(conn.ClientId))
                PlayerScores.Remove(conn.ClientId);
        }

        // Проверяем условие старта матча
        if (CurrentState.Value == GameState.WaitingForPlayers && ConnectedPlayers.Value >= _requiredPlayers)
        {
            Invoke(nameof(StartMatch), 0.2f);
        }
    }

    private void StartMatch()
    {
        if (!IsServerInitialized) return;
        if (CurrentState.Value != GameState.WaitingForPlayers) return;

        CurrentState.Value = GameState.InProgress;
        MatchTimer.Value = _matchDuration;

        Debug.Log("[GameManager] Match started!");
        OnMatchStartedObservers();
    }

    private void EndMatch()
    {
        if (!IsServerInitialized) return;
        if (CurrentState.Value != GameState.InProgress) return;

        CurrentState.Value = GameState.ShowingResults;
        Debug.Log("[GameManager] Match ended! Showing results...");

        OnMatchEndedObservers();
        Invoke(nameof(ResetToLobby), _resultsDisplayTime);
    }

    private void ResetToLobby()
    {
        if (!IsServerInitialized) return;

        // Сбрасываем счёт всех игроков
        var clients = new List<NetworkConnection>(ServerManager.Clients.Values);
        foreach (var conn in clients)
        {
            if (PlayerScores.ContainsKey(conn.ClientId))
                PlayerScores[conn.ClientId] = 0;

            ResetPlayerState(conn);
        }

        MatchTimer.Value = _matchDuration;
        CurrentState.Value = GameState.WaitingForPlayers;

        Debug.Log("[GameManager] Lobby reset. Waiting for players...");
    }

    private void ResetPlayerState(NetworkConnection conn)
    {
        foreach (var nob in conn.Objects)
        {
            if (nob.TryGetComponent<PlayerNetwork>(out var pn))
            {
                pn.HP.Value = 100;
                pn.Ammo.Value = 10;
                pn.IsAlive.Value = true;
            }
        }
    }

    // RPC для начисления очков

    [ServerRpc(RequireOwnership = false)]
    public void AddScore(NetworkConnection playerConn)
    {
        if (!IsServerInitialized) return;
        if (CurrentState.Value != GameState.InProgress) return;

        int clientId = playerConn.ClientId;

        if (PlayerScores.ContainsKey(clientId))
        {
            PlayerScores[clientId]++;
            Debug.Log($"[GameManager] Player {clientId} score: {PlayerScores[clientId]}");
        }
    }

    // RPC для уведомления клиентов

    [ObserversRpc(BufferLast = true)]
    private void OnMatchStartedObservers()
    {
        Debug.Log("[GameManager] Match started notification received.");
    }

    [ObserversRpc(BufferLast = true)]
    private void OnMatchEndedObservers()
    {
        Debug.Log("[GameManager] Match ended notification received.");
    }

    private void Update()
    {
        if (!IsServerInitialized) return;

        if (CurrentState.Value == GameState.InProgress)
        {
            MatchTimer.Value -= Time.deltaTime;

            if (MatchTimer.Value <= 0f)
            {
                MatchTimer.Value = 0f;
                EndMatch();
            }
        }
    }
}