using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class GameController : MonoBehaviour
{
    //public TwoDimensionalAnimationController player;

    // Going back to Main Menu when clicking Back to Main Menu button
    public void goBackToMain()
    {
      //  Debug.Log("Here");
      //  SceneManager.LoadScene(0);
    }

    void Start()
    {
      //  try{
            //retrieve the player object
      //      player = FindObjectOfType<PlayerController>();
     //   }

      //  catch{
    //        player=null;
      //  }
        
    }

    // Update is called once per frame
    public void Update()
    {

       // if(player==null){
       //     return;
       // }
        // Checking if player has lost and loading Lose Screen
       // if (player.getPlayerHealth() == 0)
       // {
            //player.ToggleCursorLock();
        //    SceneManager.LoadScene(3);
        //}
        // Checking if player has won and loading Win Screen
       // else if (player.hasWon())
      //  {
       //     player.ToggleCursorLock();
       //     SceneManager.LoadScene(2);
       // }
    }
}
