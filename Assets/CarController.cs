/* Ethan Gapic-Kott, 000923124*/

using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CarController : MonoBehaviour
{
    // Car Lights
    public GameObject leftTaillight, rightTaillight;
    public GameObject headlightLeft, headlightRight;

    public float normalIntensity = 1f;
    public float normalSpotAngle = 45f;
    public float highBeamIntensity = 2f;
    public float highBeamSpotAngle = 60f;

    private bool headlightsOn = false;
    private bool highBeamsOn = false;

    private Light leftHeadlight;
    private Light rightHeadlight;

    // Rigidbody
    private Rigidbody rb;

    // Steering for the car
    private float currentSteeringAngle;
    public float maxSteerinAngle = 30f;
    private float currentBrakForce;
    private bool isBraking;
    public float brakeForce = 3000f;
    public float motorForce = 2000f;

    // Wheels of the car
    public WheelCollider frontLeftCollider, frontRightCollider, rearLeftCollider, rearRightCollider;
    public Transform frontLeftTransform, frontRightTransform, rearLeftTransform, rearRightTransform;

    // Input
    private float h;
    private float v;

    // Speed of the car
    public float currentSpeed;
    public float topSpeed = 120f;

    public driveTrain _driveTrain = driveTrain.RWD;

    private GameManager gm;

    // Gravity for grounding the player/car
    [Header("Gravity & Grounding")]
    public float extraGravity = 30f;
    public float groundClearance = 0.5f;
    public LayerMask groundLayer;
    private bool isGrounded;

    [Header("Stability")]
    public float stability = 0.5f; // How strong for the to car correct itself
    public float centerOfMassY = -0.9f; // Gravity mass for the car

    void Start()
    {
        gm = GameManager.Instance;
        rb = GetComponent<Rigidbody>();

        Vector3 com = rb.centerOfMass;
        com.y = centerOfMassY;
        rb.centerOfMass = com;

        // Drag of the car
        rb.angularDrag = 2f;

        if (headlightLeft) leftHeadlight = headlightLeft.GetComponent<Light>();
        if (headlightRight) rightHeadlight = headlightRight.GetComponent<Light>();

        if (leftHeadlight)
        {
            leftHeadlight.intensity = normalIntensity;
            leftHeadlight.spotAngle = normalSpotAngle;
        }
        if (rightHeadlight)
        {
            rightHeadlight.intensity = normalIntensity;
            rightHeadlight.spotAngle = normalSpotAngle;
        }
    }

    void Update()
    {
        // Headlight functions
        if (Input.GetKeyDown(KeyCode.H))
        {
            headlightsOn = !headlightsOn;
            if (headlightLeft) headlightLeft.SetActive(headlightsOn);
            if (headlightRight) headlightRight.SetActive(headlightsOn);
            if (!headlightsOn)
            {
                highBeamsOn = false;
                ResetLights();
            }
        }

        if (headlightsOn && Input.GetKeyDown(KeyCode.B))
        {
            highBeamsOn = !highBeamsOn;
            if (highBeamsOn) SetHighBeams();
            else ResetLights();
        }
    }

    private void FixedUpdate()
    {
        // Updates car lights
        leftTaillight.SetActive(isBraking);
        rightTaillight.SetActive(isBraking);

        PlayerInput();
        Motor();
        Steering();
        UpdateWheels();
        GroundCheck();
        ApplyExtraGravity();
        StabilizeCar();
    }

    public void HamiltonCustoms(int selectedIndex)
    {
        selectedIndex = gm != null ? gm.GetComponentInChildren<TMP_Dropdown>().value : 0;
        gm.PaintShop(selectedIndex);
    }

    private void PlayerInput()
    {
        h = Input.GetAxis("Horizontal");
        v = Input.GetAxis("Vertical");
        isBraking = Input.GetKey(KeyCode.LeftShift);
    }

    private void Motor()
    {
        if (_driveTrain == driveTrain.RWD)
        {
            rearLeftCollider.motorTorque = v * motorForce;
            rearRightCollider.motorTorque = v * motorForce;
        }
        else if (_driveTrain == driveTrain.FWD)
        {
            frontLeftCollider.motorTorque = v * motorForce;
            frontRightCollider.motorTorque = v * motorForce;
        }
        else
        {
            rearLeftCollider.motorTorque = v * motorForce;
            rearRightCollider.motorTorque = v * motorForce;
            frontLeftCollider.motorTorque = v * motorForce;
            frontRightCollider.motorTorque = v * motorForce;
        }

        currentSpeed = rb.velocity.magnitude * 3.6f; // km/h
        if (currentSpeed >= topSpeed)
            currentSpeed = topSpeed;

        float stoppingDistance = (0.278f * Time.deltaTime * rb.velocity.magnitude) +
                                 (Mathf.Pow(rb.velocity.magnitude, 2) / (274 * (0.7f + 1)));
        float brakingForce = rb.mass * (rb.velocity.magnitude / stoppingDistance);
        currentBrakForce = isBraking ? brakingForce : 0f;
        ApplyBrakes(currentBrakForce);
    }

    // Physics for the car
    void ApplyBrakes(float brakez)
    {
        frontLeftCollider.brakeTorque = brakez;
        frontRightCollider.brakeTorque = brakez;
        rearLeftCollider.brakeTorque = brakez;
        rearRightCollider.brakeTorque = brakez;
    }

    private void Steering()
    {
        currentSteeringAngle = maxSteerinAngle * h;
        frontLeftCollider.steerAngle = currentSteeringAngle;
        frontRightCollider.steerAngle = currentSteeringAngle;
    }

    private void UpdateWheels()
    {
        UpdateOneWheel(frontLeftTransform, frontLeftCollider);
        UpdateOneWheel(rearLeftTransform, rearLeftCollider);
        UpdateOneWheel(frontRightTransform, frontRightCollider);
        UpdateOneWheel(rearRightTransform, rearRightCollider);
    }

    void UpdateOneWheel(Transform t, WheelCollider c)
    {
        Vector3 pos;
        Quaternion rot;
        c.GetWorldPose(out pos, out rot);
        t.position = pos;
        t.rotation = rot;
    }

    private void GroundCheck()
    {
        RaycastHit hit;
        isGrounded = Physics.Raycast(transform.position, -transform.up, out hit, groundClearance + 0.2f, groundLayer);
    }

    private void ApplyExtraGravity()
    {
        if (!isGrounded)
            rb.AddForce(Vector3.down * extraGravity, ForceMode.Acceleration);
    }

    // Aligns the car to the ground
    private void StabilizeCar()
    {
        if (isGrounded)
        {
            Vector3 predictedUp = Quaternion.AngleAxis(rb.angularVelocity.magnitude * Mathf.Rad2Deg * stability / topSpeed, rb.angularVelocity) * transform.up;
            Vector3 torqueVector = Vector3.Cross(predictedUp, Vector3.up);
            rb.AddTorque(torqueVector * stability * stability);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == "Goal")
            gm.Lap();
        if (other.gameObject.name == "Checkpoint")
            gm.checkpointPassed = true;
    }

    private void SetHighBeams()
    {
        if (leftHeadlight)
        {
            leftHeadlight.intensity = highBeamIntensity;
            leftHeadlight.spotAngle = highBeamSpotAngle;
        }
        if (rightHeadlight)
        {
            rightHeadlight.intensity = highBeamIntensity;
            rightHeadlight.spotAngle = highBeamSpotAngle;
        }
    }

    private void ResetLights()
    {
        if (leftHeadlight)
        {
            leftHeadlight.intensity = normalIntensity;
            leftHeadlight.spotAngle = normalSpotAngle;
        }
        if (rightHeadlight)
        {
            rightHeadlight.intensity = normalIntensity;
            rightHeadlight.spotAngle = normalSpotAngle;
        }
    }
}

public enum driveTrain
{
    RWD, FWD, AWD
}
