using UnityEngine;
using UnityEngine.SceneManagement;

public class HeadlightToggle : MonoBehaviour
{
    // two headlight objects
    public GameObject headlightLeft;
    public GameObject headlightRight;

    // Boolean to track headlight state (off by default)
    private bool headlightsOn = false;

    void Update()
    {
        // When the player presses H, toggle the headlights on or off
        if (Input.GetKeyDown(KeyCode.H))
        {
            headlightsOn = !headlightsOn;
            headlightLeft.SetActive(headlightsOn);
            headlightRight.SetActive(headlightsOn);
        }
    }
}
