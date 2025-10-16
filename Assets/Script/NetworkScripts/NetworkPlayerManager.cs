using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkPlayerManager : NetworkBehaviour
{
    public static NetworkPlayerManager Instance { get; private set; }

    public Dictionary<ulong, GameObject> ConnectedPlayers { get; private set; } = new();
    [Header("Max 6 Players")]
    public GameObject[] connectedPlayerTable = new GameObject[6];

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
    }

    public override void OnNetworkSpawn()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;

        foreach (var kvp in NetworkManager.Singleton.ConnectedClients)
        {
            var playerObj = kvp.Value.PlayerObject?.gameObject;
            if (playerObj != null && !ConnectedPlayers.ContainsKey(kvp.Key))
                ConnectedPlayers[kvp.Key] = playerObj;
        }

        RefreshVisibleTable();
    }

    private void OnClientConnected(ulong clientId) => StartCoroutine(AssignPlayerWhenReady(clientId));

    private IEnumerator AssignPlayerWhenReady(ulong clientId)
    {
        while (!NetworkManager.Singleton.ConnectedClients.ContainsKey(clientId))
            yield return null;

        var playerObject = NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject;
        if (playerObject != null)
        {
            ConnectedPlayers[clientId] = playerObject.gameObject;
            RefreshVisibleTable();
        }
    }

    private void OnClientDisconnected(ulong clientId)
    {
        if (ConnectedPlayers.ContainsKey(clientId))
        {
            ConnectedPlayers.Remove(clientId);
            RefreshVisibleTable();
        }
    }

    private void RefreshVisibleTable()
    {
        for (int i = 0; i < connectedPlayerTable.Length; i++)
            connectedPlayerTable[i] = null;

        int index = 0;
        foreach (var player in ConnectedPlayers.Values)
        {
            if (index >= connectedPlayerTable.Length) break;
            connectedPlayerTable[index] = player;
            index++;
        }
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
        }
    }

    public void RestartScene()
    {
        if (!NetworkManager.Singleton.IsServer) return;

        // 🔹 Despawn wszystkich starych NetworkObjectów
        foreach (var kvp in ConnectedPlayers)
        {
            var netObj = kvp.Value?.GetComponent<NetworkObject>();
            if (netObj != null)
                netObj.Despawn(true);
        }

        ConnectedPlayers.Clear();
        RefreshVisibleTable();

        // 🔹 Restart sceny dla wszystkich klientów
        Scene currentScene = SceneManager.GetActiveScene();
        NetworkManager.Singleton.SceneManager.LoadScene(
            currentScene.name,
            LoadSceneMode.Single
        );
    }
}
