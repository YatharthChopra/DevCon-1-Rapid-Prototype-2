using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    //Private variable to refer to its own class. Static means "there can only be one, and it cannot be modified"
    private static GameManager _instance;

    //Public method to "give access to" the variable
    //This is what the other scripts can access, and the GameManager says "if they call the Instance, it means the _instance variable
    public static GameManager Instance
    {
        //getter is "return value"
        get
        {
            return _instance;
        }
    }

    private void Awake()
    {
        //If the _instance does not exist, set this to the _instance and make it persistent (DontDestroyOnLoad)
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            //If there is already an _instance, destroy this one.
            Destroy(gameObject);
        }
    }

    //Array of Materials for colourOptions
    public Material[] colourOptions;

    //Renderer variable to store the car's renderer
    private Renderer carRenderer;

    //Lap Stuff
    public int currentLap = 0;
    public bool checkpointPassed = false;
    public TMP_Text lapText, speedText;

    //Access the car controller
    private CarController carController;

    // Start is called before the first frame update
    void Start()
    {

        carController = GameObject.Find("PlayerCar").GetComponent<CarController>();
        //Access the car object, and then refer to a child component.
        carRenderer = GameObject.Find("PlayerCar").GetComponentInChildren<Renderer>();

        //Set the car renderer to a default material
        carRenderer.material = colourOptions[0];
    }

    private void FixedUpdate()
    {
        //Get the carController's current speed variable and make speedText display it. F0 means "0 decimals" since its a float.
        speedText.text = carController.currentSpeed.ToString("F0") +  " km/h ";
    }

    //Public function that will change the renderer to whatever int is sent to it in the list of colours
    public void PaintShop(int selectedIndex)
    {
        carRenderer.material = colourOptions[selectedIndex];
    }

    public void Lap()
    {
        //Check to make sure the player has started before counting
        if (!checkpointPassed && currentLap == 0)
        {
            //IF the player doesn't have the checkpoint and are on lap 0, change current lap to 1
            currentLap = 1;
            lapText.text = currentLap.ToString();
        }

        //If the player is currently in a lap, and gotten the checkpoint
        if (currentLap > 0 && checkpointPassed)
        {
            //Increase the lap counter & reset checkpoint passed
            currentLap++;
            checkpointPassed = false;

            //Display the currentLap in the lapText
            lapText.text = currentLap.ToString();
        }
    }
}
