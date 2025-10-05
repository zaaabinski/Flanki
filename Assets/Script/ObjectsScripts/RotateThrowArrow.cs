using System;
using UnityEngine;
using UnityEngine.UI; // needed for Slider

public class RotateThrowArrow : MonoBehaviour
{
    [Header("Rotation")] 
    [SerializeField] private float angle = 40f;
    [SerializeField] private float speed = 2f; 
    [SerializeField] private Vector3 axis = Vector3.up;
    public bool shouldRotate = true;
    private void Update()
    {
        if (shouldRotate)
            Rotate();
    }

    private void Rotate()
    {
        float rotation = Mathf.Sin(Time.time * speed) * angle;
        transform.localRotation = Quaternion.AngleAxis(rotation, axis);
    }
    
}
