using UnityEngine;
using Unity.Netcode;

public class PlayerAppearance : NetworkBehaviour
{
    [SerializeField] private Material _hostMaterial;
    [SerializeField] private Material _clientMaterial;
    private Renderer _renderer;

    private void Awake()
    {
        _renderer = GetComponent<Renderer>();
    }

    public override void OnNetworkSpawn()
    {
        if (_renderer == null) return;

        if (OwnerClientId == 0)
            _renderer.material = _hostMaterial;
        else
            _renderer.material = _clientMaterial;
    }
}