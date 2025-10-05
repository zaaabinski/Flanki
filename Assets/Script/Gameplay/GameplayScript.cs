using System;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class GameplayScript : NetworkBehaviour
{
    
    public static GameplayScript instance;
    
    // Shared network variable for turn state
    public NetworkVariable<bool> isTeamOneTurn = new NetworkVariable<bool>(
        true,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private void Awake()
    {
        if(instance == null)
            instance = this;
        else 
            Destroy(gameObject);
        
        // Listen for changes (clients can react to turn change)
        isTeamOneTurn.OnValueChanged += OnTurnChanged;
    }

    private void OnDestroy()
    {
        isTeamOneTurn.OnValueChanged -= OnTurnChanged;
    }

    private void OnTurnChanged(bool previous, bool current)
    {
        Debug.Log($"🔄 Turn changed → {(current ? "Team One" : "Team Two")}");

        foreach (var player in NetworkPlayerManager.Instance.connectedPlayerTable)
        {
            if(player ==null) continue; 
            player.GetComponent<PlayerScript>().SwapTruns();
        }
    }
    
    public void EndPlayerTurn()
    {
        // Ask the server to flip the turn
        if (IsServer)
        {
            ToggleTurn();
        }
        else
        {
            RequestNextTurnServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestNextTurnServerRpc(ServerRpcParams rpcParams = default)
    {
        ToggleTurn();
    }
    
    private void ToggleTurn()
    {
        isTeamOneTurn.Value = !isTeamOneTurn.Value;
        Debug.Log($"✅ Server switched turn → {(isTeamOneTurn.Value ? "Team One" : "Team Two")}");
    }

    public bool GetPlayerTurn()
    {
        return isTeamOneTurn.Value;
    }
    
    public void TurnChanged()
    {
        foreach (var player in NetworkPlayerManager.Instance.connectedPlayerTable)
        {
            if (player == null) continue;
            player.GetComponent<PlayerScript>().SwapTruns();
        }
    }
    
}