/* Ethan Gapic-Kott, 000923124*/

using UnityEngine;

public class RampBoost : MonoBehaviour
{
    [Header("Boost")]
    public float forwardBoost = 30000f;
    public float upwardBoost = 8000f;

    [Header("Speed (km/h)")]
    public float minSpeed = 40f;
    public float maxSpeed = 60f;

    public enum RampColorType { Purple, Gold }
    public RampColorType rampColor = RampColorType.Purple;

    private void OnTriggerEnter(Collider other)
    {
        // Triggers boost for the player
        Rigidbody rb = other.GetComponentInParent<Rigidbody>();
        if (rb == null || !rb.CompareTag("Player")) return;

        float currentSpeed = rb.velocity.magnitude * 3.6f;

        // Applies boost if the speed is in the range
        if (currentSpeed >= minSpeed && currentSpeed <= maxSpeed)
        {
            rb.AddForce(transform.forward * forwardBoost, ForceMode.Impulse);
            rb.AddForce(Vector3.up * upwardBoost, ForceMode.Impulse);
            Debug.Log($"Boost activated on {rampColor} ramp! Speed = {currentSpeed:F1} km/h");
        }
        else
        {
            Debug.Log($"Speed {currentSpeed:F1} km/h not in range ({minSpeed}-{maxSpeed}). No boost.");
        }

        // Notifies the RampHUDManager if the ramp is passed
        RampHUDManager hud = FindObjectOfType<RampHUDManager>();
        if (hud != null)
            hud.MarkRampAsPassed(this);
    }
}
