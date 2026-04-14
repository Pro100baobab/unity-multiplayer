using TMPro;
using FishNet;
using UnityEngine;

public class ConnectionUI : MonoBehaviour
{
    [SerializeField] private GameObject _gameCanvas;

    [SerializeField] private TMP_InputField _nicknameInput;
    [SerializeField] private TMP_InputField _ipAddress;
    [SerializeField] private TMP_InputField _port;

    // Статическое поле, чтобы сохранить ник до появления сетевого игрока.
    public static string PlayerNickname { get; private set; } = "Player";

    // IP WSL
    private string serverIP = "172.19.124.99";
    private ushort serverPort = 7700;

    // Вызывается при нажатии кнопки Host
    public void StartAsHost()
    {
        SaveNickname();
        InstanceFinder.ServerManager.StartConnection();
        InstanceFinder.ClientManager.StartConnection();

        gameObject.SetActive(false);
        if (_gameCanvas != null) _gameCanvas.SetActive(true);
    }

    // Вызывается при нажатии кнопки Client
    public void StartAsClient()
    {
        SaveNickname();
        SaveConnectionSettings();
        InstanceFinder.ClientManager.StartConnection(serverIP, serverPort);

        gameObject.SetActive(false);
        if (_gameCanvas != null) _gameCanvas.SetActive(true);
    }

    private void SaveNickname()
    {
        string rawValue = _nicknameInput != null ? _nicknameInput.text : string.Empty;
        PlayerNickname = string.IsNullOrWhiteSpace(rawValue) ? "Player" : rawValue.Trim();
    }

    private void SaveConnectionSettings()
    {
        serverIP = _ipAddress.text.Trim();

        if (ushort.TryParse(_port.text.Trim(), out ushort parsedPort))
        {
            serverPort = parsedPort;
        }
        else
        {
            // Обработка ошибки: устанавливаем порт по умолчанию или показываем сообщение
            serverPort = 7700; // порт по умолчанию
            Debug.LogWarning("Некорректный порт. Используется порт по умолчанию: " + serverPort);
        }
    }

}