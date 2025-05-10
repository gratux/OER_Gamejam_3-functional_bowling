using UnityEngine;
using UnityEngine.UI;  // For UI Button integration
using Unity.Mathematics;
using UnityEngine.Splines;
using System.Collections;

public class BallLauncher : MonoBehaviour
{
    [Header("References")]
    public TrajectoryDisplay trajectoryDisplay;
    public Ball bowlingBall;
    public Button launchButton;  // Assign your UI button here

    [Header("Launch Settings")]
    public float launchSpeed = 10f;  // Speed of the ball along the spline
    public float exitVelocityMultiplier = 1.0f;  // Multiplier for the exit velocity

    [Header("Reset Settings")]
    public float resetYPosition = 7.5f;  // The Y position to place the ball at when resetting
    public float velocityThreshold = 0.05f;  // Ball is considered stopped below this velocity
    public float angularVelocityThreshold = 0.05f;  // Ball is considered stopped below this angular velocity
    public float checkStoppedInterval = 0.5f;  // How often to check if the ball has stopped

    private bool isLaunching = false;
    private bool isPhysicsActive = false;
    private SplineContainer splineContainer;
    private float splineDistance = 0f;
    private float splineLength;
    private Vector3 lastPosition;
    private Vector3 exitVelocity;
    private Coroutine checkStoppedCoroutine;

    void Start()
    {
        Debug.Log("BallLauncher initialized");

        // Connect the launch button's click event to our launch method
        if (launchButton != null)
        {
            launchButton.onClick.AddListener(LaunchBall);
        }
        else
        {
            Debug.LogError("No launch button assigned");
        }

        // Ensure we have references to required components
        if (trajectoryDisplay == null)
        {
            trajectoryDisplay = GetComponent<TrajectoryDisplay>();
        }

        if (bowlingBall == null && trajectoryDisplay != null)
        {
            bowlingBall = trajectoryDisplay.bowlingBall;
        }

        ResetBall();
    }

    void Update()
    {
        if (isLaunching)
        {
            MoveAlongSpline();
        }
    }

    public void LaunchBall()
    {
        Debug.Log("LaunchBall called");

        // Check conditions with minimal logging
        if (isLaunching || isPhysicsActive || bowlingBall == null || trajectoryDisplay == null)
        {
            Debug.LogWarning("Launch failed - preconditions not met");
            return;
        }

        // Get the spline from the trajectory display
        splineContainer = trajectoryDisplay.GetComponent<SplineContainer>();
        if (splineContainer == null || splineContainer.Splines.Count == 0)
        {
            Debug.LogError("No spline found");
            return;
        }

        // Stop any existing check coroutine
        if (checkStoppedCoroutine != null)
        {
            StopCoroutine(checkStoppedCoroutine);
            checkStoppedCoroutine = null;
        }

        // Prepare the ball for launch
        PrepareBallForLaunch();

        // Calculate spline length
        splineLength = splineContainer.Splines[0].GetLength();

        if (splineLength <= 0.01f)
        {
            Debug.LogError("Spline too small");
            return;
        }

        // Start the launch sequence
        isLaunching = true;
        isPhysicsActive = false;
        splineDistance = 0f;
        lastPosition = bowlingBall.transform.position;

        // Disable UI during launch
        if (launchButton != null) launchButton.interactable = false;
        if (trajectoryDisplay != null) trajectoryDisplay.gameObject.SetActive(false);
    }

    private void PrepareBallForLaunch()
    {
        // Ensure the ball is at the start position
        if (bowlingBall != null)
        {
            // Get the starting position from the trajectory display
            Vector3 startPos = trajectoryDisplay.transform.position;

            // Force Y to be exactly our reset Y position
            startPos.y = resetYPosition;

            // Apply lateral position from trajectory display
            if (trajectoryDisplay != null)
            {
                startPos.z = trajectoryDisplay.lateralPosition;
            }

            // Set the ball position
            bowlingBall.transform.position = startPos;

            // Make sure physics is inactive initially
            Rigidbody rb = bowlingBall.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = true;
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }
    }

