using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class trigger : MonoBehaviour
{
   public GameObject ball;
   public bool scenechao = false;
   public Animator animator;
    
    void OnTriggerEnter(Collider other){
    if(other.gameObject == ball){
        animator.SetBool("scenechao", true);
    }
                                        }
    void ChangeCheneone(){
        SceneManager.LoadScene(1);
    }
    
}
