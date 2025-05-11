using System;
using System.Collections.Generic;
using UnityEngine;
using static TrajectoryDisplay;

public class FunctionGrid : MonoBehaviour
{
    public FunctionCard functionCard_1;
    public FunctionCard functionCard_2;
    public FunctionCard functionCard_3;

    // This will be visible in the inspector
    [SerializeField]
    private List<FunctionSpritePair> functionSpriteList = new List<FunctionSpritePair>();

    // Define the initial function types to display
    [SerializeField]
    private FunctionType[] initialFunctions = new FunctionType[3]
    {
        FunctionType.Quadratic,
        FunctionType.Linear,
        FunctionType.Sine
    };

    void Start()
    {
        // Ensure we have all references
        if (functionCard_1 == null || functionCard_2 == null || functionCard_3 == null)
        {
            Debug.LogError("Missing function card references in FunctionGrid");
            return;
        }

        // Create an array of function cards for easier access
        FunctionCard[] cards = new FunctionCard[3]
        {
            functionCard_1,
            functionCard_2,
            functionCard_3
        };

        // Assign images to cards based on initial function types
        for (int i = 0; i < cards.Length; i++)
        {
            // Make sure we have a valid function type
            if (i < initialFunctions.Length)
            {
                FunctionType type = initialFunctions[i];
                Sprite sprite = GetSpriteForFunction(type);

                if (sprite != null && cards[i].functionFormulaImage != null)
                {
                    cards[i].functionFormulaImage.sprite = sprite;
                }
                else
                {
                    Debug.LogWarning($"Could not find sprite for function type {type} or image component is missing");
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
}

[Serializable]
public class FunctionSpritePair
{
    public FunctionType functionType;
    public Sprite sprite;
}