using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private string gameSceneName = "Game";
    [SerializeField] private GameObject tempCamera;
    [SerializeField] private GameObject tempPanel;

    // 🔹 HOST BUTTON
    public void OnHostButton()
    {
        StartCoroutine(StartHostNextFrame());
    }

    // 🔹 CLIENT BUTTON
    public void OnClientButton()
    {
        StartCoroutine(StartClientNextFrame());
    }

    private IEnumerator StartHostNextFrame()
    {
        yield return null; // czekaj aż scena się wczyta

        if (NetworkManager.Singleton == null)
        {
            Debug.LogError("NetworkManager nie istnieje w scenie!");
            yield break;
        }

        if (NetworkManager.Singleton.IsListening)
        {
            Debug.LogWarning("NetworkManager działa – restartuję przed startem hosta");
            NetworkManager.Singleton.Shutdown();
            yield return null;
        }

        NetworkManager.Singleton.StartHost();
        if (tempCamera != null) tempCamera.SetActive(false);
        Debug.Log("✅ Host wystartowany");
    }

    private IEnumerator StartClientNextFrame()
    {
        yield return null;

        if (NetworkManager.Singleton == null)
        {
            Debug.LogError("NetworkManager nie istnieje w scenie!");
            yield break;
        }

        if (NetworkManager.Singleton.IsListening)
        {
            Debug.LogWarning("NetworkManager działa – restartuję przed startem klienta");
            NetworkManager.Singleton.Shutdown();
            yield return null;
        }

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
