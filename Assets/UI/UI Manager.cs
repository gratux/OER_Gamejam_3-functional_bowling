using System.Collections;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.SceneManagement;


public class UIManager : MonoBehaviour
{
    public UITransitionType menuTransitionType = UITransitionType.Fade;
    public float menuSwitchAnimDuration;
    public float menuSwitchOvershoot;

    public CanvasGroup blurCanvas;
    public RectTransform mainMenu;
    public RectTransform inGameMenu;
    public RectTransform exitConfirmMenu;
    public RectTransform statsLevelMenu;
    public RectTransform levelSelectorMenu;
    public RectTransform pauseMenu;



    private Tweener _blurTween;
    private Tweener _hideWidgetTween;
    private Tweener _showWidgetTween;
/*
    private void Awake()
    {
        mainMenu.gameObject.SetActive(false);
    }
*/
    public static UIManager Instance;
    void Awake()
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

    public void GameToStatsMenu(int starsEarned) //Game -> StatsLevel Menu -> Main Menu
    {
        TogglePauseMenu();
        mainMenu.gameObject.SetActive(false);
        ShowLevelStars(starsEarned);
        statsLevelMenu.gameObject.SetActive(true);
    }
    public void StatsMenuToMainMenu() //TODO CHECK
    {
        UISwitchAnim(statsLevelMenu, mainMenu, false, menuTransitionType, false, menuSwitchAnimDuration);
    }
    public void StatsMenuToLevelSelector() //TODO CHECK
    {
        
        UISwitchAnim(statsLevelMenu, levelSelectorMenu, false, menuTransitionType, false, menuSwitchAnimDuration);
    }
    public void LevelSelectorToMainMenu() //TODO CHECK
    {
        UISwitchAnim(levelSelectorMenu, mainMenu, false, menuTransitionType, false, menuSwitchAnimDuration);
    }
    public void LevelSelectorToGame() //TODO CHECK
    {
        UISwitchAnim(levelSelectorMenu, mainMenu, false, menuTransitionType, false, menuSwitchAnimDuration);
    }
    public void MainMenuToLevelSelector()
    {
        UISwitchAnim(mainMenu, levelSelectorMenu, false, menuTransitionType, false, menuSwitchAnimDuration);
    }

    public void InitializeMainMenu()
    {
        UISwitchAnim(null, mainMenu, false, UITransitionType.Fade, false, 0.7f);
    }

    public void OpenPauseMenu()
    {
        UISwitchAnim(null, inGameMenu, false, UITransitionType.Fade, false, 0.5f);

    }
    public void MainToExitGame()
    {
        UISwitchAnim(mainMenu, exitConfirmMenu, true, menuTransitionType, false, menuSwitchAnimDuration);
    }

 
    public void ExitGameToMain()
    {
        // Suponemos que el MainMenu está en "MainMenuScene"
        string mainMenuSceneName = "Always Active Scene"; // <- Cambia esto al nombre real de tu escena de menú
        string currentSceneName = SceneManager.GetActiveScene().name;

        // Evita recargar si ya estamos en el menú
        if (currentSceneName == mainMenuSceneName)
            return;

        // Carga la escena del menú principal de forma aditiva
        SceneManager.LoadScene(mainMenuSceneName, LoadSceneMode.Additive);

        // Inicia una corrutina para descargar la escena anterior después de que cargue el menú
        StartCoroutine(UnloadCurrentSceneAfterDelay(currentSceneName));
        TogglePauseMenu();
    }

    private IEnumerator UnloadCurrentSceneAfterDelay(string sceneToUnload)
    {
        // Espera un frame para que la nueva escena se cargue correctamente
        yield return null;

        // Opcional: cambia la escena activa a Always Active si lo necesitas
        SceneManager.SetActiveScene(SceneManager.GetSceneByName("Always Active Scene"));

        // Descarga la escena anterior
        SceneManager.UnloadSceneAsync(sceneToUnload);

        // Muestra el menú principal
        UISwitchAnim(null, mainMenu, false, UITransitionType.Fade, false, menuSwitchAnimDuration);
    }


    public void ExitGame()
    {
        Application.Quit();
    }


