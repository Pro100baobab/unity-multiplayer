using FishNet.Object;
using UnityEngine;

public class PlayerAppearance : NetworkBehaviour
{
    [SerializeField] private Material _hostMaterial;
    [SerializeField] private Material _clientMaterial;
    private Renderer _renderer;

    private void Awake()
    {
        _renderer = GetComponent<Renderer>();
    }

    public override void OnStartNetwork()
    {
        base.OnStartNetwork();
        if (_renderer == null) return;
        _renderer.material = (base.OwnerId == 0) ? _hostMaterial : _clientMaterial;
    }
}