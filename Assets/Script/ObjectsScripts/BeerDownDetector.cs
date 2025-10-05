using System;
using UnityEngine;

public class BeerDownDetector : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Ground"))
        {
            /*GameManager.instance.isCanDown = true;
            GameManager.instance.TurnOnDrinkText();   */
            Debug.Log("Beer down");
        }
    }
}
