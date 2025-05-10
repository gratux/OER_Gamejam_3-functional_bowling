using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TestSampleScene : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void Awake()
    {
        if (!SceneManager.GetSceneByName("Always Active Scene").isLoaded)
        {
            SceneManager.LoadScene("Always Active Scene", LoadSceneMode.Additive);
            UIManager.Instance.OpenPauseMenu();
        }
    }
}
