using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static TrajectoryDisplay;

public class ControlsUI : MonoBehaviour
{
    [Header("References")]
    public TrajectoryDisplay trajectoryDisplay;
    public FunctionGrid functionGrid;
    public BallLauncher ballLauncher;

    [Header("Parameter Sliders")]
    public Slider aSlider;    // Primary parameter (curvature, slope, amplitude)
    public Slider bSlider;    // Secondary parameter (frequency for sine)
    public Slider cSlider;    // Lateral position slider

    [Header("Slider Value Text")]
    public TMP_Text aValueText;
    public TMP_Text bValueText;
    public TMP_Text cValueText;

    [Header("Function Display")]
    public TMP_Text currentFunctionLabel;

    // Flag to track if controls are locked during ball launch
    private bool controlsLocked = false;

    private void Start()
    {
        SetupUICallbacks();

        // Connect to ball launcher events
        if (ballLauncher != null && ballLauncher.launchButton != null)
        {
            ballLauncher.launchButton.onClick.AddListener(OnBallLaunch);
        }

        // Ensure the function grid has a reference to this UI
        if (functionGrid != null)
        {
            functionGrid.controlsUI = this;
        }

        // Initialize UI with current trajectory settings
        RefreshUI();
    }

    // Public method that can be called to refresh UI
    public void RefreshUI()
    {
        if (trajectoryDisplay == null) return;

        // Update function label
        if (currentFunctionLabel != null)
        {
            currentFunctionLabel.text = $"Function: {trajectoryDisplay.currentFunction}";
        }

        // Setup C slider - always visible
        if (cSlider != null)
        {
            cSlider.minValue = -80f;  // These values are fixed in TrajectoryDisplay
            cSlider.maxValue = 80f;
            cSlider.value = trajectoryDisplay.lateralPosition;

            if (cValueText != null)
                cValueText.text = ((int)trajectoryDisplay.lateralPosition).ToString();
        }

        // Based on current function, configure A and B sliders
        switch (trajectoryDisplay.currentFunction)
        {
            case FunctionType.Quadratic:
                // Setup A slider for quadratic
                if (aSlider != null)
                {
                    aSlider.gameObject.SetActive(true);
                    aSlider.minValue = -100f;
                    aSlider.maxValue = 100f;
                    aSlider.value = trajectoryDisplay.quadraticA;

                    if (aValueText != null)
                        aValueText.text = ((int)trajectoryDisplay.quadraticA).ToString();
                }

                // Hide B slider
                if (bSlider != null)
                    bSlider.gameObject.SetActive(false);
                break;

            case FunctionType.Cubic:
                // Setup A slider for cubic
                if (aSlider != null)
                {
                    aSlider.gameObject.SetActive(true);
                    aSlider.minValue = -100f;
                    aSlider.maxValue = 100f;
                    aSlider.value = trajectoryDisplay.cubicA;

                    if (aValueText != null)
                        aValueText.text = ((int)trajectoryDisplay.cubicA).ToString();
                }

                // Hide B slider
                if (bSlider != null)
                    bSlider.gameObject.SetActive(false);
                break;

            case FunctionType.Linear:
                // Setup A slider for linear
                if (aSlider != null)
                {
                    aSlider.gameObject.SetActive(true);
                    aSlider.minValue = -100f;
                    aSlider.maxValue = 100f;
                    aSlider.value = trajectoryDisplay.linearA;

                    if (aValueText != null)
                        aValueText.text = ((int)trajectoryDisplay.linearA).ToString();
                }

                // Hide B slider
                if (bSlider != null)
                    bSlider.gameObject.SetActive(false);
                break;

            case FunctionType.Sine:
                // Setup A slider for sine amplitude
                if (aSlider != null)
                {
                    aSlider.gameObject.SetActive(true);
                    aSlider.minValue = -100f;
                    aSlider.maxValue = 100f;
                    aSlider.value = trajectoryDisplay.sineA;

                    if (aValueText != null)
                        aValueText.text = ((int)trajectoryDisplay.sineA).ToString();
                }

                // Setup B slider for sine frequency
                if (bSlider != null)
                {
                    bSlider.gameObject.SetActive(true);
                    bSlider.minValue = 0.01f;
                    bSlider.maxValue = 5f;
                    bSlider.value = trajectoryDisplay.sineB;

                    if (bValueText != null)
                        bValueText.text = ((int)(trajectoryDisplay.sineB * 10)).ToString(); // Multiply by 10 for more useful display
                }
                break;
        }
    }

