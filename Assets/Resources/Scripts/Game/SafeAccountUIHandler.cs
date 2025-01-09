using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SafeAccountUIHandler : MonoBehaviour
{
    private bool canAdd=true;
    private bool canSub=true;
    private float amount = 0f;
    
    [SerializeField]
    private TextMeshProUGUI _amount;
    private EndLevelManager _endLevel;

    public void InitializeSafeAmount(float amount)
    {
        this.amount = amount;
        _amount.text = "\u20AC" + ((int)amount).ToString("N2");
    }
    private void Start()
    {
        _endLevel = FindObjectOfType<EndLevelManager>();
    }
    public void AddAmount()
    {
        if(canAdd)
        {
            _endLevel?.updateBalance.Invoke(amount);
            canAdd = false;
            canSub = true;
        }
    }
    public void SubAmount()
    {
        if (canSub)
        {
            _endLevel?.updateBalance.Invoke(-amount);
            canAdd = true;
            canSub = false;
        }
    }
}
