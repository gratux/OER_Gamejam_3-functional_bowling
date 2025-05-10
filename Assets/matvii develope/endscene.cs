using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class endscene : MonoBehaviour
{
    public GameObject ball;
    public int scenenum;
    
    void OnTriggerEnter(Collider other){
        if(other.gameObject == ball){
        SceneManager.LoadScene(scenenum);
        }
    }
    
}
