using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    // Starting the game when this method called
    public void PlayGame()
    {
        // Loads next scene after the current scene
        // 0 (Main menu scene) + 1 = 1 (Game scene)
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
}
