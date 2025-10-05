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


    private bool isRotationLocked;
    private bool isPowerLocked;


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
    }

    public void SwapTruns()
    {
        if (!IsOwner)
        {
            powerSlider.gameObject.SetActive(false);
            arrowPointer.gameObject.SetActive(false);
            return;
        }
        //if true then team one turn
        bool isActive = (GameplayScript.instance.GetPlayerTurn() && teamNumber == 1)
                        || (!GameplayScript.instance.GetPlayerTurn() && teamNumber == 2);

        powerSlider.gameObject.SetActive(isActive);
        arrowPointer.gameObject.SetActive(isActive);
    }

    private void SetArrowPointerRotation()
    {
        pointerRotation = arrowPointer.transform;
        arrowPointer.gameObject.GetComponent<RotateThrowArrow>().shouldRotate = false;
        powerSlider.gameObject.SetActive(true);
        isRotationLocked = true;
    }

    private void SetThrowPower()
    {
        Debug.Log("DUPA");
        throwPower = powerSlider.value;
        isPowerLocked = true;
        powerSlider.gameObject.GetComponent<PingPongThrowSlider>().moveSlider = false;
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