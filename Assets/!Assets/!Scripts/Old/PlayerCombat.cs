using Unity.Netcode;
using UnityEngine;

public class PlayerCombat : NetworkBehaviour
{
    [SerializeField] private PlayerNetwork _playerNetwork;
    [SerializeField] private int _damage = 10;

    private void Update()
    {
        if (IsOwner && Input.GetKeyDown(KeyCode.Space))
        {
            TryAttack();
        }
    }

    private void TryAttack()
    {
        if (!IsOwner) return;

        RequestAttackServerRpc(_damage);
    }

    [ServerRpc]
    private void RequestAttackServerRpc(int damage)
    {
        PlayerNetwork target = FindOtherPlayer();
        if (target == null) return;

        int newHp = Mathf.Max(0, target.HP.Value - damage);
        target.HP.Value = newHp;
    }
    private PlayerNetwork FindOtherPlayer()
    {
        // Доступ к спавн менеджеру
        foreach (var kvp in NetworkManager.Singleton.SpawnManager.SpawnedObjects)
        {
            NetworkObject obj = kvp.Value;
            if (obj == null) continue;

            PlayerNetwork player = obj.GetComponent<PlayerNetwork>();
            if (player != null && player != _playerNetwork)
            {
                return player;
            }
        }
        return null;
    }
}