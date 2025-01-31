using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class RewardObject : ETObject
{

    [SerializeField] TextMeshProUGUI _testDuration;
    private float focusTime;
    private bool isTimerRunning;
    private List<float> focusDurations = new List<float>();
    private float offsetRewardTracking = 0.5f;
    private float unfocusTimer = 0f;
    private bool isUnfocusTimerRunning = false;

    private void Update()
    {
        if (isFocused && isTimerRunning)
        {
            focusTime += Time.deltaTime;
            _testDuration.text = focusTime.ToString();
        }
        if (isUnfocusTimerRunning)
        {
            unfocusTimer += Time.deltaTime;

            if (unfocusTimer >= offsetRewardTracking)
            {
                isUnfocusTimerRunning = false;
                AddFocusDuration();
            }
        }
    }

    public override void IsFocused()
    {
        if (isUnfocusTimerRunning)
        {
            isUnfocusTimerRunning = false;
        }
        else
        {
            base.IsFocused();
            isTimerRunning = true;
        }
        GhostBustBehavior _obj = gameObject.GetComponent<GhostBustBehavior>();
        if (_obj != null)
            _obj.SetGlow();
    }

    public override void UnFocused()
    {
        base.UnFocused();
        isTimerRunning = false;
        focusTime = 0f;
        unfocusTimer = 0f;
        isUnfocusTimerRunning = true;  // Start the offset timer
        GhostBustBehavior _obj = gameObject.GetComponent<GhostBustBehavior>();
        if (_obj != null)
        {
            _testDuration.text = "look at me plz :(";
            _obj.ResetGlow();
            if (focusDurations.Count > 0) _obj.focusDurations.Add(focusDurations[focusDurations.Count - 1]);
        }
    }
    private void AddFocusDuration()
    {
        focusDurations.Add(focusTime);
        _testDuration.text = focusDurations.Count.ToString();
    }
    public void UpdateTotalFixationTime()
    {
        float totalFocusTime = 0f;
        foreach (float duration in focusDurations)
        {
            totalFocusTime += duration;
        }

        PUWStats.AddFixationTimeAlcoholicDisplays(totalFocusTime);
    }
}
