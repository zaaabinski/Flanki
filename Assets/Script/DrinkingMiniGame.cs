using UnityEngine;
using UnityEngine.UI;

public class DrinkingMiniGame : MonoBehaviour
{
    public RectTransform canImage;
    public Slider drinkSlider;

    public float pullDownSpeed = 60f;
    public float impulseAngle = 20f;
    public float impulseSpeed = 200f;
    public float maxTilt = 90f;
    public float blockTime = 2f;
    public float maxDrainSpeed = 50f;

    private float currentTilt = 0f;
    private float targetTilt = 0f;
    private bool isBlocked = false;

    private GameplayScript gameplay;
    private PlayerScript player;

    void Start()
    {
        gameplay = GameplayScript.instance;
        player = GetComponentInParent<PlayerScript>();

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
        canImage.gameObject.SetActive(gameplay.isCanDown.Value && myTurn);

        // 🔹 Aktualizuj lokalny slider na podstawie networkowej wartości
        drinkSlider.value = player.drinkAmount.Value;

        if (isBlocked || !canImage.gameObject.activeSelf) return;

        // Sterowanie
        if (Input.GetKeyDown(KeyCode.A)) targetTilt -= impulseAngle;
        if (Input.GetKeyDown(KeyCode.D)) targetTilt += impulseAngle;

        targetTilt += pullDownSpeed * Time.deltaTime * Mathf.Sign(targetTilt);
        currentTilt = Mathf.MoveTowards(currentTilt, targetTilt, impulseSpeed * Time.deltaTime);
        currentTilt = Mathf.Clamp(currentTilt, -maxTilt, maxTilt);
        targetTilt = Mathf.Clamp(targetTilt, -maxTilt, maxTilt);

        canImage.localRotation = Quaternion.Euler(0f, 0f, currentTilt);

        // Oblicz tempo spadku płynu
        float drainSpeed = (Mathf.Abs(currentTilt) / maxTilt) * maxDrainSpeed;
        float newValue = player.drinkAmount.Value - drainSpeed * Time.deltaTime;

        // 🔹 Aktualizuj wartość przez sieć
        player.SetDrinkValue(newValue);

        if (Mathf.Abs(currentTilt) >= maxTilt)
            StartCoroutine(BlockDrinking());
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
