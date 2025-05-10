using UnityEngine;
using System.Collections.Generic;
using System;
using Unity.Mathematics;
using UnityEngine.Splines;

public class TrajectoryDisplay : MonoBehaviour
{
    [Header("References")]
    public Ball bowlingBall; // Reference to the ball object

    [Header("Trajectory Settings")]
    [Range(100, 500)]
    public int controlPointCount = 100;
    public float trajectoryLength = 15f;
    public float sphereCastRadius = 0.11f;

    [Header("Visualization")]
    public float splineWidth = 0.1f;
    [SerializeField] private Color splineColor = Color.white;
    [Range(100, 500)]
    public int renderSamplePoints = 240; // High sample count for rendering

    [Header("Starting Position")]
    [Range(-80f, 80f)]
    public float lateralPosition = 0f; // Z-axis position control

    public enum FunctionType { Quadratic, Cubic, Linear, Sine }
    public FunctionType currentFunction = FunctionType.Quadratic;

    [Header("Quadratic Parameters: f(x) = ax²")]
    [Range(-100f, 100f)]
    public float quadraticA = 0.2f; // Curvature direction and intensity

    [Header("Cubic Parameters: f(x) = ax³")]
    [Range(-100f, 100f)]
    public float cubicA = 0.05f; // Curvature direction and intensity

    [Header("Linear Parameters: f(x) = ax")]
    [Range(-100f, 100f)]
    public float linearA = 0.5f; // Slope

    [Header("Sine Parameters: f(x) = a·sin(bx)")]
    [Range(-100f, 100f)]
    public float sineA = 1f; // Amplitude
    [Range(0.01f, 5f)]
    public float sineB = 1f; // Frequency

    private List<Vector3> controlPoints = new List<Vector3>();
    private Dictionary<FunctionType, Func<float, float>> functionBank;

    private SplineContainer splineContainer;
    private LineRenderer lineRenderer;

    // Flag to track when we need to update the trajectory
    private bool needsTrajectoryUpdate = true;

    // Store the last position to detect changes
    private Vector3 lastBallPosition;

    void Start()
    {
        InitializeComponents();
        UpdateFunctionBank();
        // Force initial trajectory calculation
        UpdateTrajectory();
    }

    void Update()
    {
        // Update ball position based on lateralPosition if changed
        if (bowlingBall != null)
        {
            Vector3 ballPos = bowlingBall.transform.position;
            Vector3 newPosition = new Vector3(ballPos.x, ballPos.y, lateralPosition);

            // Only update if position changed
            if (newPosition != bowlingBall.transform.position)
            {
                bowlingBall.transform.position = newPosition;
                needsTrajectoryUpdate = true;
            }

            // Check if ball moved by other means (e.g., physics)
            if (lastBallPosition != bowlingBall.transform.position)
            {
                needsTrajectoryUpdate = true;
                lastBallPosition = bowlingBall.transform.position;
            }
        }

        // Only update trajectory when needed
        if (needsTrajectoryUpdate)
        {
            UpdateTrajectory();
            needsTrajectoryUpdate = false;
        }
    }

    // New method to update the entire trajectory
    public void UpdateTrajectory()
    {
        CalculateTrajectoryControlPoints();
        UpdateTrajectoryVisualization();
    }

    private void InitializeComponents()
    {
        // Get or create the SplineContainer
        splineContainer = GetComponent<SplineContainer>();
        if (splineContainer == null)
        {
            splineContainer = gameObject.AddComponent<SplineContainer>();
        }

        // Get or create the LineRenderer
        lineRenderer = GetComponent<LineRenderer>();
        if (lineRenderer == null)
        {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
            lineRenderer.startWidth = splineWidth;
            lineRenderer.endWidth = splineWidth * 0.5f;
            lineRenderer.startColor = splineColor;
            lineRenderer.endColor = new Color(splineColor.r, splineColor.g, splineColor.b, 0.5f);
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            lineRenderer.alignment = LineAlignment.View;
            lineRenderer.numCapVertices = 8;
            lineRenderer.numCornerVertices = 8;
            lineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            lineRenderer.receiveShadows = false;
        }

        // Initialize last ball position
        if (bowlingBall != null)
        {
            lastBallPosition = bowlingBall.transform.position;
        }
    }