    public void LoadLevel(int levelNum)
    {
        //UISwitchAnim(levelSelectorMenu, levelName, true, menuTransitionType, false, menuSwitchAnimDuration);
        SceneManager.LoadScene(levelNum, LoadSceneMode.Single);
        
        levelSelectorMenu.gameObject.SetActive(false);
        mainMenu.gameObject.SetActive(false);
        inGameMenu.gameObject.SetActive(true);//yes
        statsLevelMenu.gameObject.SetActive(false);
        exitConfirmMenu.gameObject.SetActive(false);
        blurCanvas.gameObject.SetActive(false);

        SceneManager.sceneLoaded += OnSceneLoaded;
        print("asdasd");

    }
    
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != "Always Active Scene")
        {
            GameManager.Instance.SpawnBall();
            Debug.Log("Ball spawned after scene loaded.");
        }

        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    public void ReplayCurrentLevel()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.buildIndex, LoadSceneMode.Single);

        levelSelectorMenu.gameObject.SetActive(false);
        mainMenu.gameObject.SetActive(false);
        inGameMenu.gameObject.SetActive(false);
        statsLevelMenu.gameObject.SetActive(false);
        exitConfirmMenu.gameObject.SetActive(false);
        blurCanvas.gameObject.SetActive(false);
    }


    void Update()
    {
        if (SceneManager.GetActiveScene().name == "Always Active Scene") return;
        
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePauseMenu();
        }
    }
    
    private bool isPauseMenuOpen = false;


    public void TogglePauseMenu()
    {
        if (isPauseMenuOpen)
        {
            //UISwitchAnim(pauseMenu, null, false, UITransitionType.Fade, false, menuSwitchAnimDuration);
            pauseMenu.gameObject.SetActive(false);
            isPauseMenuOpen = false;
            Time.timeScale = 1f;
        }
        else
        {
            //UISwitchAnim(null, pauseMenu, false, UITransitionType.Fade, false, menuSwitchAnimDuration);
            pauseMenu.gameObject.SetActive(true);
            isPauseMenuOpen = true;
            Time.timeScale = 0f;
        }
    }
    public void ResumeButton()
    {
        TogglePauseMenu();
    }

    

    void UISwitchAnim(RectTransform hideWidget, RectTransform showWidget, bool moveLeft, UITransitionType transitionEffect, bool affectGameplay, float animDuration)
    {
        _showWidgetTween?.Kill();
        _hideWidgetTween?.Kill();

        showWidget.gameObject.SetActive(true);

        CanvasGroup showCanvasGroup = showWidget.GetComponent<CanvasGroup>();

        if (showCanvasGroup == null)
        {
            Debug.LogError(showWidget + " does not have a Canvas Group component.");
            return;
        }

        CanvasGroup hideCanvasGroup = null;
        if (hideWidget != null)
        {
            hideWidget.gameObject.SetActive(true);

            hideCanvasGroup = hideWidget.GetComponent<CanvasGroup>();

            if (hideCanvasGroup == null)
            {
                Debug.LogError(hideWidget + " does not have a Canvas Group component.");
                return;
            }

            hideCanvasGroup.blocksRaycasts = false;
        }

        showCanvasGroup.blocksRaycasts = false;

        switch (transitionEffect)
        {
            case UITransitionType.Slide:

                if (moveLeft)
                {
                    if (hideWidget != null)
                    {
                        _hideWidgetTween = hideWidget.DOAnchorPosX(-Screen.width, animDuration);
                    }
                    showWidget.anchoredPosition = new Vector2(Screen.width, 0);
                }
                else
                {
                    if (hideWidget != null)
                    {
                        _hideWidgetTween = hideWidget.DOAnchorPosX(Screen.width, animDuration);
                    }
                    showWidget.anchoredPosition = new Vector2(-Screen.width, 0);
                }

                _showWidgetTween = showWidget.DOAnchorPosX(0, animDuration).SetEase(Ease.OutBack, menuSwitchOvershoot)
                    .OnComplete(() =>
                    {
                        OnUITransitionComplete(hideWidget, showCanvasGroup);
                    });

                break;

            case UITransitionType.Fade:

                showWidget.anchoredPosition = Vector2.zero;

                showCanvasGroup.alpha = 0;

                if (hideWidget != null)
                {
                    _hideWidgetTween = hideCanvasGroup.DOFade(0, animDuration * 0.5f).SetEase(Ease.InQuad)
                        .OnComplete(() =>
                        {
                            _showWidgetTween = showCanvasGroup.DOFade(1, animDuration * 0.5f).SetEase(Ease.InQuad)
                            .OnComplete(() =>
                            {
                                OnUITransitionComplete(hideWidget, showCanvasGroup);
                            });
                        });
                }
                else
                {
                    _showWidgetTween = showCanvasGroup.DOFade(1, animDuration * 0.5f).SetEase(Ease.InQuad)
                        .OnComplete(() =>
                        {
                            OnUITransitionComplete(hideWidget, showCanvasGroup);
                        });
                }

                break;

            case UITransitionType.Instant:

                _showWidgetTween?.Kill();
                _hideWidgetTween?.Kill();

                showCanvasGroup.alpha = 1;
                showWidget.anchoredPosition = Vector2.zero;

                if (hideWidget != null)
                {
                    hideCanvasGroup.blocksRaycasts = false;
                    hideCanvasGroup.alpha = 0;
                }

                OnUITransitionComplete(hideWidget, showCanvasGroup);

                break;

        }

    }

    void OnUITransitionComplete(RectTransform hideWidget, CanvasGroup showCanvasGroup)
    {
        if (hideWidget != null)
        {
            hideWidget.gameObject.SetActive(false);
        }
        showCanvasGroup.blocksRaycasts = true;
    }

    public void FadeInFadeOutBlur(float startAlpha, float endAlpha, float animDuration, bool pingPongBlur)
    {
        _blurTween?.Kill();

        blurCanvas.alpha = startAlpha;
        blurCanvas.gameObject.SetActive(true);

        _blurTween = blurCanvas.DOFade(endAlpha, animDuration).SetEase(Ease.InQuad)
            .OnComplete(() =>
            {
                if (pingPongBlur && endAlpha == 1)
                {
                    _blurTween.Kill();

                    _blurTween = blurCanvas.DOFade(startAlpha, animDuration).SetEase(Ease.InQuad)
                    .OnComplete(() =>
                    {
                        blurCanvas.blocksRaycasts = false;
                        blurCanvas.gameObject.SetActive(false);
                    });
                }
                else
                {
                    blurCanvas.blocksRaycasts = endAlpha == 0 ? false : true;
                    blurCanvas.gameObject.SetActive(endAlpha == 0 ? false : true);
                }

            });
    }

    public List<Image> goldStars;
    public List<Image> greyStars;

    public void ShowLevelStars(int starsEarned)
    {
        for (int i = 0; i < 3; i++)
        {
            if (i < starsEarned)
            {
                goldStars[i].enabled = true;
                greyStars[i].enabled = false;
            }
            else
            {
                goldStars[i].enabled = false;
                greyStars[i].enabled = true;
            }
        }
    }
}

public enum UITransitionType
{
    Slide,
    Fade,
    Instant
}