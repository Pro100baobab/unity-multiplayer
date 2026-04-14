using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using FishNet.Connection;

public class PlayerNetwork : NetworkBehaviour
{
    [SerializeField] private List<Transform> _spawnPoints;

    public readonly SyncVar<string> Nickname = new SyncVar<string>("Player");
    public readonly SyncVar<int> HP = new SyncVar<int>(100);
    public readonly SyncVar<bool> IsAlive = new SyncVar<bool>(true);
    public readonly SyncVar<int> Ammo = new SyncVar<int>(10);

    private NetworkConnection _lastAttacker;

    public override void OnStartNetwork()
    {
        base.OnStartNetwork();

        // Подписка на изменения значений
        Nickname.OnChange += OnNicknameChanged;
        HP.OnChange += OnHpChanged;
        IsAlive.OnChange += OnIsAliveChanged;
        Ammo.OnChange += OnAmmoChanged;

        if (_spawnPoints == null || _spawnPoints.Count == 0)
        {
            _spawnPoints = new List<Transform>();
            GameObject[] respawnObjects = GameObject.FindGameObjectsWithTag("Respawn");
            foreach (GameObject point in respawnObjects)
                _spawnPoints.Add(point.transform);
        }

        if (base.IsServerInitialized)
        {
            Ammo.Value = 10;
            if (_spawnPoints.Count > 0)
                transform.position = _spawnPoints[Random.Range(0, _spawnPoints.Count)].position;
            else
                transform.position = Vector3.zero;
        }

        if (base.Owner.IsLocalClient)
            SetNicknameServer(ConnectionUI.PlayerNickname);

        if (base.OwnerId != 0)
            transform.Rotate(Vector3.up, 180);
    }

    public override void OnStopNetwork()
    {
        base.OnStopNetwork();

        Nickname.OnChange -= OnNicknameChanged;
        HP.OnChange -= OnHpChanged;
        IsAlive.OnChange -= OnIsAliveChanged;
        Ammo.OnChange -= OnAmmoChanged;
    }

    private void OnNicknameChanged(string oldValue, string newValue, bool asServer)
    {
        // UI обновляется в PlayerView
    }

    private void OnHpChanged(int oldValue, int newValue, bool asServer)
    {
        if (!asServer) return;
        if (newValue <= 0 && IsAlive.Value)
        {
            IsAlive.Value = false;

            // Начисляем очко убийце
            if (_lastAttacker != null && _lastAttacker.IsValid)
            {
                GameManager gm = NetworkManager.GetInstance<GameManager>();
                gm?.AddScore(_lastAttacker);
            }

            StartCoroutine(RespawnRoutine());
        }
    }

    private void OnIsAliveChanged(bool oldValue, bool newValue, bool asServer)
    {
        if (!newValue)
            HidePlayer();
        else
            ShowPlayer();
    }

    private void OnAmmoChanged(int oldValue, int newValue, bool asServer) { }

    private void HidePlayer()
    {
        if (TryGetComponent<CharacterController>(out var cc))
            cc.enabled = false;
    }

    private void ShowPlayer()
    {
        if (TryGetComponent<CharacterController>(out var cc))
            cc.enabled = true;
    }

    private IEnumerator RespawnRoutine()
    {
        yield return new WaitForSeconds(3f);

        int idx = Random.Range(0, _spawnPoints.Count);
        Vector3 newPosition = _spawnPoints[idx].transform.position;

        // Телепортируем на сервере
        if (base.IsServerInitialized)
        {
            transform.position = newPosition;

            if (TryGetComponent<CharacterController>(out var cc))
            {
                cc.enabled = false;
                cc.enabled = true;
            }
        }

        // Отправляем RPC всем клиентам
        TeleportPlayerObservers(newPosition);

        HP.Value = 100;
        Ammo.Value = 10;
        IsAlive.Value = true;
    }

    [ObserversRpc(BufferLast = true)]
    private void TeleportPlayerObservers(Vector3 spawnPosition)
    {
        if (!base.IsServerInitialized)
        {
            transform.position = spawnPosition;
            if (TryGetComponent<CharacterController>(out var cc))
            {
                cc.enabled = false;
                cc.enabled = true;
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetNicknameServer(string nickname)
    {
        string safeValue = string.IsNullOrWhiteSpace(nickname)
            ? $"Player_{OwnerId}"
            : nickname.Trim();
        if (safeValue.Length > 31) safeValue = safeValue[..31];
        Nickname.Value = safeValue;
    }

    /*
    // Метод для установки атакующего (вызывается из Projectile)
    [ServerRpc(RequireOwnership = false)]
    public void SetLastAttacker(NetworkConnection attacker)
    {
        _lastAttacker = attacker;
    }*/

    public void SetLastAttacker(NetworkConnection attacker)
    {
        // Если мы на сервере — устанавливаем сразу
        if (base.IsServerInitialized)
        {
            _lastAttacker = attacker;
            Debug.Log($"[PlayerNetwork] Last attacker set to client {attacker?.ClientId} for player {OwnerId}");
        }
    }
}