using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerNetwork : NetworkBehaviour
{
    public NetworkVariable<FixedString32Bytes> Nickname = new(
        default,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    public NetworkVariable<int> HP = new(
        100,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    public NetworkVariable<bool> IsAlive = new(
        true,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    public NetworkVariable<int> Ammo = new(
        10,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    [SerializeField] private List<Transform> _spawnPoints;

    public override void OnNetworkSpawn()
    {
        if(_spawnPoints == null || _spawnPoints.Count == 0)
        {
            _spawnPoints = new List<Transform>();

            GameObject[] respawnObjects = GameObject.FindGameObjectsWithTag("Respawn");
            foreach (GameObject point in respawnObjects)
                _spawnPoints.Add(point.transform);
        }


        HP.OnValueChanged += OnHpChanged;
        IsAlive.OnValueChanged += OnIsAliveChanged;

        if (IsServer)
        {
            Ammo.Value = 10; // максимальное количество патронов

            if (_spawnPoints.Count > 0)
            {
                int idx = Random.Range(0, _spawnPoints.Count);
                transform.position = _spawnPoints[idx].position;
            }
            else
            {
                transform.position = Vector3.zero;
            }
        }

        if (IsOwner)
        {
            SubmitNicknameServerRpc(ConnectionUI.PlayerNickname);
        }

        if (OwnerClientId != 0)
            transform.Rotate(Vector3.up, 180);

    }

    public override void OnNetworkDespawn()
    {
        HP.OnValueChanged -= OnHpChanged;
        IsAlive.OnValueChanged -= OnIsAliveChanged;
    }

    private void OnHpChanged(int prev, int next)
    {
        if (!IsServer) return;

        if (next <= 0 && IsAlive.Value)
        {
            IsAlive.Value = false;
            StartCoroutine(RespawnRoutine());
        }
    }

    private void OnIsAliveChanged(bool prev, bool next)
    {
        if (next == false)
        {
            HidePlayer();
        }
        else
        {
            ShowPlayer();
        }
    }

    private void HidePlayer()
    {
        CharacterController cc = GetComponent<CharacterController>();
        if (cc != null)
        {
            cc.enabled = false;
        }
    }
    private void ShowPlayer()
    {
        CharacterController cc = GetComponent<CharacterController>();
        if (cc != null)
        {
            cc.enabled = true;
        }
    }

    private IEnumerator RespawnRoutine()
    {
        yield return new WaitForSeconds(3f);

        int idx = Random.Range(0, _spawnPoints.Count);
        Vector3 newPosition = _spawnPoints[idx].transform.position;

        TPPlayerClientRpc(newPosition);

        HP.Value = 100;
        Ammo.Value = 10;
        IsAlive.Value = true;
    }

    [ClientRpc]
    private void TPPlayerClientRpc(Vector3 spawnPosition)
    {
        if (IsOwner)
        {
            transform.position = spawnPosition;
        }
    }


    [ServerRpc(RequireOwnership = false)]
    private void SubmitNicknameServerRpc(string nickname)
    {
        string safeValue = string.IsNullOrWhiteSpace(nickname) ? $"Player_{OwnerClientId}" : nickname.Trim();
        if (safeValue.Length > 31) safeValue = safeValue.Substring(0, 31);
        Nickname.Value = new FixedString32Bytes(safeValue);
    }
}