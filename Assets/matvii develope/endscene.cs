using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class endscene : MonoBehaviour
{
    public GameObject ball;
    public int scenenum;// next scene?
    
    
    void OnTriggerEnter(Collider other){
        if(other.gameObject == ball){
            GameManager.Instance.endLevel(2, scenenum);//TODO CHANGE FIRST PARAMETER TO RLLY EARNED STARS
            print("done");
        }
    }
    
}
