using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerNetwork : NetworkBehaviour
{
    // Ќик доступен всем клиентам, измен€етс€ только сервером.
    public NetworkVariable<FixedString32Bytes> Nickname = new(
        default,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    // «доровье доступно всем, измен€етс€ сервером.
    public NetworkVariable<int> HP = new(
        100,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    public override void OnNetworkSpawn()
    {
        // ”станавливаем позицию на сервере
        if (IsServer)
        {
            if (OwnerClientId == 0)
                transform.position = new Vector3(-4, 2, 0); // Host слева
            else
                transform.position = new Vector3(4, 2, 0);  // Client справа
        }

        if (IsOwner)
        {
            SubmitNicknameServerRpc(ConnectionUI.PlayerNickname);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SubmitNicknameServerRpc(string nickname)
    {
        // Ќормализаци€ ника: если пусто, назначаем "Player_"+ClientId
        string safeValue = string.IsNullOrWhiteSpace(nickname) ? $"Player_{OwnerClientId}" : nickname.Trim();
        // FixedString32Bytes может содержать только 32 символа, обрежем при необходимости.
        if (safeValue.Length > 31) safeValue = safeValue.Substring(0, 31);
        Nickname.Value = new FixedString32Bytes(safeValue);
    }
}