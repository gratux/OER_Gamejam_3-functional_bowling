using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void Start()
    {
        UIManager.Instance.InitializeMainMenu();
    }
    

    public void endLevel(int starsEarned, int nextLevel)//with next level i can know which level i passed (nextlevel-1)
    {
        UIManager.Instance.GameToStatsMenu(starsEarned);
        //TODO SAVE STARS EARNED
        
        
        UIManager.Instance.ExitGameToMain();
    }
}
public class Level
{
    public int numLevel;
    public int earnedStars; //if a level has stars is completed
    public int shoots;
    public int numPins;
    public int ballsAvailable;

    public Level(int numLevel, int numPins, int ballsAvailable)
    {
        this.numLevel = numLevel;
        this.earnedStars = 0;
        this.shoots = 0;
        this.numPins = numPins;
        this.ballsAvailable = ballsAvailable;
    }

    public void registerResult(int newStarsEraned, int shootsMade)
    {
        earnedStars = Mathf.Max(earnedStars, newStarsEraned); //if user gets more stars than before
        shoots = shootsMade;
    }
}