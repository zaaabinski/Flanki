using Unity.Netcode;
using UnityEngine;
using System.Collections;

public class RockScript : NetworkBehaviour
{
    private NetworkVariable<bool> hitCan = new NetworkVariable<bool>(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private void Start()
    {
        if (IsServer)
        {
            Invoke(nameof(DestroyMe), 4f);
        }
    }

    private void DestroyMe()
    {
        if (!hitCan.Value)
        {
            GameplayScript.instance.EndPlayerTurn();
        }

        NetworkObject.Despawn(true);
    }

    private void OnCollisionEnter(Collision other)
    {
        if (!IsServer) return;

        if (other.gameObject.CompareTag("Can"))
        {
            hitCan.Value = true;
            Debug.Log("🍺 Beer hit! Checking if it falls...");

            // Start coroutine to check if can actually fell after 1 second
            StartCoroutine(CheckIfCanFell());
        }
    }

    private IEnumerator CheckIfCanFell()
    {
        Debug.Log("Working");
        yield return new WaitForSeconds(1.5f);
        Debug.Log("Working");

        if (!GameplayScript.instance.isCanDown.Value)
        {
            Debug.Log("⚠️ Can was hit but didn't fall — ending turn.");
            GameplayScript.instance.EndPlayerTurn();
        }

        NetworkObject.Despawn(true);
    }
}