using Unity.Netcode;
using UnityEngine;
using System.Collections;

public class PickupManager : MonoBehaviour
{
    [SerializeField] private GameObject _healthPickupPrefab;
    [SerializeField] private Transform[] _spawnPoints;
    [SerializeField] private float _respawnDelay = 10f;

    private bool _spawned = false;

    private void Update()
    {
        if (!_spawned && NetworkManager.Singleton.IsServer)
        {
            _spawned = true;
            SpawnAll();
        }
    }

    private void SpawnAll()
    {
        foreach (var point in _spawnPoints)
            SpawnPickup(point.position);
    }

    public void OnPickedUp(Vector3 position)
    {
        StartCoroutine(RespawnAfterDelay(position));
    }

    private IEnumerator RespawnAfterDelay(Vector3 position)
    {
        yield return new WaitForSeconds(_respawnDelay);
        SpawnPickup(position);
    }

    private void SpawnPickup(Vector3 position)
    {
        var go = Instantiate(_healthPickupPrefab, position, Quaternion.identity);
        var pickup = go.GetComponent<HealthPickup>();
        pickup.Init(this);
        go.GetComponent<NetworkObject>().Spawn();
    }
}