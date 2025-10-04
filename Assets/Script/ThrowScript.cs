using System;
using UnityEngine;
using UnityEngine.UI; // needed for Slider

public class ThrowScript : MonoBehaviour
{
    [SerializeField] private GameObject rockPrefab;

    [Header("Rock variables")]
    private Quaternion throwRotation;
    private bool rotationLocked;
    private bool rockThrown;

    [Header("Rotation")] 
    [SerializeField] private float angle = 40f;
    [SerializeField] private float speed = 2f; 
    [SerializeField] private Vector3 axis = Vector3.up;

    [Header("Additional variables")] 
    [SerializeField] private Slider slider;   // use Slider type instead of GameObject
    private float throwPower;                 // store slider value here
    
    private void GetRotation()
    {
        if (!rotationLocked)
        {
            Rotate();
            if (Input.GetKeyDown(KeyCode.Space))
            {
                rotationLocked = true;
                throwRotation = gameObject.transform.rotation;
                slider.gameObject.SetActive(true);
                Debug.Log("Rotation locked: " + throwRotation);
            }
        }
        else if (!rockThrown && Input.GetKeyDown(KeyCode.Space))
        {
            GetSliderValue();
            ThrowRock();
        }
    }

    private void Update()
    {
        GetRotation();
    }
    
    private void Rotate()
    {
        float rotation = Mathf.Sin(Time.time * speed) * angle;
        transform.localRotation = Quaternion.AngleAxis(rotation, axis);
    }

    private void GetSliderValue()
    {
        throwPower = slider.value; 
        Debug.Log("Slider value at throw: " + throwPower);
        slider.GetComponent<ThrowPowerScript>().moveSlider = false;
    }

    private void ThrowRock()
    {
        GameObject rock = Instantiate(rockPrefab, transform.position, throwRotation);
        Rigidbody rb = rock.GetComponent<Rigidbody>();

        Vector3 forwardDir = throwRotation * Vector3.forward;

        // Use slider value to scale force
        Vector3 force = forwardDir * (20f * throwPower) + Vector3.up * (3f * throwPower);
        
        rb.AddForce(force, ForceMode.Impulse);
        gameObject.GetComponent<Renderer>().enabled = false;
        rockThrown = true;
    }
}
