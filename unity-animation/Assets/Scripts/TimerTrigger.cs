using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimerTrigger : MonoBehaviour
{
    public Timer timer;
    // Start is called before the first frame update
    void Start()
    {
        timer.enabled = false;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            timer.enabled = true;
            timer.StartTimer();
        }
    }
}