    public void UpdateFunctionBank()
    {
        functionBank = new Dictionary<FunctionType, Func<float, float>>
        {
            // Quadratic: z = ax²
            { FunctionType.Quadratic, x => quadraticA * x * x },
            
            // Cubic: z = ax³
            { FunctionType.Cubic, x => cubicA * x * x * x },
            
            // Linear: z = ax
            { FunctionType.Linear, x => linearA * x },
            
            // Sine: z = a*sin(bx)
            { FunctionType.Sine, x => sineA * Mathf.Sin(sineB * x) }
        };

        // Flag for trajectory update
        needsTrajectoryUpdate = true;
    }

    private void CalculateTrajectoryControlPoints()
    {
        controlPoints.Clear();

        // Get X and Z from the ball (if available), but Y from this transform
        float startX = bowlingBall != null ? bowlingBall.transform.position.x : transform.position.x;
        float startZ = bowlingBall != null ? bowlingBall.transform.position.z : transform.position.z;

        // Always use Y from this object, not from the ball
        float startY = transform.position.y;

        Vector3 startPosition = new Vector3(startX, startY, startZ);

        // Always add the start position as the first point
        controlPoints.Add(startPosition);

        // For sine waves with higher frequencies, use more control points
        int actualControlPoints = controlPointCount;
        if (currentFunction == FunctionType.Sine)
        {
            // Scale control points based on frequency - higher frequency = more points
            float frequencyFactor = Mathf.Max(1.0f, sineB / 2.0f);
            actualControlPoints = Mathf.Min(500, Mathf.FloorToInt(controlPointCount * frequencyFactor));
        }

        bool hitSomething = false;
        var function = functionBank[currentFunction];

        // Generate points along the trajectory
        for (int i = 1; i < actualControlPoints && !hitSomething; i++)
        {
            // Use a precise linear distribution for sine waves
            float t;
            if (currentFunction == FunctionType.Sine)
            {
                // For sine waves, use perfect linear distribution to ensure wave accuracy
                t = (float)i / (actualControlPoints - 1);
            }
            else
            {
                // For other functions, use smoothstep for better visual distribution
                t = (float)i / (actualControlPoints - 1);
                t = t * t * (3f - 2f * t); // Smoothstep function
            }

            float distanceAlongX = t * trajectoryLength;

            // Scale x for better function evaluation
            float scaledX = distanceAlongX / trajectoryLength * 5f;

            // Calculate z-offset based on the current function
            float zOffset = function(scaledX);

            // Calculate the position along the trajectory
            Vector3 pointPosition = new Vector3(
                startPosition.x + distanceAlongX,
                startY, // Always use this object's Y position
                startPosition.z + zOffset
            );

            // Check for collision
            Vector3 previousPoint = controlPoints[controlPoints.Count - 1];
            Vector3 direction = pointPosition - previousPoint;
            float distance = direction.magnitude;

            RaycastHit hit;
            if (Physics.SphereCast(previousPoint, sphereCastRadius, direction.normalized, out hit, distance))
            {
                // Add the collision point and stop
                controlPoints.Add(previousPoint + direction.normalized * hit.distance);
                hitSomething = true;
                continue;
            }

            controlPoints.Add(pointPosition);
        }

        // Ensure minimum points for a valid spline
        if (controlPoints.Count < 2)
        {
            Vector3 minimalForwardPoint = new Vector3(
                startPosition.x + 0.1f,
                startY, // Use this object's Y position
                startPosition.z
            );
            controlPoints.Add(minimalForwardPoint);
        }
    }

