using FishNet.Object;
using UnityEngine;

public class PlayerShooting : NetworkBehaviour
{
    [SerializeField] private GameObject _projectilePrefab;
    [SerializeField] private Transform _firePoint;
    [SerializeField] private float _cooldown = 1f;
    [SerializeField] private int _maxAmmo = 10;

    private float _lastShotTime;
    private PlayerNetwork _playerNetwork;

    public override void OnStartNetwork()
    {
        base.OnStartNetwork();
        _playerNetwork = GetComponent<PlayerNetwork>();

        if (base.IsServerInitialized)
            _playerNetwork.Ammo.Value = _maxAmmo;
    }

    private void Update()
    {
        if (!base.IsOwner) return;
        if (!_playerNetwork.IsAlive.Value) return;
        if (!Input.GetKeyDown(KeyCode.Space)) return;

        ShootServer(_firePoint.position, _firePoint.forward);
    }

    [ServerRpc]
    private void ShootServer(Vector3 pos, Vector3 dir)
    {
        if (_playerNetwork.HP.Value <= 0) return;
        if (_playerNetwork.Ammo.Value <= 0) return;
        if (Time.time < _lastShotTime + _cooldown) return;

        _lastShotTime = Time.time;
        _playerNetwork.Ammo.Value--;

        GameObject go = Instantiate(_projectilePrefab, pos + dir * 1.2f, Quaternion.LookRotation(dir));
        base.Spawn(go, base.Owner); // передаём владельца
    }

    public int GetCurrentAmmo() => _playerNetwork != null ? _playerNetwork.Ammo.Value : 0;
}