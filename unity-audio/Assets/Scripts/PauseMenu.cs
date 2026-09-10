using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;

public class PauseMenu : MonoBehaviour
{
    public GameObject pauseCanvas;
    private bool isPaused = false;
    public AudioMixer audioMixer;
    public string normalSnapshot = "Normal";
    public string muffledSnapshot = "Muffled";
    public float transitionTime = 0.5f; 

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!isPaused)
            {
                Pause();
                
            } else
            {
                Resume();
            }
        }

    }

    public void Pause()
    {
        isPaused = true;
        pauseCanvas.SetActive(true);
        audioMixer.FindSnapshot(muffledSnapshot).TransitionTo(transitionTime);
        Debug.Log("Muffled Snapshot Playing");
        Time.timeScale = 0f;
        

    }

    public void Resume()
    {
        isPaused = false;
        pauseCanvas.SetActive(false);
        audioMixer.FindSnapshot(normalSnapshot).TransitionTo(transitionTime);
        Debug.Log("Normal Snapshot Playing");
        Time.timeScale = 1f;
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void MainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    public void Options()
    {
        Time.timeScale = 1f;
        PlayerPrefs.SetString("Previous", SceneManager.GetActiveScene().name);
        SceneManager.LoadScene("Options");
    }


}