    private void SetupUICallbacks()
    {
        // Setup slider callbacks
        if (aSlider != null)
        {
            aSlider.onValueChanged.RemoveAllListeners();
            aSlider.onValueChanged.AddListener(OnASliderChanged);
        }

        if (bSlider != null)
        {
            bSlider.onValueChanged.RemoveAllListeners();
            bSlider.onValueChanged.AddListener(OnBSliderChanged);
        }

        if (cSlider != null)
        {
            cSlider.onValueChanged.RemoveAllListeners();
            cSlider.onValueChanged.AddListener(OnCSliderChanged);
        }
    }

    // Called when a function card button is clicked
    public void SetFunctionType(int functionIndex)
    {
        if (trajectoryDisplay == null || controlsLocked) return;

        if (functionGrid != null && functionIndex >= 0 && functionIndex < functionGrid.initialFunctions.Length)
        {
            FunctionType newFunction = functionGrid.initialFunctions[functionIndex];

            // Don't set the function type again - FunctionGrid already did this
            // Just verify it worked
            Debug.Log($"ControlsUI: Function selected: {newFunction}, Trajectory function: {trajectoryDisplay.currentFunction}");

            // Then refresh the UI
            RefreshUI();
        }
    }

    // Called when ball is launched - lock controls
    public void OnBallLaunch()
    {
        controlsLocked = true;

        // Disable sliders
        if (aSlider != null) aSlider.interactable = false;
        if (bSlider != null) bSlider.interactable = false;
        if (cSlider != null) cSlider.interactable = false;

        // Disable function buttons
        if (functionGrid != null)
        {
            if (functionGrid.functionCard_1?.functionButton != null)
                functionGrid.functionCard_1.functionButton.interactable = false;

            if (functionGrid.functionCard_2?.functionButton != null)
                functionGrid.functionCard_2.functionButton.interactable = false;

            if (functionGrid.functionCard_3?.functionButton != null)
                functionGrid.functionCard_3.functionButton.interactable = false;
        }
    }

    // Called from BallLauncher.ResetBall() - unlock controls
    public void OnBallReset()
    {
        controlsLocked = false;

        // Enable sliders
        if (aSlider != null) aSlider.interactable = true;
        if (bSlider != null) bSlider.interactable = true;
        if (cSlider != null) cSlider.interactable = true;

        // Enable function buttons
        if (functionGrid != null)
        {
            if (functionGrid.functionCard_1?.functionButton != null)
                functionGrid.functionCard_1.functionButton.interactable = true;

            if (functionGrid.functionCard_2?.functionButton != null)
                functionGrid.functionCard_2.functionButton.interactable = true;

            if (functionGrid.functionCard_3?.functionButton != null)
                functionGrid.functionCard_3.functionButton.interactable = true;
        }

        // Refresh UI to match current trajectory state
        RefreshUI();
    }

    // Slider callback methods
    private void OnASliderChanged(float value)
    {
        if (controlsLocked || trajectoryDisplay == null) return;

        // Use the full decimal value for calculations
        switch (trajectoryDisplay.currentFunction)
        {
            case FunctionType.Quadratic:
                trajectoryDisplay.SetQuadraticCurvature(value);
                break;

            case FunctionType.Cubic:
                trajectoryDisplay.SetCubicCurvature(value);
                break;

            case FunctionType.Linear:
                trajectoryDisplay.SetLinearSlope(value);
                break;

            case FunctionType.Sine:
                trajectoryDisplay.SetSineAmplitude(value);
                break;
        }

        // Display only integer value in UI
        if (aValueText != null)
            aValueText.text = ((int)value).ToString();
    }

    private void OnBSliderChanged(float value)
    {
        if (controlsLocked || trajectoryDisplay == null) return;

        // Only Sine uses parameter B
        // Use the full decimal value for calculation
        if (trajectoryDisplay.currentFunction == FunctionType.Sine)
            trajectoryDisplay.SetSineFrequency(value);

        // Display integer value in UI (multiplied for better display)
        if (bValueText != null)
            bValueText.text = ((int)(value * 10)).ToString();
    }

    private void OnCSliderChanged(float value)
    {
        if (controlsLocked || trajectoryDisplay == null) return;

        // Use the full decimal value for calculation
        trajectoryDisplay.SetLateralPosition(value);

        // Display only integer value in UI
        if (cValueText != null)
            cValueText.text = ((int)value).ToString();
    }

    // Check if ball was reset
    private void Update()
    {
        if (controlsLocked && trajectoryDisplay != null &&
            trajectoryDisplay.gameObject.activeSelf)
        {
            OnBallReset();
        }
    }
}