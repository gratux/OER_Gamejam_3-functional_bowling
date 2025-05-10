using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Changesceneone : MonoBehaviour
{
    public int scenenum;
    
                                        
    void ChangeCheneone(){
        SceneManager.LoadScene(scenenum);
    }
    
}