    private void MoveAlongSpline()
    {
        if (bowlingBall == null || splineContainer == null ||
            splineContainer.Splines == null || splineContainer.Splines.Count == 0)
        {
            Debug.LogError("Missing components");
            EndLaunch(false);
            return;
        }

        Spline spline = splineContainer.Splines[0];

        try
        {
            // Calculate how far to move this frame
            float stepDistance = launchSpeed * Time.deltaTime;

            // Add to our accumulated distance
            splineDistance += stepDistance;

            // Check if we've reached the end of the spline
            if (splineDistance >= splineLength)
            {
                Debug.Log("Reached end of spline");
                EndLaunch(true);
                return;
            }

            // Convert distance to normalized t-value (0-1)
            float t = splineDistance / splineLength;

            // Get position on spline
            float3 position3 = spline.EvaluatePosition(t);
            Vector3 newPosition = new Vector3(position3.x, position3.y, position3.z);

            // IMPORTANT: Maintain the current Y position
            newPosition.y = bowlingBall.transform.position.y;

            // Get tangent on spline for velocity direction
            float3 tangent3 = spline.EvaluateTangent(t);
            Vector3 tangentDirection = new Vector3(tangent3.x, tangent3.y, tangent3.z);

            // Ensure the tangent is horizontal and normalized
            tangentDirection.y = 0;
            tangentDirection.Normalize();

            // Calculate velocity for physics handoff
            exitVelocity = tangentDirection * launchSpeed;

            // Move the ball
            bowlingBall.transform.position = newPosition;

            // Apply ball rolling
            ApplyBallRolling(tangentDirection, stepDistance);

            // Check for collisions
            CheckForCollisionsDuringMovement(newPosition);
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Exception in MoveAlongSpline: " + ex.Message);
            EndLaunch(false);
        }
    }

    // Helper method to apply proper ball rolling rotation
    private void ApplyBallRolling(Vector3 direction, float distanceMoved)
    {
        if (direction.magnitude < 0.1f) return;

        Collider collider = bowlingBall.GetComponent<Collider>();
        if (collider == null) return;

        float ballRadius = collider.bounds.extents.y;

        // Create rotation axis perpendicular to movement direction
        Vector3 rotationAxis = Vector3.Cross(Vector3.up, direction).normalized;

        // Apply rolling rotation based on distance moved
        float rotationAngle = distanceMoved / (ballRadius * Mathf.PI * 2) * 360f;

        if (!float.IsNaN(rotationAngle) && rotationAxis.magnitude > 0.01f)
        {
            bowlingBall.transform.Rotate(rotationAxis, rotationAngle, Space.World);
        }
    }

    // Helper method to check for collisions during guided movement
    private void CheckForCollisionsDuringMovement(Vector3 position)
    {
        Collider ballCollider = bowlingBall.GetComponent<Collider>();
        if (ballCollider == null) return;

        // Use a larger overlap radius to detect collisions sooner
        float detectionRadius = ballCollider.bounds.extents.x * 1.1f;

        int maxColliders = 10;
        Collider[] hitColliders = new Collider[maxColliders];
        int numColliders = Physics.OverlapSphereNonAlloc(
            position,
            detectionRadius,
            hitColliders,
            ~LayerMask.GetMask("Ball") // Ignore the ball's own layer
        );

        for (int i = 0; i < numColliders; i++)
        {
            // Skip self-collision, triggers, and objects tagged as "Floor"
            if (hitColliders[i] != null &&
                hitColliders[i] != ballCollider &&
                !hitColliders[i].isTrigger &&
                !hitColliders[i].CompareTag("Floor"))
            {
                Debug.Log("Collision with: " + hitColliders[i].name);
                EndLaunch(true);
                return;
            }
        }
    }

