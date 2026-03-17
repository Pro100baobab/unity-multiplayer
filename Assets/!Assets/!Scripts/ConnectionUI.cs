using TMPro;
using Unity.Netcode;
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
        NetworkManager.Singleton.StartHost();
    }

    // Вызывается при нажатии кнопки Client
    public void StartAsClient()
    {
        SaveNickname();
        NetworkManager.Singleton.StartClient();
    }

    private void SaveNickname()
    {
        string rawValue = _nicknameInput != null ? _nicknameInput.text : string.Empty;
        PlayerNickname = string.IsNullOrWhiteSpace(rawValue) ? "Player" : rawValue.Trim();
    }
}