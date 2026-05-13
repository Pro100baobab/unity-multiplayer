using FishNet;
using UnityEngine;
using System.Collections;

public class PickupManager : MonoBehaviour
{
    [SerializeField] private GameObject _healthPickupPrefab;
    [SerializeField] private Transform[] _spawnPoints;
    [SerializeField] private float _respawnDelay = 10f;

    private bool _spawned;

    private void Start()
    {
        Debug.Log("стартуем и запускаем коррутину");

        StartCoroutine(WaitAndSpawn());
    }

    private IEnumerator WaitAndSpawn()
    {
        // Ждём пока сервер стартует
        yield return new WaitUntil(() =>
            InstanceFinder.ServerManager != null &&
            InstanceFinder.ServerManager.Started);

        if (_spawned)
            yield break;

        _spawned = true;

        foreach (Transform point in _spawnPoints)
        {
            Debug.Log("запустили цикл спавна");

            SpawnPickup(point.position);
        }
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
        GameObject go = Instantiate(_healthPickupPrefab, position, Quaternion.identity);
        go.GetComponent<HealthPickup>().Init(this);

        InstanceFinder.ServerManager.Spawn(go);

        Debug.Log("Создали объект");
    }
}