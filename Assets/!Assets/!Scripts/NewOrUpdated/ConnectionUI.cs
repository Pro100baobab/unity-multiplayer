using TMPro;
using FishNet;
using UnityEngine;

public class ConnectionUI : MonoBehaviour
{
    [SerializeField] private TMP_InputField _nicknameInput;

    // Статическое поле, чтобы сохранить ник до появления сетевого игрока.
    public static string PlayerNickname { get; private set; } = "Player";

    // Вызывается при нажатии кнопки Host
    public void StartAsHost()
    {
        SaveNickname();
        InstanceFinder.ServerManager.StartConnection();
        InstanceFinder.ClientManager.StartConnection();
        gameObject.SetActive(false);
    }

    // Вызывается при нажатии кнопки Client
    public void StartAsClient()
    {
        SaveNickname();
        InstanceFinder.ClientManager.StartConnection();
        gameObject.SetActive(false);
    }

    private void SaveNickname()
    {
        string rawValue = _nicknameInput != null ? _nicknameInput.text : string.Empty;
        PlayerNickname = string.IsNullOrWhiteSpace(rawValue) ? "Player" : rawValue.Trim();
    }
}