    private void EndLaunch(bool applyPhysics)
    {
        Debug.Log("EndLaunch with physics: " + applyPhysics);
        isLaunching = false;

        if (applyPhysics && bowlingBall != null)
        {
            // Enable physics and apply exit velocity
            Rigidbody rb = bowlingBall.GetComponent<Rigidbody>();
            if (rb != null)
            {
                // IMPORTANT: First set to non-kinematic, THEN set velocities
                rb.isKinematic = false;

                // Now it's safe to set velocities
                rb.velocity = exitVelocity * exitVelocityMultiplier;

                // Add some spin based on the trajectory
                Vector3 spin = Vector3.Cross(Vector3.up, exitVelocity.normalized) * launchSpeed * 0.5f;
                rb.angularVelocity = spin;

                // Mark that physics is now active
                isPhysicsActive = true;

                // Keep trajectory game object disabled during physics simulation
                if (trajectoryDisplay != null)
                    trajectoryDisplay.gameObject.SetActive(false);

                // Start checking if the ball has stopped
                checkStoppedCoroutine = StartCoroutine(CheckIfBallStopped());
            }
            else
            {
                ResetBall();
            }
        }
        else
        {
            // If we're not applying physics, just reset immediately
            ResetBall();
        }
    }

    private IEnumerator CheckIfBallStopped()
    {
        // Wait a minimum time before checking if stopped
        yield return new WaitForSeconds(1.0f);

        Rigidbody rb = bowlingBall.GetComponent<Rigidbody>();
        int consecutiveStoppedFrames = 0;  // Track consecutive frames where velocity is below threshold
        int requiredStoppedFrames = 2;     // Number of consecutive checks required to confirm stopped

        // Keep checking until the ball has stopped
        while (rb != null)
        {
            // Check velocity and angular velocity
            if (rb.velocity.magnitude < velocityThreshold &&
                rb.angularVelocity.magnitude < angularVelocityThreshold)
            {
                consecutiveStoppedFrames++;

                // If we've had enough consecutive stopped frames, the ball has truly stopped
                if (consecutiveStoppedFrames >= requiredStoppedFrames)
                {
                    break;  // Exit the loop - ball is stopped
                }
            }
            else
            {
                // Reset counter if velocity increases above threshold
                consecutiveStoppedFrames = 0;
            }

            // Wait before checking again
            yield return new WaitForSeconds(checkStoppedInterval);
        }

        // Ball has stopped - reset everything
        ResetBall();
        checkStoppedCoroutine = null;
    }

    public void ResetBall()
    {
        Debug.Log("ResetBall called");

        if (bowlingBall != null)
        {
            // Reset position
            Vector3 originPos = trajectoryDisplay != null ?
                               trajectoryDisplay.transform.position :
                               transform.position;
            originPos.y = resetYPosition;
            if (trajectoryDisplay != null)
            {
                originPos.z = trajectoryDisplay.lateralPosition;
            }
            bowlingBall.transform.position = originPos;

            // Reset rotation to identity (upright position)
            bowlingBall.transform.rotation = Quaternion.identity;

            // Completely reset physics
            Rigidbody rb = bowlingBall.GetComponent<Rigidbody>();
            if (rb != null)
            {
                // Always set to non-kinematic first to ensure velocities can be zeroed
                rb.isKinematic = false;
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                // Reset all forces
                rb.ResetInertiaTensor();
                rb.Sleep();  // Force the rigidbody to sleep
                             // Set back to kinematic
                rb.isKinematic = true;
            }
        }

        // UI and state management
        if (trajectoryDisplay != null) trajectoryDisplay.gameObject.SetActive(true);
        if (launchButton != null) launchButton.interactable = true;

        isLaunching = false;
        isPhysicsActive = false;
        exitVelocity = Vector3.zero;
    }

    // Public method that can be called to force a reset
    public void ForceReset()
    {
        Debug.Log("ForceReset called");

        if (checkStoppedCoroutine != null)
        {
            StopCoroutine(checkStoppedCoroutine);
            checkStoppedCoroutine = null;
        }

        ResetBall();
    }
}