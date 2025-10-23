using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;
using System.Collections;

public class GameplayScript : NetworkBehaviour
{
    public static GameplayScript instance;

    public NetworkVariable<bool> isTeamOneTurn = new NetworkVariable<bool>(true);
    public NetworkVariable<bool> isCanDown = new NetworkVariable<bool>(false);
    public NetworkVariable<bool> isCanMinigameFinished = new NetworkVariable<bool>(false);

    [SerializeField] private GameObject emptyCanPrefab;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);

        isTeamOneTurn.OnValueChanged += OnTurnChanged;
        emptyCanPrefab = GameObject.Find("Beer");
    }

    private void OnTurnChanged(bool previous, bool current)
    {
        Debug.Log($"🔄 Turn changed → {(current ? "Team One" : "Team Two")}");
        foreach (var player in NetworkPlayerManager.Instance.connectedPlayerTable)
        {
            if (player == null) continue;
            player.GetComponent<PlayerScript>().SwapTurns();
        }
    }

    public void EndPlayerTurn()
    {
        if (IsServer) ToggleTurn();
        else RequestNextTurnServerRpc();
    }

    public void MinigameStateChangeRcp()
    {
        if (IsServer) isCanMinigameFinished.Value = true;
        else RequestMinigameFinishServerRpc();
    }

    public void CanDownStateChangeRcp()
    {
        if (IsServer) isCanDown.Value = true;
        else RequestCanDownStateChangeServerRpc();
    }

    public void CanPositionChangeRcp()
    {
        if (IsServer) CanPositionReset();
        else RequestCanPositionResetServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestNextTurnServerRpc(ServerRpcParams rpcParams = default) => ToggleTurn();

    [ServerRpc(RequireOwnership = false)]
    private void RequestMinigameFinishServerRpc(ServerRpcParams rpcParams = default) => isCanMinigameFinished.Value = true;

    [ServerRpc(RequireOwnership = false)]
    private void RequestCanDownStateChangeServerRpc(ServerRpcParams rpcParams = default) => isCanDown.Value = true;

    [ServerRpc(RequireOwnership = false)]
    private void RequestCanPositionResetServerRpc(ServerRpcParams rpcParams = default) => CanPositionReset();

    private void ToggleTurn()
    {
        isTeamOneTurn.Value = !isTeamOneTurn.Value;
        isCanMinigameFinished.Value = false;
        Debug.Log($"✅ Server switched turn → {(isTeamOneTurn.Value ? "Team One" : "Team Two")}");
        CanUpBool();
        CanPositionReset();
    }

    public bool GetTeamTurn() => isTeamOneTurn.Value;

    public void CanUpBool() => isCanDown.Value = false;

    public void CanPositionReset()
    {
        if (emptyCanPrefab == null) return;
        emptyCanPrefab.transform.position = new Vector3(0, 0.33f, 0);
        emptyCanPrefab.transform.rotation = Quaternion.identity;

        Rigidbody rb = emptyCanPrefab.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }

    private void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;
    private void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!IsServer) return;

        ResetNetworkVariables();
        StartCoroutine(ResetPlayersNextFrame());
    }

    private void ResetNetworkVariables()
    {
        isTeamOneTurn.Value = true;
        isCanDown.Value = false;
        isCanMinigameFinished.Value = false;
        CanPositionReset();
    }

    private IEnumerator ResetPlayersNextFrame()
    {
        yield return null; // poczekaj jedną klatkę, gracze już spawn
        ResetAllPlayers();
    }

    private void ResetAllPlayers()
    {
        foreach (var kvp in NetworkPlayerManager.Instance.ConnectedPlayers)
        {
            var player = kvp.Value?.GetComponent<PlayerScript>();
            if (player != null)
                player.ResetPlayerStateServerRpc();
        }
    }
}
