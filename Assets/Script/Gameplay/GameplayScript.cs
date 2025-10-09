using System;
using System.Threading.Tasks.Sources;
using Unity.Netcode;
using Unity.Netcode.Components;
using Unity.VisualScripting;
using UnityEditor.ShortcutManagement;
using UnityEngine;

public class GameplayScript : NetworkBehaviour
{
    
    public static GameplayScript instance;
    
    // Shared network variable for turn state
    public NetworkVariable<bool> isTeamOneTurn = new NetworkVariable<bool>(true);
    
    public NetworkVariable<bool> isCanDown = new NetworkVariable<bool>(false);
    
    public NetworkVariable<bool> isCanMinigameFinished = new NetworkVariable<bool>(false);
    
    [SerializeField] private GameObject emptyCanPrefab;
  
    
    private void Awake()
    {
        if(instance == null)
            instance = this;
        else 
            Destroy(gameObject);
        
        isCanMinigameFinished.Value = false;
        isTeamOneTurn.OnValueChanged += OnTurnChanged;
    }

    /*private void OnDestroy()
    {
        isTeamOneTurn.OnValueChanged -= OnTurnChanged;
    }*/

    private void OnTurnChanged(bool previous, bool current)
    {
        Debug.Log($"🔄 Turn changed → {(current ? "Team One" : "Team Two")}");

        foreach (var player in NetworkPlayerManager.Instance.connectedPlayerTable)
        {
            if(player ==null) continue; 
            player.GetComponent<PlayerScript>().SwapTurns();
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

    public void MinigameStateChangeRcp()
    {
        if(IsServer)
            isCanMinigameFinished.Value = true;
        else
            RequestMinigameFinishServerRpc();
    }

    public void CanDownStateChangeRcp()
    {
        if(IsServer)
            isCanDown.Value = true;
    }

    public void CanPositionChangeRcp()
    {
        if (IsServer)
            CanPositionReset();
        else
            RequestCanPositionResetServerRpc();
    }
    
    [ServerRpc(RequireOwnership = false)]
    private void RequestNextTurnServerRpc(ServerRpcParams rpcParams = default)
    {
        ToggleTurn();
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestMinigameFinishServerRpc(ServerRpcParams rpcParams = default)
    {
        isCanMinigameFinished.Value = true;
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestCanDownStateChangeServerRpc(ServerRpcParams rpcParams = default)
    {
        isCanDown.Value = true;
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestCanPositionResetServerRpc(ServerRpcParams rpcParams = default)
    {
        CanPositionReset();
    }
    
    
    private void ToggleTurn()
    {
        isTeamOneTurn.Value = !isTeamOneTurn.Value;
        isCanMinigameFinished.Value = false;
        Debug.Log($"✅ Server switched turn → {(isTeamOneTurn.Value ? "Team One" : "Team Two")}");
        Debug.Log("Seting up the can");
        CanUpBool();
        CanPositionReset();
    }

    public bool GetTeamTurn()
    {
        return isTeamOneTurn.Value;
    }
    
    public void CanUpBool()
    {
        isCanDown.Value = false;
    }

    public void CanPositionReset()
    {
        emptyCanPrefab.transform.position = new Vector3(0,0.25f,0);
        emptyCanPrefab.transform.rotation = Quaternion.identity;
        emptyCanPrefab.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
        emptyCanPrefab.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
    }
    
}