using System;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerScript : NetworkBehaviour
{
    public int teamNumber;

    [SerializeField] private GameObject rockPrefab;
    [SerializeField] private Slider powerSlider;
    [SerializeField] private Transform arrowPointer;
    [SerializeField] private float throwPower;
    [SerializeField] private Transform pointerRotation;

    private RotateThrowArrow RTA;
    private PingPongThrowSlider PPTS;

    private bool isRotationLocked;
    private bool isPowerLocked;

    [Header("Running variables")]
    [SerializeField] private float speed = 0f;
    [SerializeField] private float speedGain = 1.1f;
    [SerializeField] private float speedLoss = 0.5f;
    [SerializeField] private float maxSpeed = 20f;
    [SerializeField] private float minSpeed = 0f;

    private bool lastInputRight = false;
    public bool hasStarted = false;

    public GameObject showSequenceObject;
    public GameObject canRotationMinigameObject;

    private bool iWasLastThrower = false;
    private bool hasEndedTurn = false;

    public NetworkVariable<float> drinkAmount = new NetworkVariable<float>(
        100f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server
    );

    private Animator animator;
    private bool wasCanDown = false;

    // lokalne flagi ownera
    private bool isDrinkingActive = false;

    private const float DRINK_PAUSE_POINT = 80f / 150f; // 80/150 klatek

    private void Start()
    {
        RTA = arrowPointer.GetComponent<RotateThrowArrow>();
        PPTS = powerSlider.GetComponent<PingPongThrowSlider>();
        animator = GetComponentInChildren<Animator>();

        if (!IsOwner)
        {
            if (powerSlider != null) powerSlider.gameObject.SetActive(false);
            if (arrowPointer != null) arrowPointer.gameObject.SetActive(false);
            if (showSequenceObject != null) showSequenceObject.SetActive(false);
            if (canRotationMinigameObject != null) canRotationMinigameObject.SetActive(false);
        }

        drinkAmount.OnValueChanged += OnDrinkChanged;
    }

    private void OnDestroy()
    {
        drinkAmount.OnValueChanged -= OnDrinkChanged;
    }

    private void OnDrinkChanged(float oldVal, float newVal)
    {
        if (IsServer && newVal <= 0f)
            ResultsGameManager.Instance.AnnounceWinnerServerRpc(OwnerClientId);
    }

    public void SetDrinkValue(float newVal)
    {
        if (IsServer)
            drinkAmount.Value = Mathf.Clamp(newVal, 0f, 100f);
        else
            UpdateDrinkValueServerRpc(newVal);
    }

    [ServerRpc(RequireOwnership = false)]
    private void UpdateDrinkValueServerRpc(float newVal)
    {
        drinkAmount.Value = Mathf.Clamp(newVal, 0f, 100f);
    }

    void Update()
    {
        if (!IsOwner) return;

        HandleMinigamesUI();

        // === RZUCANIE ===
        if (!isRotationLocked && !isPowerLocked && Input.GetKeyDown(KeyCode.Space) && IsMyTurn())
            SetArrowPointerRotation();
        else if (isRotationLocked && !isPowerLocked && Input.GetKeyDown(KeyCode.Space))
        {
            SetThrowPower();

            // synchronizacja animacji i rzutu dla wszystkich
            PlayAnimationServerRpc("Throw");
            Invoke("ThrowRockInvokeFunction",1.4f);
        }

        // === RUCH ===
        if (!IsMyTurn() && GameplayScript.instance.isCanDown.Value)
        {
            int z = (teamNumber == 1) ? -1 : 1;
            Vector3 targetPos = GameplayScript.instance.isCanMinigameFinished.Value
                ? new Vector3(0, transform.position.y, 8 * z)
                : new Vector3(0, transform.position.y, z);
            MovePlayerToPosition(targetPos);
        }

        // === LOKALNE PICIE ===
        if (isDrinkingActive)
        {
            AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(0);
            if (info.IsName("Drinking"))
            {
                if (info.normalizedTime >= DRINK_PAUSE_POINT && animator.speed > 0f)
                {
                    animator.speed = 0f; // pauza picia
                }

                // jeśli animacja już się skończyła, wróć do Idle
                if (info.normalizedTime >= 1f)
                {
                    animator.speed = 1f;
                    isDrinkingActive = false;
                    PlayAnimationServerRpc("Idle"); // lokalny Idle synchronizowany
                }
            }
        }
    }

    private void HandleMinigamesUI()
    {
        if (!IsOwner) return;

        bool canIsDown = GameplayScript.instance.isCanDown.Value;
        bool myTurn = IsMyTurn();

        if (canRotationMinigameObject != null)
            canRotationMinigameObject.SetActive(canIsDown && myTurn);

        // picie uruchamia się tylko jeśli ja byłem ostatnim rzucającym
        if (canIsDown && myTurn && iWasLastThrower && !isDrinkingActive)
        {
            isDrinkingActive = true;
            PlayAnimationLocal("StartDrinking");
        }

        // kończymy picie, gdy can nie jest już na dole
        if ((!canIsDown || !myTurn) && isDrinkingActive)
        {
            FinishDrinkingAnimation();
            wasCanDown = false;
            iWasLastThrower = false;
        }

        wasCanDown = canIsDown;
    }


    private void MovePlayerToPosition(Vector3 pos)
    {
        speed = CalculatePlayerSpeed();
        float currentSpeed = Mathf.Min(speed, maxSpeed);

        transform.position = Vector3.MoveTowards(transform.position, pos, currentSpeed * Time.deltaTime);

        bool isMoving = Vector3.Distance(transform.position, pos) > 0.05f;

        if (isMoving) PlayAnimationServerRpc("Run");
        else PlayAnimationServerRpc("Idle");

        if (Vector3.Distance(transform.position, pos) < 0.01f)
        {
            hasStarted = false;
            lastInputRight = false;
            speed = 0;

            if (!hasEndedTurn && GameplayScript.instance.isCanMinigameFinished.Value)
            {
                hasEndedTurn = true;
                GameplayScript.instance.EndPlayerTurn();
            }
        }
        else hasEndedTurn = false;
    }

    private float CalculatePlayerSpeed()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (!hasStarted || !lastInputRight)
            {
                lastInputRight = true;
                hasStarted = true;
                speed = Mathf.Min(speed * speedGain + 0.5f, maxSpeed);
            }
            else speed *= speedLoss;
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (!hasStarted || lastInputRight)
            {
                lastInputRight = false;
                hasStarted = true;
                speed = Mathf.Min(speed * speedGain + 1f, maxSpeed);
            }
            else speed *= speedLoss;
        }

        speed = Mathf.Max(speed - Time.deltaTime * 0.5f, minSpeed);
        return speed;
    }

    public void SwapTurns()
    {
        if (!IsOwner) return;

        bool myTurn = IsMyTurn();

        arrowPointer.gameObject.SetActive(myTurn);
        powerSlider.gameObject.SetActive(false);
        showSequenceObject.SetActive(myTurn);

        if (canRotationMinigameObject != null)
            canRotationMinigameObject.SetActive(myTurn && GameplayScript.instance.isCanDown.Value);

        isRotationLocked = false;
        isPowerLocked = false;
        PPTS.moveSlider = true;
        RTA.shouldRotate = true;
    }

    public bool IsMyTurn()
    {
        return (GameplayScript.instance.GetTeamTurn() && teamNumber == 1) ||
               (!GameplayScript.instance.GetTeamTurn() && teamNumber == 2);
    }

    private void SetArrowPointerRotation()
    {
        pointerRotation = arrowPointer.transform;
        RTA.shouldRotate = false;
        powerSlider.gameObject.SetActive(true);
        isRotationLocked = true;
    }

    private void SetThrowPower()
    {
        throwPower = powerSlider.value;
        isPowerLocked = true;
        PPTS.moveSlider = false;
        iWasLastThrower = true;
    }

    public void ThrowRockInvokeFunction()
    {
        ThrowRockServerRpc(arrowPointer.position, arrowPointer.rotation, throwPower);
    }
    
    [ServerRpc]
    private void ThrowRockServerRpc(Vector3 position, Quaternion rotation, float power)
    {
        GameObject rock = Instantiate(rockPrefab, position, rotation);
        rock.GetComponent<NetworkObject>().Spawn();
        Vector3 force = (rotation * Vector3.forward * (10f * power)) + Vector3.up * (2f * power);
        rock.GetComponent<Rigidbody>().AddForce(force, ForceMode.Impulse);
    }

    [ServerRpc(RequireOwnership = false)]
    public void ResetPlayerStateServerRpc(ServerRpcParams rpcParams = default)
    {
        drinkAmount.Value = 100f;
        hasStarted = false;
        isRotationLocked = false;
        isPowerLocked = false;
        lastInputRight = false;
        hasEndedTurn = false;
        iWasLastThrower = false;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ResetPlayerStateServerRpc();
    }

    // -----------------------
    // RPC synchronizujące animacje (Throw/Run/Idle)
    // -----------------------
    [ServerRpc(RequireOwnership = false)]
    private void PlayAnimationServerRpc(string animationCommand)
    {
        PlayAnimationClientRpc(animationCommand);
    }

    [ClientRpc]
    private void PlayAnimationClientRpc(string animationCommand)
    {
        if (animator == null) return;

        switch (animationCommand)
        {
            case "Throw":
                animator.SetTrigger("Throw");
                break;
            case "Run":
                animator.SetTrigger("Run");
                break;
            case "Idle":
                animator.SetTrigger("Idle");
                break;
        }
    }

    // -----------------------
    // animacje lokalne (Drinking)
    // -----------------------
    private void PlayAnimationLocal(string command)
    {
        if (animator == null) return;

        switch (command)
        {
            case "StartDrinking":
                animator.speed = 1f;
                animator.Play("Drinking", 0, 0f);
                break;
            case "FinishDrinking":
                animator.speed = 1f;
                PlayAnimationServerRpc("Idle"); // zakończenie picia = Idle
                break;
        }
    }

    private void FinishDrinkingAnimation()
    {
        isDrinkingActive = false;
        animator.speed = 1f;
        PlayAnimationServerRpc("Idle");
    }
}
