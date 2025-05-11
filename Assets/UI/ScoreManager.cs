using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    // List to keep track of all pins in the scene
    [SerializeField]
    private List<GameObject> pins;  // List of pins in the scene.

    private int pinsHitCount = 0;   // Counter for the number of pins hit.
    private int totalPins = 3;      // Total number of pins in the scene.

    private void Start()
    {
        ResetPinCount();
        totalPins = pins.Count;

        Debug.Log("Total pins: " + totalPins);
    }

    // This function is called when a pin is hit.
    public void PinHit(GameObject pin)
    {
        if (!pins.Contains(pin))  // Check if the pin hasn't been counted already
        {
            pins.Add(pin);         // Add the pin to the list of hit pins
            pinsHitCount++;        // Increment the hit pin counter

            Debug.Log("Pin hit! Total pins hit: " + pinsHitCount);

            // After hitting a pin, check if all the pins are hit or if the turn is finished

        }
    }

    void Update()
    {
        if (GameManager.Instance.ballsLaunched == 3)
        {
            CalculateStars();
        }
    }

    // Calculate stars based on the number of pins hit
    public void CalculateStars()
    {
        int starsEarned = 0;

        if (pinsHitCount == 0)
        {
            // If no pins are hit, 0 stars
            starsEarned = 1;
        }
        else if (pinsHitCount == totalPins)
        {
            // If all pins are hit, 3 stars
            starsEarned = 3;
        }
        else
        {
            // If at least one pin is hit, but not all, 1 star
            starsEarned = 2;
        }

        Debug.Log("Pins hit: " + pinsHitCount + "/" + totalPins + ". Stars earned: " + starsEarned);
        GameManager.Instance.endLevel(starsEarned, 1);  // Example: Pass stars and level number to the GameManager
    }

    // Function to reset the pin hit counter
    public void ResetPinCount()
    {
        pins.Clear();
        pinsHitCount = 0;
        Debug.Log("Pin counter reset.");
    }
    
}
