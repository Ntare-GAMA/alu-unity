using UnityEngine;

public class TimerTrigger : MonoBehaviour
{
    private Timer timer; // Reference to the Timer script on Player

    void Start()
    {
        // Find the Timer script attached to the Player
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            timer = player.GetComponent<Timer>();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Check if the object exiting is the Player
        if (other.CompareTag("Player"))
        {
            // Enable the Timer script and start counting
            if (timer != null)
            {
                timer.enabled = true; // Enable the Timer script
            }
        }
    }
}