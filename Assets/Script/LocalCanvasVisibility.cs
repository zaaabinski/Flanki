using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;

public class LocalCanvasVisibility : NetworkBehaviour
{
    private Canvas myCanvas;

    void Awake()
    {
        myCanvas = GetComponent<Canvas>();
    }

    public override void OnNetworkSpawn()
    {
        // 🔹 Włącz tylko, jeśli to nasz gracz
        if (!IsOwner)
        {
            myCanvas.enabled = false;
            foreach (var g in GetComponentsInChildren<Graphic>(true))
                g.enabled = false;
        }
    }
}