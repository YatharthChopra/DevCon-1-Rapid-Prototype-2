using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;

public class CarController : MonoBehaviour
{
    //Two taillight objects
    public GameObject leftTaillight, rightTaillight;

    //Two headlight objects
    public GameObject headlightLeft, headlightRight;

    // lowbeam headlight settings
    public float normalIntensity = 1f;
    public float normalSpotAngle = 45f;

    // Highbeam settings
    public float highBeamIntensity = 2f;
    public float highBeamSpotAngle = 60f;

    // Track the headlight state
    private bool headlightsOn = false;

    // Track whether high beams are enabled
    private bool highBeamsOn = false;

    // References to the Light components on each headlight
    private Light leftHeadlight;
    private Light rightHeadlight;

    //Rigidbody
    private Rigidbody rb;

    //Steering Stuff
    private float currentSteeringAngle;     //Variable to hold the math
    public float maxSteerinAngle;       //Visible in the editor

    //Braking Stuff
    private float currentBrakForce;
    private bool isBraking;
    public float brakeForce;

    //Motor stuff
    public float motorForce;

    //Wheels
    public WheelCollider frontLeftCollider, frontRightCollider, rearLeftCollider, rearRightCollider;

    public Transform frontLeftTransform, frontRightTransform, rearLeftTransform, rearRightTransform;

    //Input stuff, global variabls to store the input
    private float h;
    private float v;

    //Speed stuff
    public float currentSpeed;
    public float topSpeed;

    //Variables for the enum driveTrain caled _driveTrain. Default is RWD.
    //UNITY DOESN'T KNOW WHAT THIS MEANS We are going to give it meaning.
    public driveTrain _driveTrain = driveTrain.RWD;

    //GameManager variable reference
    private GameManager gm;

    //List of Colours
    public TMP_Dropdown dropdownList;

    //TO-DO - speed text

    // Start is called before the first frame update
    void Start()
    {
        //only need to refer to the Instance public method because we know that there can be only one
        gm = GameManager.Instance;
        //Initialize the rb
        rb = GetComponent<Rigidbody>();

        // Get the Light components from the headlight objects
        if (headlightLeft != null)
            leftHeadlight = headlightLeft.GetComponent<Light>();
        if (headlightRight != null)
            rightHeadlight = headlightRight.GetComponent<Light>();

        // Set initial headlight settings to normal
        if (leftHeadlight != null)
        {
            leftHeadlight.intensity = normalIntensity;
            leftHeadlight.spotAngle = normalSpotAngle;
        }
        if (rightHeadlight != null)
        {
            rightHeadlight.intensity = normalIntensity;
            rightHeadlight.spotAngle = normalSpotAngle;
        }
    }

    // Update is called once per frame
    void Update()
    {
        //TO-DO Reset Button

        // Toggle headlights when the player presses H
        if (Input.GetKeyDown(KeyCode.H))
        {
            headlightsOn = !headlightsOn;
            if (headlightLeft != null) headlightLeft.SetActive(headlightsOn);
            if (headlightRight != null) headlightRight.SetActive(headlightsOn);

            // If turning off headlights, also turn off high beams
            if (!headlightsOn)
            {
                highBeamsOn = false;
                // Optionally, reset the lights to normal settings when turned off
                if (leftHeadlight != null)
                {
                    leftHeadlight.intensity = normalIntensity;
                    leftHeadlight.spotAngle = normalSpotAngle;
                }
                if (rightHeadlight != null)
                {
                    rightHeadlight.intensity = normalIntensity;
                    rightHeadlight.spotAngle = normalSpotAngle;
                }
            }
        }

        //Toggle high beams when the player presses B (only if headlights are on)
        if (headlightsOn && Input.GetKeyDown(KeyCode.B))
        {
            highBeamsOn = !highBeamsOn;
            if (highBeamsOn)
            {
                // Activate high beam settings
                if (leftHeadlight != null)
                {
                    leftHeadlight.intensity = highBeamIntensity;
                    leftHeadlight.spotAngle = highBeamSpotAngle;
                }
                if (rightHeadlight != null)
                {
                    rightHeadlight.intensity = highBeamIntensity;
                    rightHeadlight.spotAngle = highBeamSpotAngle;
                }
            }
            else
            {
                // Revert back to normal headlight settings
                if (leftHeadlight != null)
                {
                    leftHeadlight.intensity = normalIntensity;
                    leftHeadlight.spotAngle = normalSpotAngle;
                }
                if (rightHeadlight != null)
                {
                    rightHeadlight.intensity = normalIntensity;
                    rightHeadlight.spotAngle = normalSpotAngle;
                }
            }
        }
    }

