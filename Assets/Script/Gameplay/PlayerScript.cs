using System;
using Unity.Netcode;
using UnityEngine;
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
    
    private void Start()
    {
        RTA = arrowPointer.gameObject.GetComponent<RotateThrowArrow>();
        PPTS = powerSlider.gameObject.GetComponent<PingPongThrowSlider>();

    }

    void Update()
    {
        if (!IsOwner) return;

        // End turn
        if (Input.GetKeyDown(KeyCode.P))
        {
            GameplayScript.instance.EndPlayerTurn();
        }
        
        // Throw logic
        if (!isRotationLocked && !isPowerLocked && Input.GetKeyDown(KeyCode.Space))
        {
            SetArrowPointerRotation();
        }
        else if (isRotationLocked && !isPowerLocked && Input.GetKeyDown(KeyCode.Space))
        {
            SetThrowPower();
            ThrowRockServerRpc(arrowPointer.position, arrowPointer.rotation, throwPower);
        }

        if (IsMyTurn()&&GameplayScript.instance.isCanDown.Value)
        {
            
        }
        else if (!IsMyTurn()&&GameplayScript.instance.isCanDown.Value)
        {
            if(!GameplayScript.instance.isCanMinigameFinished.Value)
            {
                int z = 1;
                if (teamNumber == 1)
                    z *= -1;
                Vector3 position = new Vector3(0, transform.position.y, z);
                MovePlayerToPosition(position);
            }
            else
            {
                Debug.Log("MINIGAME DONE, CAN COME BACK TO START POSITION");
                int z = 1;
                if (teamNumber == 1)
                    z *= -1;
                
                Vector3 position = new Vector3(0, transform.position.y, 8 *z);
                MovePlayerToPosition(position);
            }
        }
    }

    private void MovePlayerToPosition(Vector3 pos)
    { 
        speed = CalculatePlayerSpeed();
        float currentSpeed = Mathf.Min(speed, maxSpeed);

        Vector3 targetPosition = pos;

        if (transform.position == targetPosition)
        {
            hasStarted = false;
            lastInputRight = false;
            speed = 0;
            if (GameplayScript.instance.isCanMinigameFinished.Value)
            {
                GameplayScript.instance.EndPlayerTurn();
                Debug.Log("TURN FINISHED NATURALY");
            }
        }
        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            currentSpeed * Time.deltaTime
        );
    }
    
    private float CalculatePlayerSpeed()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (!hasStarted || !lastInputRight) // correct alternation
            {
                lastInputRight = true;
                hasStarted = true;
                speed = Mathf.Min(speed * speedGain + 0.5f, maxSpeed); // accelerate
            }
            else // pressed right twice
            {
                speed *= speedLoss; // slow down
            }
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (!hasStarted || lastInputRight) // correct alternation
            {
                lastInputRight = false;
                hasStarted = true;
                speed = Mathf.Min(speed * speedGain + 1f, maxSpeed);
            }
            else // pressed left twice
            {
                speed *= speedLoss;
            }
        }

        speed = Mathf.Max(speed - Time.deltaTime * 0.5f, minSpeed);

        return speed;
    }
    
    
    public void SwapTurns()
    {
        if (!IsOwner)
        {
            powerSlider.gameObject.SetActive(false);
            arrowPointer.gameObject.SetActive(false);
            return;
        }
        
        arrowPointer.gameObject.SetActive(IsMyTurn());
        powerSlider.gameObject.SetActive(false);
        
        isRotationLocked = false;
        isPowerLocked = false;
        PPTS.moveSlider = true;
        RTA.shouldRotate = true;
    }

    private bool IsMyTurn()
    {
        return (GameplayScript.instance.GetTeamTurn() && teamNumber == 1) || (!GameplayScript.instance.GetTeamTurn() && teamNumber == 2);
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
    }

    [ServerRpc]
    private void ThrowRockServerRpc(Vector3 position, Quaternion rotation, float power)
    {
        GameObject rock = Instantiate(rockPrefab, position, rotation);
        rock.GetComponent<NetworkObject>().Spawn(); // Everyone sees it

        Vector3 forwardDir = rotation * Vector3.forward;
        Vector3 force = forwardDir * (10f * power) + Vector3.up * (2f * power);
        rock.GetComponent<Rigidbody>().AddForce(force, ForceMode.Impulse);
    }
}