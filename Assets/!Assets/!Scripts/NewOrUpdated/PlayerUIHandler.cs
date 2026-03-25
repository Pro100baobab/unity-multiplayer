using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUIHandler : NetworkBehaviour
{
    [SerializeField] private TMP_Text _ammoText;
    [SerializeField] private TMP_Text _respawnTimerText;
    [SerializeField] private Image _ammoImage;
    private PlayerShooting _shooting;
    private PlayerNetwork _playerNetwork;
    private float _deathTime;

    private void Awake()
    {
        _shooting = GetComponent<PlayerShooting>();
        _playerNetwork = GetComponent<PlayerNetwork>();
    }

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            enabled = false;
            _ammoText.gameObject.SetActive(false);
            _ammoImage.gameObject.SetActive(false);
            return;
        }

        if (_playerNetwork != null)
        {
            _playerNetwork.Ammo.OnValueChanged += OnAmmoChanged;
            _playerNetwork.IsAlive.OnValueChanged += OnIsAliveChanged;
            OnAmmoChanged(0, _playerNetwork.Ammo.Value);
        }
    }

    public override void OnNetworkDespawn()
    {
        if (_playerNetwork != null && IsOwner)
        {
            _playerNetwork.IsAlive.OnValueChanged -= OnIsAliveChanged;
            _playerNetwork.Ammo.OnValueChanged -= OnAmmoChanged;
        }
    }

    private void Update()
    {
        if (!IsOwner) return;

        if (_shooting != null && _ammoText != null)
            _ammoText.text = $"{_shooting.GetCurrentAmmo()}";


        if (_playerNetwork != null && !_playerNetwork.IsAlive.Value && _respawnTimerText != null)
        {
            float elapsed = Time.time - _deathTime;
            float remaining = Mathf.Max(0, 3f - elapsed);
            _respawnTimerText.text = $"Respawn: {remaining:F1}s";
            if (remaining <= 0) _respawnTimerText.text = "";
        }
        else if (_respawnTimerText != null)
        {
            _respawnTimerText.text = "";
        }
    }

    private void OnIsAliveChanged(bool prev, bool next)
    {
        if (!next)
        {
            _deathTime = Time.time;
        }
    }

    private void OnAmmoChanged(int oldValue, int newValue)
    {
        if (_ammoText != null)
            _ammoText.text = $"{newValue}";
    }
}