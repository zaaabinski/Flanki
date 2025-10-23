using System;
using UnityEngine;

public class ResetCanRotation : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void OnEnable()
    {
        gameObject.transform.localRotation = Quaternion.Euler(0,0,0);
    }
}
