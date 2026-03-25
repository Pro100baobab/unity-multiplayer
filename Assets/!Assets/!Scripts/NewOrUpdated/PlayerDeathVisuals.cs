using Unity.Netcode;
using UnityEngine;

public class PlayerDeathVisuals : NetworkBehaviour
{
    [SerializeField] private GameObject _model;
    [SerializeField] private GameObject ui;
    [SerializeField] private GameObject alive_panel;

    private PlayerNetwork _playerNetwork;

    private void Awake()
    {
        _playerNetwork = GetComponent<PlayerNetwork>();
        _model = gameObject;
    }

    public override void OnNetworkSpawn()
    {
        if (_playerNetwork != null)
        {
            _playerNetwork.IsAlive.OnValueChanged += OnIsAliveChanged;
            OnIsAliveChanged(false, _playerNetwork.IsAlive.Value);
        }
    }

    public override void OnNetworkDespawn()
    {
        if (_playerNetwork != null)
            _playerNetwork.IsAlive.OnValueChanged -= OnIsAliveChanged;
    }

    private void OnIsAliveChanged(bool prev, bool next)
    {
        if (_model != null)
        {
            _model.GetComponent<MeshRenderer>().enabled = next;
            ui.SetActive(next);
            alive_panel.SetActive(!next);
        }
    }
}