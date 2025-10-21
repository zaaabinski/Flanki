using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class DrinkingMiniGame : NetworkBehaviour
{
    [Header("UI Elements")]
    public RectTransform canImage;
    public Slider drinkSlider;

    [Header("Drinking Settings")]
    public float pullDownSpeed = 60f;      // naturalny pull puszki
    public float impulseAngle = 20f;       // tilt przy naciśnięciu klawisza
    public float impulseSpeed = 200f;      // szybkość wyrównania tilt
    public float maxTilt = 90f;            // maksymalne wychylenie puszki
    public float blockTime = 2f;           // czas blokady po maksymalnym tilt
    public float maxDrainSpeed = 50f;      // maksymalna prędkość picia (na sekundę)

    private float currentTilt = 0f;
    private float targetTilt = 0f;
    private bool isBlocked = false;

    private PlayerScript player;
    private GameplayScript gameplay;

    void Start()
    {
        player = GetComponentInParent<PlayerScript>();
        gameplay = GameplayScript.instance;

        if (drinkSlider != null)
        {
            drinkSlider.minValue = 0f;
            drinkSlider.maxValue = 100f;
        }

        if (canImage != null)
            canImage.gameObject.SetActive(false);
    }

    void Update()
    {
        if (player == null || gameplay == null || drinkSlider == null) return;

        bool myTurn = player.IsOwner && player.IsMyTurn();
        bool canDown = gameplay.isCanDown.Value && myTurn;

        // 🔹 Pokaż puszkę tylko właścicielowi, gdy jego tura i can jest w dół
        if (canImage != null)
            canImage.gameObject.SetActive(canDown);

        // 🔹 Zawsze aktualizujemy slider z NetworkVariable
        drinkSlider.value = player.drinkAmount.Value;

        if (!myTurn || isBlocked || !canDown) return;

        // -----------------------------
        // 1️⃣ Sterowanie puszką (lokalne)
        // -----------------------------
        if (Input.GetKeyDown(KeyCode.A)) targetTilt -= impulseAngle;
        if (Input.GetKeyDown(KeyCode.D)) targetTilt += impulseAngle;

        // naturalny pull puszki
        targetTilt += pullDownSpeed * Time.deltaTime * Mathf.Sign(targetTilt);

        // smooth tilt
        currentTilt = Mathf.MoveTowards(currentTilt, targetTilt, impulseSpeed * Time.deltaTime);
        currentTilt = Mathf.Clamp(currentTilt, -maxTilt, maxTilt);
        targetTilt = Mathf.Clamp(targetTilt, -maxTilt, maxTilt);

        if (canImage != null)
            canImage.localRotation = Quaternion.Euler(0f, 0f, currentTilt);

        // -----------------------------
        // 2️⃣ Wysyłamy tilt do serwera (tylko właściciel)
        // -----------------------------
        if (IsOwner)
        {
            SendTiltServerRpc(currentTilt);
        }

        // 🔹 Blokada przy maksymalnym tilt
        if (Mathf.Abs(currentTilt) >= maxTilt && !isBlocked)
        {
            StartCoroutine(BlockDrinking());
        }
    }

    // -----------------------------
    // Serwer aktualizuje NetworkVariable drinkAmount
    // -----------------------------
    [ServerRpc]
    private void SendTiltServerRpc(float tilt)
    {
        if (player == null) return;

        // Obliczamy prędkość picia (drain) z tilt
        float drainSpeed = (Mathf.Abs(tilt) / maxTilt) * maxDrainSpeed;

        // odejmujemy od wartości gracza (Time.deltaTime serwera)
        float newValue = player.drinkAmount.Value - drainSpeed * Time.deltaTime;
        player.drinkAmount.Value = Mathf.Clamp(newValue, 0f, 100f);
    }

    private System.Collections.IEnumerator BlockDrinking()
    {
        isBlocked = true;
        yield return new WaitForSeconds(blockTime);
        isBlocked = false;
        currentTilt = 0f;
        targetTilt = 0f;
    }
}
