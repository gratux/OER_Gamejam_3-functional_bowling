using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PinCounter : MonoBehaviour
{
    // Reference to the ScoreManager to notify when the pin is hit
    private ScoreManager scoreManager;

    private void Start()
    {
        // Find the ScoreManager in the scene
        scoreManager = FindObjectOfType<ScoreManager>();
    }

    // This function is called when another collider enters the pin's trigger
    private void OnTriggerEnter(Collider other)
    {
        // Check if the object that collided with the pin is the ball (assuming it has the "Ball" tag)
        if (other.CompareTag("Player")) // Assuming the ball has the "Ball" tag
        {
            // Notify the ScoreManager that this pin was hit
            scoreManager.PinHit(this.gameObject);
            // Optionally, disable the pin or play a sound or animation
            // gameObject.SetActive(false);  // Example: Disable the pin when it's hit
        }
    }
}