using TMPro;
using FishNet.Object;
using UnityEngine;
using UnityEngine.UI;
using System.Text;
using FishNet.Connection;
using FishNet.Object.Synchronizing;
using FishNet;

/// <summary>
/// Управляет UI лобби и экрана результатов.
/// </summary>
public class LobbyUIManager : NetworkBehaviour
{
    [Header("Lobby UI")]
    [SerializeField] private GameObject _lobbyPanel;
    [SerializeField] private TMP_Text _waitingText;
    [SerializeField] private TMP_Text _playersCountText;

    [Header("Game UI")]
    [SerializeField] private GameObject _gamePanel;
    [SerializeField] private TMP_Text _timerText;

    [Header("Results UI")]
    [SerializeField] private GameObject _resultsPanel;
    [SerializeField] private TMP_Text _resultsText;
    [SerializeField] private TMP_Text _countdownText;
    //[SerializeField] private Button _readyButton;

    private GameManager _gameManager;
    private float _resultsTimer;
    //private bool _isLocalClientReady;

    public override void OnStartNetwork()
    {
        base.OnStartNetwork();

        // Находим GameManager
        _gameManager = NetworkManager.GetInstance<GameManager>();

        if (_gameManager == null)
        {
            Debug.LogError("[LobbyUIManager] GameManager not found!");
            return;
        }

        // Подписываемся на изменения состояния
        _gameManager.CurrentState.OnChange += OnGameStateChanged;
        _gameManager.ConnectedPlayers.OnChange += OnConnectedPlayersChanged;
        _gameManager.MatchTimer.OnChange += OnMatchTimerChanged;
        _gameManager.PlayerScores.OnChange += OnPlayerScoresChanged;

        // Инициализируем UI
        UpdateUIForState(_gameManager.CurrentState.Value);
        UpdateConnectedPlayers(_gameManager.ConnectedPlayers.Value);
    }

    public override void OnStopNetwork()
    {
        base.OnStopNetwork();

        if (_gameManager != null)
        {
            _gameManager.CurrentState.OnChange -= OnGameStateChanged;
            _gameManager.ConnectedPlayers.OnChange -= OnConnectedPlayersChanged;
            _gameManager.MatchTimer.OnChange -= OnMatchTimerChanged;
            _gameManager.PlayerScores.OnChange -= OnPlayerScoresChanged;
        }
    }

    private void Update()
    {
        // Обновляем UI каждый кадр для плавного отображения таймера
        if (_gameManager != null)
        {
            UpdateTimerDisplay(_gameManager.MatchTimer.Value);

            // В состоянии результатов показываем обратный отсчёт
            if (_gameManager.CurrentState.Value == GameManager.GameState.ShowingResults)
            {
                if (_countdownText != null)
                {
                    float timeLeft = _gameManager.ResultDisplayTime - (Time.time - _resultsTimer);
                    _countdownText.text = $"Returning to lobby in {Mathf.CeilToInt(timeLeft)}s...";
                }
            }
        }
    }

    private void OnGameStateChanged(GameManager.GameState oldValue, GameManager.GameState newValue, bool asServer)
    {
        Debug.Log($"[LobbyUIManager] State changed: {oldValue} -> {newValue}");
        UpdateUIForState(newValue);
    }

    private void OnConnectedPlayersChanged(int oldValue, int newValue, bool asServer)
    {
        UpdateConnectedPlayers(newValue);
    }

    private void OnMatchTimerChanged(float oldValue, float newValue, bool asServer)
    {
        UpdateTimerDisplay(newValue);
    }

    private void OnPlayerScoresChanged(SyncDictionaryOperation op, int key, int value, bool asServer)
    {
        // Обновляем таблицу результатов при изменении счёта
        if (_gameManager.CurrentState.Value == GameManager.GameState.ShowingResults)
        {
            UpdateResultsDisplay();
        }
    }

    private void UpdateUIForState(GameManager.GameState state)
    {
        // Скрываем все панели
        if (_lobbyPanel != null) _lobbyPanel.SetActive(false);
        if (_gamePanel != null) _gamePanel.SetActive(false);
        if (_resultsPanel != null) _resultsPanel.SetActive(false);

        switch (state)
        {
            case GameManager.GameState.WaitingForPlayers:
                if (_lobbyPanel != null) _lobbyPanel.SetActive(true);
                break;

            case GameManager.GameState.InProgress:
                if (_gamePanel != null) _gamePanel.SetActive(true);
                break;

            case GameManager.GameState.ShowingResults:
                if (_resultsPanel != null)
                {
                    _resultsPanel.SetActive(true);
                    _resultsTimer = Time.time;
                    UpdateResultsDisplay();
                }
                break;
        }
    }

    private void UpdateConnectedPlayers(int count)
    {
        if (_playersCountText != null)
        {
            int required = _gameManager != null ? _gameManager.RequiredPlayers : 2;
            _playersCountText.text = $"Players: {count}/{required}";
        }

        if (_waitingText != null)
        {
            if (count < (_gameManager?.RequiredPlayers ?? 2))
            {
                _waitingText.text = "Waiting for players...";
            }
            else
            {
                _waitingText.text = "Match starting!";
            }
        }
    }

    private void UpdateTimerDisplay(float timer)
    {
        if (_timerText != null)
        {
            int minutes = Mathf.FloorToInt(timer / 60);
            int seconds = Mathf.FloorToInt(timer % 60);
            _timerText.text = $"{minutes:00}:{seconds:00}";
        }
    }

    private void UpdateResultsDisplay()
    {
        if (_resultsText == null || _gameManager == null) return;

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("_MATCH RESULTS_\n");

        int maxScore = 0;
        string winner = "";

        foreach (var kvp in _gameManager.PlayerScores)
        {
            int clientId = kvp.Key;
            int score = kvp.Value;
            string playerName = GetPlayerName(clientId);

            sb.AppendLine($"{playerName}: {kvp.Value} points");

            if (kvp.Value > maxScore)
            {
                maxScore = kvp.Value;
                winner = playerName;
            }
        }

        sb.AppendLine($"\nWinner: {winner}");

        _resultsText.text = sb.ToString();
    }

    private string GetPlayerName(int clientId)
    {
        // На клиенте ServerManager.Clients синхронизирован, можно получить NetworkConnection по ClientId
        if (InstanceFinder.ClientManager != null && InstanceFinder.ClientManager.Clients.TryGetValue(clientId, out NetworkConnection conn))
        {
            if (conn.FirstObject != null)
            {
                PlayerNetwork pn = conn.FirstObject.GetComponent<PlayerNetwork>();
                if (pn != null && !string.IsNullOrEmpty(pn.Nickname.Value))
                    return pn.Nickname.Value;
            }
        }
        return $"Player {clientId}";
    }

    /*
    // Публичный метод для кнопки "Ready"
    public void OnReadyButtonClicked()
    {
        _isLocalClientReady = true;
        if (_readyButton != null)
            _readyButton.interactable = false;

        // Здесь можно отправить RPC на сервер, что игрок готов
        // ServerSetReady();
    }

    [ServerRpc(RequireOwnership = false)]
    private void ServerSetReady(NetworkConnection sender = null)
    {
        // Логика готовности игрока (если требуется подтверждение от всех)
    }
    */
}