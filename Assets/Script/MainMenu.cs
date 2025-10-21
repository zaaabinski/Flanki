using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine.UI; // WAŻNE: Dodaj to do obsługi UI

public class MainMenu : MonoBehaviour
{
    [Header("UI References")]
    // Przypisz w Inspektorze to pole tekstowe, gdzie klient wpisuje IP
    [SerializeField] private InputField ipInputField; 
    
    // ... (pozostałe Twoje pola)
    [SerializeField] private string gameSceneName = "Game";
    [SerializeField] private GameObject tempCamera;
    [SerializeField] private GameObject tempPanel;

// ----------------------------------------------------------------------------------------------------

    // 🔹 HOST BUTTON
    public void OnHostButton()
    {
        // Host użyje domyślnego adresu (np. 0.0.0.0), aby nasłuchiwać na wszystkich interfejsach
        StartCoroutine(StartHostNextFrame());
    }

    // 🔹 CLIENT BUTTON
    public void OnClientButton()
    {
        // 1. Sprawdzamy i ustawiamy adres z pola tekstowego
        string ipAddress = ipInputField.text;

        if (string.IsNullOrEmpty(ipAddress))
        {
            Debug.LogError("‼️ Wprowadź adres IP hosta, aby się połączyć!");
            return;
        }

        SetConnectionAddress(ipAddress);
        
        // 2. Rozpoczynamy połączenie
        StartCoroutine(StartClientNextFrame());
    }

// ----------------------------------------------------------------------------------------------------
    
    /// <summary>
    /// Ustawia adres IP, z którym klient spróbuje się połączyć.
    /// </summary>
    private void SetConnectionAddress(string ip)
    {
        // Sprawdza, czy używamy Unity Transport (UTP)
        if (NetworkManager.Singleton.NetworkConfig.NetworkTransport is UnityTransport utp)
        {
            utp.ConnectionData.Address = ip;
            Debug.Log($"✅ Adres klienta ustawiony na: {ip}");
        }
        else
        {
            // Opcjonalnie: obsługa błędu, jeśli używasz innego transportu
            Debug.LogError("Network Transport nie jest Unity Transport (UTP)! Nie można ustawić adresu.");
        }
    }

// ----------------------------------------------------------------------------------------------------
    
    // ... (pozostałe funkcje StartHostNextFrame, StartClientNextFrame itd. pozostają bez zmian)
    private IEnumerator StartHostNextFrame()
    {
        // ... (Twój obecny kod dla hosta)
        yield return null; 
        if (NetworkManager.Singleton == null) { /*...*/ yield break; }
        if (NetworkManager.Singleton.IsListening) { /*...*/ yield return null; }

        NetworkManager.Singleton.StartHost();
        if (tempCamera != null) tempCamera.SetActive(false);
        Debug.Log("✅ Host wystartowany");
    }

    private IEnumerator StartClientNextFrame()
    {
        // ... (Twój obecny kod dla klienta)
        yield return null;
        if (NetworkManager.Singleton == null) { /*...*/ yield break; }
        if (NetworkManager.Singleton.IsListening) { /*...*/ yield return null; }

        // StartClient użyje adresu ustawionego wcześniej w SetConnectionAddress()
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