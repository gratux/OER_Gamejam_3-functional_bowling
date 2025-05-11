using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    
    public GameObject ballPrefab;
    private GameObject currentBall;
    
    
    public List<Level> levels = new List<Level>() // store level data
    {
        new Level(1, 3, 4),//3 parameter is maxAttemps
        new Level(2, 4, 5),
        new Level(3, 3, 5),
        new Level(4, 3, 6),
        new Level(5, 3, 7)
    };
    
    
    
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
        
        GameManager.Instance.SetStarsLevel(starsEarned);
        //UIManager.Instance.ExitGameToMain();
        
        int levelIndex = nextLevel - 1;
        if (levelIndex >= 0 && levelIndex < levels.Count) //save stars earned
        {
            levels[levelIndex].registerResult(starsEarned, 1);//TODO ShootsMade for now useless
        }
    }
    
    public void SpawnBall()
    {
        GameObject spawnPoint = GameObject.Find("SpawnPointBall");

        if (spawnPoint != null && ballPrefab != null)
        {
            currentBall = Instantiate(ballPrefab, spawnPoint.transform.position, spawnPoint.transform.rotation);
            currentBall.transform.localScale = Vector3.one * 0.25f; // 75% reduction size
            print("instanciado");
        }
        else
        {
            print("not find spawnpoint");
        }
    }

    public List<Image> goldStarsLevelSelector;
    public List<Image> greyStarsLevelSelector;
    public void SetStarsLevel(int earnedStars)
    {
        for (int i = 0; i < 3; i++)
        {
            bool activaDorada = i < earnedStars;
            goldStarsLevelSelector[i].enabled  = activaDorada;
            greyStarsLevelSelector[i].enabled  = !activaDorada;
        }
    }
}
public class Level
{
    public int numLevel;
    public int earnedStars; //if a level has stars is completed
    public int shoots; // TODO CHANGE SHOOTS TO FUNC TYPE USED??
    public int numPins;
    public int ballsAvailable;
    //attemps por las funcciones
    //dic int functype

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