/* Ethan Gapic-Kott, 000923124*/

using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class RampHUDManager : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text rampHUDText; // UI text displaying next ramp
    public float detectionRadius = 200f;

    private Transform player;
    private List<RampBoost> passedRamps = new List<RampBoost>();

    void Start()
    {
        // Finds player car in game scene
        GameObject playerObj = GameObject.Find("PlayerCar");
        if (playerObj == null)
        {
            Debug.LogError("PlayerCar not found in scene!");
            enabled = false;
            return;
        }
        player = playerObj.transform;

        if (rampHUDText == null)
        {
            Debug.LogError("RampHUD Text not assigned in Inspector!");
            enabled = false;
            return;
        }

        rampHUDText.text = "";
    }

    void Update()
    {
        // Shows next ramp in range of the player
        RampBoost nextRamp = FindNextRamp();
        if (nextRamp == null)
        {
            rampHUDText.text = "";
            return;
        }

        float distance = Vector3.Distance(player.position, nextRamp.transform.position);
        int targetSpeed = Mathf.RoundToInt((nextRamp.minSpeed + nextRamp.maxSpeed) / 2);

        string colorHex = "#FFFFFF";
        string rampName = nextRamp.rampColor.ToString();

        if (nextRamp.rampColor == RampBoost.RampColorType.Purple) colorHex = "#A020F0";
        else if (nextRamp.rampColor == RampBoost.RampColorType.Gold) colorHex = "#FFD700";

        if (distance < detectionRadius)
        {
            rampHUDText.text =
                $"<color={colorHex}>{rampName} ramp</color> in {distance:F0} meters\n" +
                $"Drive {targetSpeed} km/h";
        }
        else
        {
            rampHUDText.text = "";
        }
    }

    RampBoost FindNextRamp()
    {
        // Finds the nearest ramp that hasnt been passed by the player
        RampBoost[] ramps = FindObjectsOfType<RampBoost>();
        float closestDist = Mathf.Infinity;
        RampBoost closestRamp = null;

        foreach (RampBoost ramp in ramps)
        {
            if (passedRamps.Contains(ramp)) continue;

            Vector3 toRamp = ramp.transform.position - player.position;

            // Only locates ramps ahead of player
            if (Vector3.Dot(player.forward, toRamp) <= 0) continue;

            float dist = toRamp.magnitude;
            if (dist < closestDist)
            {
                closestDist = dist;
                closestRamp = ramp;
            }
        }

        return closestRamp;
    }

    public void MarkRampAsPassed(RampBoost ramp)
    {
        // Adds passed ramp to a list ti hide UI text
        if (ramp != null && !passedRamps.Contains(ramp))
            passedRamps.Add(ramp);
    }
}
