using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NetworkPlayerManager : NetworkBehaviour
{
    public static NetworkPlayerManager Instance { get; private set; }

    // Actual player registry (runtime use)
    public Dictionary<ulong, GameObject> ConnectedPlayers { get; private set; } = new();

    // Fixed-size table for Inspector visualization
    [Header("Max 6 Players")]
    public GameObject[] connectedPlayerTable = new GameObject[6];

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public override void OnNetworkSpawn()
    {
        // Subscribe to connection events on all clients
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;

        // Add already connected players (host)
        foreach (var kvp in NetworkManager.Singleton.ConnectedClients)
        {
            var playerObj = kvp.Value.PlayerObject?.gameObject;
            if (playerObj != null && !ConnectedPlayers.ContainsKey(kvp.Key))
            {
                ConnectedPlayers[kvp.Key] = playerObj;
            }
        }

        RefreshVisibleTable();
    }

    private void OnClientConnected(ulong clientId)
    {
        StartCoroutine(AssignPlayerWhenReady(clientId));
    }

    private IEnumerator AssignPlayerWhenReady(ulong clientId)
    {
        while (!NetworkManager.Singleton.ConnectedClients.ContainsKey(clientId))
            yield return null;

        var playerObject = NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject;
        if (playerObject != null)
        {
            ConnectedPlayers[clientId] = playerObject.gameObject;
            RefreshVisibleTable();
            Debug.Log($"✅ Player connected: {clientId} ({playerObject.name})");
        }
    }

    private void OnClientDisconnected(ulong clientId)
    {
        if (ConnectedPlayers.ContainsKey(clientId))
        {
            ConnectedPlayers.Remove(clientId);
            RefreshVisibleTable();
            Debug.Log($"❌ Player disconnected: {clientId}");
        }
    }

    private void RefreshVisibleTable()
    {
        // Clear table first
        for (int i = 0; i < connectedPlayerTable.Length; i++)
        {
            connectedPlayerTable[i] = null;
        }

        // Fill table with up to 6 players
        int index = 0;
        foreach (var player in ConnectedPlayers.Values)
        {
            if (index >= connectedPlayerTable.Length) break;
            connectedPlayerTable[index] = player;
            index++;
        }

#if UNITY_EDITOR
        // Force Unity to refresh Inspector view
        UnityEditor.EditorApplication.delayCall += () =>
        {
            if (this != null)
                UnityEditor.EditorUtility.SetDirty(this);
        };
#endif
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
        }
    }

    public GameObject GetPlayer(ulong clientId)
    {
        return ConnectedPlayers.ContainsKey(clientId) ? ConnectedPlayers[clientId] : null;
    }
}
