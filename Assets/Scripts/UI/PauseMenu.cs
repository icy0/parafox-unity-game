using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public static bool gameIsPaused = false;

    public GameObject pauseMenuUI;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log("esc");
            if (gameIsPaused)
            {
                pauseMenuUI.SetActive(false);
                gameIsPaused = false;

            }
            else
            {
                pauseMenuUI.SetActive(true);
                gameIsPaused = true;

            }
        }
    }

    public void ShowStartMenu()
    {
        SceneManager.LoadScene(0);
    }
}

