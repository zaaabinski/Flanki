using UnityEngine;
using Unity.Netcode;

public class NetworkManagerReset : MonoBehaviour
{
    private void Awake()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.Shutdown(); // zamyka hosta/clienta
            Destroy(NetworkManager.Singleton.gameObject);
            Debug.Log("🔄 NetworkManager zniszczony przy wejściu do menu");
        }
    }
}