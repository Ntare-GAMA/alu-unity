using UnityEngine;
using UnityEngine.UI;

public class WinTrigger : MonoBehaviour
{
    public GameObject winCanvas;   // NEW

    public int winFontSize = 60;
    public Color winColor = Color.green;

    private Timer timer;
    private Text timerText;
    private bool hasWon = false;

    void Start()
    {
        GameObject player = GameObject.FindWithTag("Player");

        if (player != null)
        {
            timer = player.GetComponent<Timer>();

            if (timer != null)
            {
                timerText = timer.timerText;
            }
        }

        // Make sure win screen is hidden at start
        if (winCanvas != null)
        {
            winCanvas.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasWon) return;

        if (other.CompareTag("Player"))
        {
            hasWon = true;
            Win();
        }
    }

    private void Win()
    {
        if (timer != null)
        {
            timer.Win(); // NEW: send final time
        }

        if (winCanvas != null)
        {
            winCanvas.SetActive(true);
        }

        Debug.Log("Player Wins!");
    }
}