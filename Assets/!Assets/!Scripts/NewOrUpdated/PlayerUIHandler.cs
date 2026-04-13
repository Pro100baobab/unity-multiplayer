using TMPro;
using FishNet.Object;
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
    private bool _isLocal;

    private void Awake()
    {
        _shooting = GetComponent<PlayerShooting>();
        _playerNetwork = GetComponent<PlayerNetwork>();
    }

    public override void OnStartNetwork()
    {
        base.OnStartNetwork();
        _isLocal = base.Owner.IsLocalClient;

        if (!_isLocal)
        {
            enabled = false;
            if (_ammoText) _ammoText.gameObject.SetActive(false);
            if (_ammoImage) _ammoImage.gameObject.SetActive(false);
            return;
        }

        // Подписка на изменения SyncVar
        _playerNetwork.Ammo.OnChange += OnAmmoChanged;
        _playerNetwork.IsAlive.OnChange += OnIsAliveChanged;

        // Начальные значения
        UpdateAmmo(_playerNetwork.Ammo.Value);
        _deathTime = Time.time;
    }

    public override void OnStopNetwork()
    {
        base.OnStopNetwork();
        if (_playerNetwork != null)
        {
            _playerNetwork.Ammo.OnChange -= OnAmmoChanged;
            _playerNetwork.IsAlive.OnChange -= OnIsAliveChanged;
        }
    }

    private void Update()
    {
        if (!_isLocal) return;

        if (_shooting != null && _ammoText != null)
            _ammoText.text = $"{_shooting.GetCurrentAmmo()}";

        if (_playerNetwork != null && !_playerNetwork.IsAlive.Value && _respawnTimerText != null)
        {
            float elapsed = Time.time - _deathTime;
            float remaining = Mathf.Max(0, 3f - elapsed);
            _respawnTimerText.text = $"Respawn: {remaining:F1}s";
        }
        else if (_respawnTimerText != null)
        {
            _respawnTimerText.text = "";
        }
    }

    private void OnIsAliveChanged(bool oldValue, bool newValue, bool asServer)
    {
        if (!newValue) _deathTime = Time.time;
    }

    private void OnAmmoChanged(int oldValue, int newValue, bool asServer)
    {
        UpdateAmmo(newValue);
    }

    private void UpdateAmmo(int ammo)
    {
        if (_ammoText != null) _ammoText.text = $"{ammo}";
    }
}