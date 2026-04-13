using TMPro;
using FishNet.Object;
using UnityEngine;

public class PlayerView : NetworkBehaviour
{
    [SerializeField] private PlayerNetwork _playerNetwork;
    [SerializeField] private TMP_Text _nicknameText;
    [SerializeField] private TMP_Text _hpText;

    public override void OnStartNetwork()
    {
        base.OnStartNetwork();
        if (_playerNetwork == null) _playerNetwork = GetComponent<PlayerNetwork>();

        // Подписываемся на изменения SyncVar
        _playerNetwork.Nickname.OnChange += OnNicknameChanged;
        _playerNetwork.HP.OnChange += OnHpChanged;

        // Устанавливаем начальные значения
        UpdateNickname(_playerNetwork.Nickname.Value);
        UpdateHp(_playerNetwork.HP.Value);
    }

    public override void OnStopNetwork()
    {
        base.OnStopNetwork();
        if (_playerNetwork != null)
        {
            _playerNetwork.Nickname.OnChange -= OnNicknameChanged;
            _playerNetwork.HP.OnChange -= OnHpChanged;
        }
    }

    private void OnNicknameChanged(string oldValue, string newValue, bool asServer)
    {
        UpdateNickname(newValue);
    }

    private void OnHpChanged(int oldValue, int newValue, bool asServer)
    {
        UpdateHp(newValue);
    }

    private void UpdateNickname(string nick)
    {
        if (_nicknameText != null)
            _nicknameText.text = nick;
    }

    private void UpdateHp(int hp)
    {
        if (_hpText != null)
            _hpText.text = $"HP: {hp}";
    }
}