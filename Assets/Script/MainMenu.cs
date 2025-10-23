using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine.UI;
using TMPro; // For TextMeshPro

public class MainMenu : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private InputField ipInputField; 
    [SerializeField] private TextMeshProUGUI hostIpText; // TMP text to show host IP
    [SerializeField] private string gameSceneName = "Game";
    [SerializeField] private GameObject tempCamera;
    [SerializeField] private GameObject tempPanel;

    private const string PlayerPrefKey_IP = "LastIP";

    // ----------------------------------------------------------------------------------------------------
    // 🔹 HOST BUTTON
    public void OnHostButton()
    {
        StartCoroutine(StartHostNextFrame());
    }

    // 🔹 CLIENT BUTTON
    public void OnClientButton()
    {
        string ipAddress = ipInputField.text;

        // Check if there is a saved IP in PlayerPrefs
        if (string.IsNullOrEmpty(ipAddress))
        {
            if (PlayerPrefs.HasKey(PlayerPrefKey_IP))
            {
                ipAddress = PlayerPrefs.GetString(PlayerPrefKey_IP);
                Debug.Log($"♻️ Using saved IP: {ipAddress}");
                ipInputField.text = ipAddress; // Show it in input field
            }
            else
            {
                Debug.LogError("‼️ Wprowadź adres IP hosta, aby się połączyć!");
                return;
            }
        }
        else
        {
            // Player typed a new IP → save it
            PlayerPrefs.SetString(PlayerPrefKey_IP, ipAddress);
            PlayerPrefs.Save();
            Debug.Log($"💾 New IP saved: {ipAddress}");
        }

        SetConnectionAddress(ipAddress);
        StartCoroutine(StartClientNextFrame());
    }

    // ----------------------------------------------------------------------------------------------------
    private void SetConnectionAddress(string ip)
    {
        if (NetworkManager.Singleton.NetworkConfig.NetworkTransport is UnityTransport utp)
        {
            utp.ConnectionData.Address = ip;
            Debug.Log($"✅ Adres klienta ustawiony na: {ip}");
        }
        else
        {
            Debug.LogError("Network Transport nie jest Unity Transport (UTP)! Nie można ustawić adresu.");
        }
    }

    // ----------------------------------------------------------------------------------------------------
    private IEnumerator StartHostNextFrame()
    {
        yield return null;
        if (NetworkManager.Singleton == null) yield break;
        if (NetworkManager.Singleton.IsListening) yield return null;

        NetworkManager.Singleton.StartHost();

        // Show host IP in TMP
        if (hostIpText != null)
        {
            string localIP = "127.0.0.1"; // Default fallback
            try
            {
                var hostData = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());
                foreach (var ip in hostData.AddressList)
                {
                    if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    {
                        localIP = ip.ToString();
                        break;
                    }
                }
            }
            catch { }
            hostIpText.text = $"Host IP: {localIP}";
        }

        if (tempCamera != null) tempCamera.SetActive(false);
        Debug.Log("✅ Host wystartowany");
    }

    private IEnumerator StartClientNextFrame()
    {
        yield return null;
        if (NetworkManager.Singleton == null) yield break;
        if (NetworkManager.Singleton.IsListening) yield return null;

        NetworkManager.Singleton.StartClient();
        if (tempCamera != null) tempCamera.SetActive(false);
        
        Debug.Log("✅ Client wystartowany");
    }

    private void HideMenu()
    {
        tempPanel.SetActive(false);
    }

    public void LoadMenu() => SceneManager.LoadScene("Menu");
    public void Play() => SceneManager.LoadScene("Game");
}
