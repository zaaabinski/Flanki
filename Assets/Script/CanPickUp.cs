using System;
using Unity.Netcode;
using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class CanPickUp : NetworkBehaviour
{
    private bool playerInRange;
    private ulong currentPlayerId;                    // who owns this interaction
    private List<char> sequence = new List<char>();
    private int currentIndex = 0;
    private bool sequenceGenerated = false;
    private PlayerScript currentPlayer;               // reference to the interacting player
    
    [SerializeField] private TextMeshProUGUI showSequenceText;
    
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        
        
        other.gameObject.GetComponent<PlayerScript>().showSequenceObject.SetActive(true);
        showSequenceText = other.GetComponentInChildren<TextMeshProUGUI>();
        var ps = other.GetComponent<PlayerScript>();
        if (ps == null || !ps.IsOwner) return; // ✅ only the local owner triggers the mini-game

        playerInRange = true;
        currentPlayer = ps;
        currentPlayerId = ps.OwnerClientId;

        sequenceGenerated = false;
        currentIndex = 0;
        Debug.Log($"Can Pick Up now play the minigame (Player {currentPlayerId})");

        currentPlayer.hasStarted = false;

        GenerateSequence();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        var ps = other.GetComponent<PlayerScript>();
        if (ps == null || ps.OwnerClientId != currentPlayerId) return;

        playerInRange = false;
        currentIndex = 0;
        sequenceGenerated = false;
        sequence.Clear();
        currentPlayer = null;

        Debug.Log($"Player {currentPlayerId} left — sequence reset.");
    }

    private void Update()
    {
        // ✅ only the *local owner* who entered can play the minigame
        if (!playerInRange || currentPlayer == null || !currentPlayer.IsOwner || !sequenceGenerated)
            return;

        if (Input.anyKeyDown)
        {
            bool matchedAny = false;
            foreach (char c in sequence)
            {
                if (Input.GetKeyDown(c.ToString().ToLower()))
                {
                    HandleInput(c);
                    matchedAny = true;
                    break;
                }
            }

            if (!matchedAny)
                ResetSequenceProgress();
        }
    }

    private void GenerateSequence()
    {
        sequence.Clear();
        currentIndex = 0;

        string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        System.Random rnd = new System.Random();

        for (int i = 0; i < 4; i++)
        {
            char letter = alphabet[rnd.Next(0, alphabet.Length)];
            sequence.Add(letter);
        }

        sequenceGenerated = true;

        string seqStr = string.Join("-", sequence);
        showSequenceText.text = seqStr;
        Debug.Log($"[Player {currentPlayerId}] Mini-game sequence: {seqStr}");
    }

    private void HandleInput(char pressedKey)
    {
        if (pressedKey == sequence[currentIndex])
        {
            currentIndex++;
            Debug.Log($"Correct ({pressedKey}) {currentIndex}/{sequence.Count}");

            if (currentIndex >= sequence.Count)
            {
                Debug.Log($"Player {currentPlayerId} finished minigame!");
                showSequenceText.text = "";
                MiniGameFinishedServerRpc(currentPlayerId);
                ResetSequence();
            }
        }
        else
        {
            Debug.Log($"Wrong key ({pressedKey}) — restarting");
            ResetSequenceProgress();
        }
    }

    private void ResetSequenceProgress()
    {
        currentIndex = 0;
        Debug.Log("Sequence progress reset!");
    }

    private void ResetSequence()
    {
        sequenceGenerated = false;
        sequence.Clear();
        currentIndex = 0;
    }

    [ServerRpc(RequireOwnership = false)]
    private void MiniGameFinishedServerRpc(ulong senderId)
    {
        // ✅ called on the server; broadcast to all if needed
        GameplayScript.instance.MinigameStateChangeRcp();
        GameplayScript.instance.CanPositionChangeRcp();
        Debug.Log($"Mini-game completed by Player {senderId}");
    }
}
