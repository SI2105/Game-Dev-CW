using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    public PlayerController player;

    // Going back to Main Menu when clicking Back to Main Menu button
    public void goBackToMain()
    {
        SceneManager.LoadScene(0);
    }

    void Start()
    {
        player = FindObjectOfType<PlayerController>();
    }

    // Update is called once per frame
    public void Update()
    {
        // Checking if player has lost and loading Lose Screen
        if (player.getPlayerHealth() == 0)
        {
            SceneManager.LoadScene(3);
        }
        // Checking if player has won and loading Win Screen
        else if (player.hasWon())
        {
            SceneManager.LoadScene(2);
        }
    }
}
