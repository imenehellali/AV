
using UnityEngine;

public abstract class ETObject : MonoBehaviour
{
    public bool isFocused = false;
    private float offsetTracking = 0.5f; // 0.5 seconds offset
    private float unfocusedTime;
    private bool timerRunning = false;

    public virtual void IsFocused()
    {
        if (timerRunning && Time.deltaTime - unfocusedTime < offsetTracking)
        {
            // Consider it as if it was still focused
            timerRunning = false;
        }
        isFocused = true;

    }

    public virtual void UnFocused()
    {
        isFocused = false;
        unfocusedTime = Time.deltaTime;
        timerRunning = true;
    }

    public virtual bool IsGazeLocked() => isFocused; // && !timerRunning;
}
