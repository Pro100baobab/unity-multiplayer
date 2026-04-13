using FishNet.Object;
using UnityEngine;

public class PlayerDeathVisuals : NetworkBehaviour
{
    [SerializeField] private GameObject _model;
    [SerializeField] private GameObject _ui;
    [SerializeField] private GameObject _alivePanel;

    private PlayerNetwork _playerNetwork;
    private MeshRenderer _modelRenderer;

    private void Awake()
    {
        _playerNetwork = GetComponent<PlayerNetwork>();
        if (_model == null)
            _model = gameObject;
        _modelRenderer = _model.GetComponent<MeshRenderer>();
    }

    public override void OnStartNetwork()
    {
        base.OnStartNetwork();

        _playerNetwork.IsAlive.OnChange += OnIsAliveChanged;
        
        // Применяем начальное состояние
        UpdateVisuals(_playerNetwork.IsAlive.Value);
    }

    public override void OnStopNetwork()
    {
        base.OnStopNetwork();
        if (_playerNetwork != null)
            _playerNetwork.IsAlive.OnChange -= OnIsAliveChanged;
    }

    private void OnIsAliveChanged(bool oldValue, bool newValue, bool asServer)
    {
        UpdateVisuals(newValue);
    }

    private void UpdateVisuals(bool alive)
    {
        if (_modelRenderer != null)
            _modelRenderer.enabled = alive;
        if (_ui != null)
            _ui.SetActive(alive);
        if (_alivePanel != null)
            _alivePanel.SetActive(!alive);
    }
}