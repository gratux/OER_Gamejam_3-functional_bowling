using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static TrajectoryDisplay;

public class FunctionGrid : MonoBehaviour
{
    [Header("Function Cards")]
    public FunctionCard functionCard_1;
    public FunctionCard functionCard_2;
    public FunctionCard functionCard_3;

    [Header("References")]
    public TrajectoryDisplay trajectoryDisplay;
    public ControlsUI controlsUI;

    // This will be visible in the inspector
    [SerializeField]
    private List<FunctionSpritePair> functionSpriteList = new List<FunctionSpritePair>();

    // Define the initial function types to display
    // Made public so ControlsUI can access it
    public FunctionType[] initialFunctions = new FunctionType[3]
    {
        FunctionType.Quadratic,
        FunctionType.Linear,
        FunctionType.Sine
    };

    private FunctionCard[] functionCards;

    void Start()
    {
        // Ensure we have all references
        if (functionCard_1 == null || functionCard_2 == null || functionCard_3 == null)
        {
            Debug.LogError("Missing function card references in FunctionGrid");
            return;
        }

        // Create an array of function cards for easier access
        functionCards = new FunctionCard[3]
        {
            functionCard_1,
            functionCard_2,
            functionCard_3
        };

        // Assign images to cards based on initial function types
        for (int i = 0; i < functionCards.Length; i++)
        {
            // Make sure we have a valid function type
            if (i < initialFunctions.Length)
            {
                FunctionType type = initialFunctions[i];
                Sprite sprite = GetSpriteForFunction(type);
                if (sprite != null && functionCards[i].functionFormulaImage != null)
                {
                    functionCards[i].functionFormulaImage.sprite = sprite;
                }
                else
                {
                    Debug.LogWarning($"Could not find sprite for function type {type} or image component is missing");
                }

                // Set up button click handlers
                int cardIndex = i; // Need to capture the index for the lambda
                if (functionCards[i].functionButton != null)
                {
                    functionCards[i].functionButton.onClick.AddListener(() => OnFunctionCardClicked(cardIndex));
                }
            }
        }
    }

    // Helper method to get a sprite for a function type
    private Sprite GetSpriteForFunction(FunctionType type)
    {
        foreach (var pair in functionSpriteList)
        {
            if (pair.functionType == type)
                return pair.sprite;
        }
        return null;
    }

    // Called when one of the function card buttons is clicked
    // Modify OnFunctionCardClicked method in FunctionGrid.cs
    public void OnFunctionCardClicked(int cardIndex)
    {
        if (cardIndex < 0 || cardIndex >= initialFunctions.Length || trajectoryDisplay == null)
            return;

        // Get the function type associated with this card
        FunctionType selectedFunction = initialFunctions[cardIndex];

        // Directly update the trajectory display - ensure this line works
        trajectoryDisplay.SetFunctionType((int)selectedFunction);

        // Debug output to verify it's being called
        Debug.Log($"FunctionGrid: Selected function: {selectedFunction}, setting index: {(int)selectedFunction}");

        // If we have reference to ControlsUI, notify it
        if (controlsUI != null)
        {
            // Pass the card index, not the function type
            controlsUI.SetFunctionType(cardIndex);
        }
        // Update the trajectory display
        trajectoryDisplay.SetFunctionType((int)selectedFunction);

        // If we have reference to ControlsUI, tell it to update
        if (controlsUI != null)
        {
            controlsUI.SetFunctionType(cardIndex);
        }

        Debug.Log($"Selected function: {selectedFunction}");
    }

    // Method to swap a function type on a specific card
    public void SetCardFunction(int cardIndex, FunctionType functionType)
    {
        if (cardIndex < 0 || cardIndex >= functionCards.Length)
            return;

        // Update the function type in our array
        if (cardIndex < initialFunctions.Length)
        {
            initialFunctions[cardIndex] = functionType;
        }

        // Update the card's sprite
        Sprite sprite = GetSpriteForFunction(functionType);
        if (sprite != null && functionCards[cardIndex].functionFormulaImage != null)
        {
            functionCards[cardIndex].functionFormulaImage.sprite = sprite;
        }
    }

    // Method to get the current function type for a card
    public FunctionType GetCardFunction(int cardIndex)
    {
        if (cardIndex >= 0 && cardIndex < initialFunctions.Length)
        {
            return initialFunctions[cardIndex];
        }

        // Default fallback
        return FunctionType.Quadratic;
    }
}

// Serializable class to pair function types with their UI sprites
[Serializable]
public class FunctionSpritePair
{
    public FunctionType functionType;
    public Sprite sprite;
}