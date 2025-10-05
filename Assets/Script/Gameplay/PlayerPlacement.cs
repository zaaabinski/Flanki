using System;
using UnityEngine;
using Unity.Netcode;

public class PlayerPlacement : NetworkBehaviour
{
    private bool teamsBalanced = false;
    [SerializeField] private GameObject teamOnePlacements;
    [SerializeField] private GameObject teamTwoPlacements;

    private void Update()
    {
        if (teamsBalanced) return;
        int nonNullCount = 0;
        foreach (var player in NetworkPlayerManager.Instance.connectedPlayerTable)
        {
            if (player != null) nonNullCount++;
        }

        if (nonNullCount >= 2 && !teamsBalanced)
            BalanceTeams();
    }

    private void BalanceTeams()
    {
        // Get the team placement points
        Transform[] teamOneSpots = teamOnePlacements.GetComponentsInChildren<Transform>();
        Transform[] teamTwoSpots = teamTwoPlacements.GetComponentsInChildren<Transform>();

        int teamOneIndex = 0;
        int teamTwoIndex = 0;

        var table = NetworkPlayerManager.Instance.connectedPlayerTable;

        for (int i = 0; i < table.Length; i++)
        {
            GameObject player = table[i];
            if (player == null) continue; // Skip empty slots

            // Alternate assignment: even index → team 1, odd index → team 2
            if (i % 2 == 0)
            {
                if (teamOneIndex + 1 < teamOneSpots.Length)
                {
                    Transform spot = teamOneSpots[teamOneIndex + 1]; // +1 because [0] is the parent itself
                    player.transform.position = new Vector3(
                        spot.position.x,
                        player.transform.position.y,
                        spot.position.z
                    );
                    player.transform.rotation = Quaternion.identity; // normal rotation
                    teamOneIndex++;
                    player.GetComponent<PlayerScript>().teamNumber = 1;
                }
            }
            else
            {
                if (teamTwoIndex + 1 < teamTwoSpots.Length)
                {
                    Transform spot = teamTwoSpots[teamTwoIndex + 1];
                    player.transform.position = new Vector3(
                        spot.position.x,
                        player.transform.position.y,
                        spot.position.z
                    );
                    player.transform.rotation = Quaternion.Euler(0, 180, 0); // rotate 180 degrees
                    teamTwoIndex++;
                    player.GetComponent<PlayerScript>().teamNumber = 2;
                }
            }
        }
        GameplayScript.instance.TurnChanged();
        teamsBalanced = true;
    }


}