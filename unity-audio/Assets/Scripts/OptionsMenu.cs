using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Audio;

public class OptionsMenu : MonoBehaviour
{
    public Toggle invertY;
    private string previousScene;
    private bool invert;
    public AudioMixer audioMixer;
    public Slider bgmSlider;
    public Slider sfxSlider;
    private const string BGM_VOLUME_KEY = "BGMVolume";
    private const string SFX_VOLUME_KEY = "SFXVolume";
    private const float VOLUME_MIN_DB = -80f;
    private const float VOLUME_MAX_DB = 0f;
    private float currentBGMValue;
    private float currentSFXValue;
    

    private void Start()
    {
        previousScene = PlayerPrefs.GetString("Previous", "MainMenu");
        invertY.isOn = PlayerPrefs.GetInt("InvertY", 0) == 1;
        LoadBGMVolume();
        bgmSlider.onValueChanged.AddListener(HandleBGMSliderChanged);
        sfxSlider.onValueChanged.AddListener(HandleSFXSliderChanged);
    }

    public void Back()
    {
        float savedVolume = PlayerPrefs.GetFloat(BGM_VOLUME_KEY, 1f);
        float savedSFXVolume = PlayerPrefs.GetFloat(SFX_VOLUME_KEY, 1f);
        SetBGMVolume(savedVolume);
        SetSFXVolume(savedSFXVolume);
        SceneManager.LoadScene(previousScene);
    }

    public void Apply()
    {
        invert = invertY.isOn;
        PlayerPrefs.SetInt("InvertY", invert ? 1 : 0);
        PlayerPrefs.SetFloat(BGM_VOLUME_KEY, currentBGMValue);
        PlayerPrefs.SetFloat(SFX_VOLUME_KEY, currentSFXValue);
        SceneManager.LoadScene(previousScene);
    }

    private void LoadBGMVolume()
    {
        float savedVolume = PlayerPrefs.GetFloat(BGM_VOLUME_KEY, 1f);
        float savedSfxVolume = PlayerPrefs.GetFloat(SFX_VOLUME_KEY, 1f);
        currentBGMValue = savedVolume;
        bgmSlider.value = savedVolume;
        currentSFXValue = savedSfxVolume;
        sfxSlider.value = savedSfxVolume;
        SetBGMVolume(savedVolume);
        SetSFXVolume(savedSfxVolume);
    }

    private void SetBGMVolume(float value)
    {
        float dbVal = Mathf.Lerp(VOLUME_MIN_DB, VOLUME_MAX_DB, value);
        if (value <= 0)
            dbVal = -80f;
        audioMixer.SetFloat("BGMVolume", dbVal);
    }

    private void HandleBGMSliderChanged(float value)
    {
        currentBGMValue = value;
        SetBGMVolume(value);
    }

    private void HandleSFXSliderChanged(float value)
    {
        currentSFXValue = value;
        SetSFXVolume(value);
        
    }

    private void SetSFXVolume(float value)
    {
        float dbVal = Mathf.Lerp(VOLUME_MIN_DB, VOLUME_MAX_DB, value);
        if (value <= 0)
            dbVal = -80f;
        audioMixer.SetFloat("SFXVolume", dbVal);
    }




}