    //FixedUpdate() is a built-in function that runs once per frame at a fxed interval
    private void FixedUpdate()
    {
        //Set the tailLights
        //SetActive() is the function for activating/deactivating a gameObject, it takes a boolean parameter (true/false).
        //isBraking is a boolean.
        leftTaillight.SetActive(isBraking);
        rightTaillight.SetActive(isBraking);

        //Call four seperate functions
        PlayerInput();
        Motor();
        Steering();
        UpdateWheels();
    }

    //NEW Function ito call the GameManagers's PaintSHop() function
    public void HamiltonCustoms(int selectedIndex)
    {
        //Assign the selectedIndex parameter to the dropdown option
        selectedIndex = dropdownList.value;

        //Now call the PaintSHop on the GameManager and send it that number.
        gm.PaintShop(selectedIndex);
    }

    private void PlayerInput()
    {
        //Set the inputs
        h = Input.GetAxis("Horizontal");
        v = Input.GetAxis("Vertical");

        isBraking = Input.GetKey(KeyCode.LeftShift);
    }

    private void Motor()
    {
        //If the enum is set to RWD, move the rear wheels. FWD = move front. AWD = Move all whels
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

        //Velocity is NOT speed. Speed is velocity in a direction (magnitude)
        //Default is given in meters/second.
        currentSpeed = rb.velocity.magnitude * 3.6f;    //Converts m/s to kph

        //Don't let the car go past top speed
        if (currentSpeed >= topSpeed)
        {
            currentSpeed = topSpeed;
        }

        //The AASHTO stopping distance formula. I learned what this is.
        //(0.278f * reaction time * speed) + (speed^2) / (254 * (friction ratng + slope)
        float stoppingDistance = (0.278f * Time.deltaTime * rb.velocity.magnitude) + (Mathf.Pow(rb.velocity.magnitude, 2) / (274 * (0.7f + 1)));
        //Now that we know stoping distance, apply that to the brakes
        //Mass of the vehicle * (speed /stoppingDistance)
        float brakingForce = rb.mass * (rb.velocity.magnitude / stoppingDistance);

        //Ternary if statement (shorthand)
        //is the player braking? If true, currentBrakeForce is the formula above
        //If false, currentBrakeForce is 0
        currentBrakForce = isBraking ? brakingForce : 0f;

        //Call AplyBrakes and send it to the math.
        ApplyBrakes(currentBrakForce);
    }

    void ApplyBrakes(float brakez)
    {
        //Apply the calculation to all the four wheelcolliers
        frontLeftCollider.brakeTorque = brakez;
        frontRightCollider.brakeTorque = brakez;
        rearLeftCollider.brakeTorque = brakez;
        rearRightCollider.brakeTorque = brakez;
    }

    private void Steering()
    {
        //Steering Angle is whatever the max stering angle is * h (horizontal)
        currentSteeringAngle = maxSteerinAngle * h;
        frontLeftCollider.steerAngle = currentSteeringAngle;
        frontRightCollider.steerAngle = currentSteeringAngle;
    }

    private void UpdateWheels()
    {
        //Call UpdateOneWheel for times and send it the Transform and collider for each
        UpdateOneWheel(frontLeftTransform, frontLeftCollider);
        UpdateOneWheel(rearLeftTransform, rearLeftCollider);
        UpdateOneWheel(frontRightTransform, frontRightCollider);
        UpdateOneWheel(rearRightTransform, rearRightCollider);
    }

    void UpdateOneWheel(Transform t, WheelCollider c)
    {
        //Two local variables for position and rotation
        Vector3 pos;
        Quaternion rot;

        //WheelColliders have built-in function GetWorldPose
        //It gets the current position and rotation in World Space 

        //"out" keyword sends a value somewhere.
        //We are finding where the colliders are, and sending their
        //position and rotation OUT to our variables
        c.GetWorldPose(out pos, out rot);

        //Now set the Transforms to be whatever the WheelColliders 
        //position and roation are.
        t.position = pos;
        t.rotation = rot;
    }

    private void OnTriggerEnter(Collider other)
    {
        //Check to see what the player collided with. If the player collided with the goal, call Lap() on the GM
        if (other.gameObject.name =="Goal")
        {
            gm.Lap();
        }

        //If the player collided with the checkpoint, set the bool on the GM!
        if (other.gameObject.name == "Checkpoint")
        {
            gm.checkpointPassed = true;
        }
    }
}

//Enum is a list of things. Unity only cares about the index
public enum driveTrain
{
    RWD, FWD, AWD
}