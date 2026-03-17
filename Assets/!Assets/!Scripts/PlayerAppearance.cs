using UnityEngine;
using Unity.Netcode;

public class PlayerAppearance : NetworkBehaviour
{
    [SerializeField] private Material _hostMaterial;   // красный для Host
    [SerializeField] private Material _clientMaterial; // синий для Client
    private Renderer _renderer;

    private void Awake()
    {
        _renderer = GetComponent<Renderer>();
    }

    public override void OnNetworkSpawn()
    {
        if (_renderer == null) return;

        // OwnerClientId == 0 соответствует Host (первый подключившийся)
        if (OwnerClientId == 0)
            _renderer.material = _hostMaterial;
        else
            _renderer.material = _clientMaterial;
    }
}