    private void UpdateTrajectoryVisualization()
    {
        // Ensure we have a SplineContainer component
        if (splineContainer == null)
        {
            splineContainer = gameObject.AddComponent<SplineContainer>();
        }
        else
        {
            // Remove all existing splines
            while (splineContainer.Splines.Count > 0)
            {
                splineContainer.RemoveSplineAt(0);
            }
        }

        // Create a new spline
        Spline newSpline = new Spline();

        // Add knots to the spline
        foreach (Vector3 point in controlPoints)
        {
            float3 position = new float3(point.x, point.y, point.z);
            BezierKnot knot = new BezierKnot(position);
            newSpline.Add(knot);
        }

        // Choose the best tangent mode for each function type
        TangentMode tangentMode;

        switch (currentFunction)
        {
            case FunctionType.Sine:
                // For sine waves
                tangentMode = TangentMode.AutoSmooth;
                break;
            case FunctionType.Linear:
                // For linear function, use Linear tangents for perfect straight lines
                tangentMode = TangentMode.Linear;
                break;
            default:
                // For quadratic and cubic, AutoSmooth works well
                tangentMode = TangentMode.AutoSmooth;
                break;
        }

        // Apply the tangent mode to all knots
        for (int i = 0; i < newSpline.Count; i++)
        {
            newSpline.SetTangentMode(i, tangentMode);
        }

        // Add the spline to the container
        splineContainer.AddSpline(newSpline);

        // Update the LineRenderer with high-quality sampling
        UpdateLineRendererFromSpline(newSpline);
    }

    private void UpdateLineRendererFromSpline(Spline spline)
    {
        if (lineRenderer == null || spline == null || spline.Count == 0) return;

        // Use high sample count for smooth rendering, especially for sine waves
        int samplePoints = renderSamplePoints;
        if (currentFunction == FunctionType.Sine && sineB > 5f)
        {
            // Increase sample points for high-frequency waves
            samplePoints = Mathf.Min(500, Mathf.FloorToInt(renderSamplePoints * 1.5f));
        }

        lineRenderer.positionCount = samplePoints;

        for (int i = 0; i < samplePoints; i++)
        {
            float t = (float)i / (samplePoints - 1);
            float3 position = spline.EvaluatePosition(t);
            lineRenderer.SetPosition(i, new Vector3(position.x, position.y, position.z));
        }
    }

    // Modified public methods for UI integration - now they all trigger updates

    // Switch function type (0=Quadratic, 1=Cubic, 2=Linear, 3=Sine)
    public void SetFunctionType(int typeIndex)
    {
        currentFunction = (FunctionType)typeIndex;
        UpdateFunctionBank();
        // No need to call UpdateTrajectory() as UpdateFunctionBank() sets needsTrajectoryUpdate = true
    }

    // Lateral position control
    public void SetLateralPosition(float position)
    {
        lateralPosition = position;
        needsTrajectoryUpdate = true;
    }

    // Function-specific parameter setters
    public void SetQuadraticCurvature(float a)
    {
        quadraticA = a;
        UpdateFunctionBank();
    }

    public void SetCubicCurvature(float a)
    {
        cubicA = a;
        UpdateFunctionBank();
    }

    public void SetLinearSlope(float a)
    {
        linearA = a;
        UpdateFunctionBank();
    }

    public void SetSineAmplitude(float a)
    {
        sineA = a;
        UpdateFunctionBank();
    }

    public void SetSineFrequency(float b)
    {
        sineB = b;
        UpdateFunctionBank();
    }

    // Added setters for other properties that should trigger updates

    public void SetControlPointCount(int count)
    {
        controlPointCount = Mathf.Clamp(count, 100, 500);
        needsTrajectoryUpdate = true;
    }

    public void SetTrajectoryLength(float length)
    {
        trajectoryLength = length;
        needsTrajectoryUpdate = true;
    }

    public void SetSphereCastRadius(float radius)
    {
        sphereCastRadius = radius;
        needsTrajectoryUpdate = true;
    }

    public void SetSplineWidth(float width)
    {
        splineWidth = width;
        if (lineRenderer != null)
        {
            lineRenderer.startWidth = width;
            lineRenderer.endWidth = width * 0.5f;
        }
        needsTrajectoryUpdate = true;
    }

    public void SetRenderSamplePoints(int points)
    {
        renderSamplePoints = Mathf.Clamp(points, 100, 500);
        needsTrajectoryUpdate = true;
    }

    // Add method to handle inspector changes when in editor
#if UNITY_EDITOR
    private void OnValidate()
    {
        if (Application.isPlaying)
        {
            needsTrajectoryUpdate = true;
        }
    }
#endif
}