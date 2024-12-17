using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Events;
using System;
public class TaskProgress : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI timer;
    [SerializeField] private TextMeshProUGUI moneyDisplay;
    [SerializeField] private GameObject instructionPanel;


    private float remainingTime;

    private void OnEnable()
    {
         MoneyManager.instance.OnMoneyWon+=UpdateMoneyDisplay;
        GameSettings.Instance.OnTimeUp+=UpdateTimer;

    }
    private void OnDisable()
    {
        MoneyManager.instance.OnMoneyWon -= UpdateMoneyDisplay;
        GameSettings.Instance.OnTimeUp -= UpdateTimer;
    }
    
    private void UpdateMoneyDisplay()=>moneyDisplay.text = "\u20AC" +((int)MoneyManager.instance.GetMoney()).ToString("N2");
    

    private void UpdateTimer(float time)
    {
        remainingTime = time;
        timer.text = FormatTime(remainingTime);
    }

    
    //Check this function ! i am pretty sure it does nothing .... 
    public void ToggleInstructionPanel()
    {
        instructionPanel.SetActive(!instructionPanel.activeSelf);
        if (!instructionPanel.activeSelf)
        {
            GameSettings.Instance.StartLevelTimer(); // Ensures timer continues without restarting
        }
    }

    private string FormatTime(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60F);
        int seconds = Mathf.FloorToInt(time % 60F);
        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }
}
