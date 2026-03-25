using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerView : NetworkBehaviour
{
    [SerializeField] private PlayerNetwork _playerNetwork;
    [SerializeField] private TMP_Text _nicknameText;
    [SerializeField] private TMP_Text _hpText;

    public override void OnNetworkSpawn()
    {
        // Подписываемся на изменения сетевых переменных
        _playerNetwork.Nickname.OnValueChanged += OnNicknameChanged;
        _playerNetwork.HP.OnValueChanged += OnHpChanged;

        // Устанавливаем начальные значения
        OnNicknameChanged(default, _playerNetwork.Nickname.Value);
        OnHpChanged(0, _playerNetwork.HP.Value);
    }

    public override void OnNetworkDespawn()
    {
        // Отписываемся при деспавне
        _playerNetwork.Nickname.OnValueChanged -= OnNicknameChanged;
        _playerNetwork.HP.OnValueChanged -= OnHpChanged;
    }

    private void OnNicknameChanged(FixedString32Bytes oldValue, FixedString32Bytes newValue)
    {
        _nicknameText.text = newValue.ToString();
    }

    private void OnHpChanged(int oldValue, int newValue)
    {
        _hpText.text = $"HP: {newValue}";
    }
}