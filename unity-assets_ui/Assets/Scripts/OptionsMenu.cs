using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class OptionsMenu : MonoBehaviour
{
    public Toggle invertYToggle;

    private string previousScene;

    void Start()
    {
        // store where player came from
        previousScene = PlayerPrefs.GetString("PreviousScene");

        // load saved setting into UI
        invertYToggle.isOn = SettingsData.invertY;
    }

    public void Apply()
    {
        // save setting
        SettingsData.invertY = invertYToggle.isOn;

        // return to previous scene
        SceneManager.LoadScene(previousScene);
    }

    public void Back()
    {
        string sceneToLoad = previousScene;

        // Safety fallback
        if (string.IsNullOrEmpty(sceneToLoad))
        {
            sceneToLoad = "MainMenu";
        }

        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneToLoad);
    }
}