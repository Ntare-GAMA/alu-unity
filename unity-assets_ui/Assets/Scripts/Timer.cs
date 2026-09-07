using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Timer : MonoBehaviour
{
    public Text timerText; // Reference to the UI Text component
    private float elapsedTime = 0f;
    private bool isRunning = true; // NEW: flag to stop the timer
    private Coroutine timerCoroutine;

    void Start()
    {
        timerText.text = FormatTime(elapsedTime);
        timerCoroutine = StartCoroutine(UpdateTimer());
    }

    private IEnumerator UpdateTimer()
    {
        while (isRunning) // <- Use the flag instead of "while(true)"
        {
            yield return new WaitForSeconds(0.1f);
            elapsedTime += 0.1f;
            timerText.text = FormatTime(elapsedTime);
        }
    }

    private string FormatTime(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60);
        float seconds = time % 60;
        return string.Format("{0:00}:{1:00.00}", minutes, seconds);
    }

    // Call this method to stop the timer
    public void StopTimer()
    {
        isRunning = false;
        if (timerCoroutine != null)
        {
            StopCoroutine(timerCoroutine);
        }
    }

    public void Win()
    {
        StopTimer();

        // Find WinCanvas FinalTime text
        GameObject winCanvas = GameObject.Find("WinCanvas");

        if (winCanvas != null)
        {
            Transform finalTimeObj = winCanvas.transform.Find("FinalTime");

            if (finalTimeObj != null)
            {
                UnityEngine.UI.Text finalText = finalTimeObj.GetComponent<UnityEngine.UI.Text>();

                if (finalText != null)
                {
                    finalText.text = "Final Time: " + timerText.text;
                }
            }
        }
    }
}
