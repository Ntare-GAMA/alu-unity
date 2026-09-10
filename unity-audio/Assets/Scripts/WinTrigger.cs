using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class WinTrigger : MonoBehaviour
{
    public GameObject winScreen;
    public Timer timer;
    public AudioSource source;
    private void OnTriggerEnter(Collider other)
    {
        source.enabled = false;
        winScreen.SetActive(true);
        if (other.gameObject.CompareTag("Player"))
        {
            FindObjectOfType<AudioManager>().Play("Victory");
            timer.StopTimer();
        }
    }
}
