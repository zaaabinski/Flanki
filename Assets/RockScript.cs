using Unity.Netcode;
using UnityEngine;

public class RockScript : NetworkBehaviour
{
    // Let server know if the can was hit
    private NetworkVariable<bool> hitCan = new NetworkVariable<bool>(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private void Start()
    {
        if (IsServer)
        {
            Invoke(nameof(DestroyMe), 4f); // only server schedules the turn/end
        }
    }

    private void DestroyMe()
    {
        if (!hitCan.Value)
        {
            // Only server ends the turn and despawns
            GameplayScript.instance.EndPlayerTurn();
            NetworkObject.Despawn(true);
        }
        else
        {
            // Rock hit the can, just despawn
            NetworkObject.Despawn(true);
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        if (!IsServer) return; // only server updates hitCan

        if (other.gameObject.CompareTag("Can"))
        {
            hitCan.Value = true; // server authoritative
            Debug.Log("🍺 Beer hit! Turn will not end.");
            NetworkObject.Despawn(true); // optional: destroy immediately
        }
    }
}