using Unity.Netcode;
using UnityEngine;
using TMPro;
using UnityEngine.Rendering;
using UnityEngine.UI;
public class ResultsGameManager : NetworkBehaviour
{
    public static ResultsGameManager Instance;

    [Header("UI")] [SerializeField] private GameObject localCanvas;
    [SerializeField] private GameObject resultsPanel;      // panel w scenie
    [SerializeField] private TextMeshProUGUI resultsText;  // TMP w panelu

    private bool gameEnded = false;

    private void Awake()
    {
        Instance = this;

        if (resultsPanel != null)
            resultsPanel.SetActive(false);
        
        localCanvas = GameObject.Find("LocalCanvas");

        resultsPanel = localCanvas.transform.GetChild(0).gameObject;
        resultsText = resultsPanel.GetComponentInChildren<TextMeshProUGUI>();
    }

    // Wywołaj na serwerze/hoście, gdy ktoś wygrał
    [ServerRpc(RequireOwnership = false)]
    public void AnnounceWinnerServerRpc(ulong winnerId)
    {
        if (gameEnded) return;
        gameEnded = true;

        ulong hostId = NetworkManager.Singleton.LocalClientId;

        // Wyślij wynik do wszystkich klientów (zdalnych)
        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            // ClientRpc dla każdego klienta (w tym host też może otrzymać)
            bool isWinner = client.ClientId == winnerId;

            // Wyślij tylko do zdalnych klientów
            if (!NetworkManager.Singleton.IsHost || client.ClientId != hostId)
            {
                var rpcParams = new ClientRpcParams
                {
                    Send = new ClientRpcSendParams { TargetClientIds = new[] { client.ClientId } }
                };
                AnnounceResultClientRpc(isWinner, rpcParams);
            }
        }

        // Host pokazuje wynik lokalnie, niezależnie od RPC
        bool hostIsWinner = winnerId == hostId;
        ShowResult(hostIsWinner);
    }

    [ClientRpc]
    private void AnnounceResultClientRpc(bool isWinner, ClientRpcParams rpcParams = default)
    {
        // Każdy klient dostaje swój wynik
        ShowResult(isWinner);
    }

    private void ShowResult(bool isWinner)
    {
        Debug.Log($"[ShowResult] Called on {(IsHost ? "Host" : "Client")} - Winner: {isWinner}");

        if (resultsPanel == null)
        {
            Debug.LogWarning("Results panel nie jest przypisany w Inspectorze!");
            return;
        }

        resultsPanel.SetActive(true);
        Image panelImage = resultsPanel.GetComponent<Image>();
        if (panelImage != null)
        {
            // Zmieniamy kolor na czarny z pełną alfą
            panelImage.color = new Color(0f, 0f, 0f, 1f); // R,G,B,Alpha
        }

        if (resultsText != null)
            resultsText.text = isWinner ? "YOU WIN!" : "YOU LOSE!";
    }
}
