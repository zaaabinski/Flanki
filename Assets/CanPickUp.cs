using System;
using Unity.Netcode;
using UnityEngine;

public class CanPickUp : MonoBehaviour
{
   private bool playerInRange;
   private void OnTriggerEnter(Collider other)
   {
      if (other.tag == "Player")
      {
         playerInRange = true;
         Debug.Log("Can Pick Up now play the minigame");
         other.gameObject.GetComponent<PlayerScript>().hasStarted = false;
      }
   }

   private void OnTriggerExit(Collider other)
   {
      playerInRange = false;
   }

   private void Update()
   {
      if (playerInRange && Input.GetKeyDown(KeyCode.E))
      {
         MiniGameFinished();
      }
   }

   private void MiniGameFinished()
   {
      GameplayScript.instance.MinigameStateChangeRcp();
      GameplayScript.instance.CanPositionChangeRcp();
   }
   
}
