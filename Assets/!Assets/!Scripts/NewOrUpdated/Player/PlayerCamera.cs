using FishNet.Object;
using UnityEngine;

public class PlayerCamera : NetworkBehaviour
{
    [SerializeField] private Vector3 _offset = new(0f, 2f, -6f);
    private Camera _cam;
    private bool _isLocal;

    public override void OnStartNetwork()
    {
        base.OnStartNetwork();
        _isLocal = base.Owner.IsLocalClient;
        if (!_isLocal)
        {
            enabled = false;
            return;
        }
        _cam = Camera.main;
    }

    private void LateUpdate()
    {
        if (_cam == null) return;
        _cam.transform.position = transform.position + _offset;
        _cam.transform.LookAt(transform.position);
    }
}