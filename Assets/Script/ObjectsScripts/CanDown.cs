using System;
using UnityEngine;

public class CanDown : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Ground"))
        {
            GameplayScript.instance.CanDownStateChangeRcp();
            Debug.Log("Can down");
        }
    }
}
