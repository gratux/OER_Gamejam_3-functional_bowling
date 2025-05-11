using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    
    public Ball ballPrefab;
    public BallControls ballControlsPrefab;
    public Ball currentBall;
    public BallControls currentBallControls;

    public Button ballLaunchButton;
    
    
    public List<Level> levels = new List<Level>() // store level data
    {
        new Level(1, 3, 3),//3 parameter is maxAttemps
        new Level(2, 4, 3),
        new Level(3, 3, 3),
        new Level(4, 3, 3),
        new Level(5, 3, 3)
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
        GameManager.Instance.SetStarsLevel(starsEarned);
        UIManager.Instance.GameToStatsMenu(starsEarned);
        //TODO SAVE STARS EARNED
        
        //UIManager.Instance.ExitGameToMain();
        
        int levelIndex = nextLevel - 1;
        if (levelIndex >= 0 && levelIndex < levels.Count) //save stars earned
        {
            levels[levelIndex].registerResult(starsEarned);//TODO ShootsMade for now useless
        }
    }
    
    public void SpawnBall()
    {
        GameObject spawnPoint = GameObject.Find("SpawnPointBall");

        if (spawnPoint != null && ballPrefab != null)
        {
            currentBall = Instantiate(ballPrefab, spawnPoint.transform.position, spawnPoint.transform.rotation);
            print("instanciado ball");
        }
        else
        {
            print("not find spawnpoint");
        }
    }

    public void SpawnBallControls()
    {
        GameObject spawnPoint = GameObject.Find("SpawnPointBall");

        if (spawnPoint != null && ballControlsPrefab != null)
        {
            currentBallControls = Instantiate(ballControlsPrefab, spawnPoint.transform.position, spawnPoint.transform.rotation);
            print("instanciado ball controls");
            currentBallControls.trajectoryDisplay.bowlingBall = currentBall;
            currentBallControls.ballLauncher.bowlingBall = currentBall;
            currentBallControls.ballLauncher.launchButton = ballLaunchButton;
            currentBallControls.ballLauncher.ballTrail = currentBall.transform.Find("TrailParticles").gameObject.GetComponent<ParticleSystem>();
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

    public void ballStoped()
    {
        ballsLaunched++;
        
        Level level = levels[UIManager.Instance.currentLevel];
        level.ballUsed();
        print(ballsLaunched);
    }

    public int ballsLaunched = 0;

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
        //this.shoots = 0;
        this.numPins = numPins;
        this.ballsAvailable = ballsAvailable;
    }

    public void registerResult(int newStarsEraned)
    {
        earnedStars = Mathf.Max(earnedStars, newStarsEraned); //if user gets more stars than before
    }

    public void ballUsed()
    {
        ballsAvailable--;
        if (0 == this.ballsAvailable)
        {
            //go to stats menu and reset attemps
            ballsAvailable = 3;
        }
    }
}