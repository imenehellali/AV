using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class ResourceElementUOHandler : MonoBehaviour
{
    [SerializeField]
    private GameObject assignButton;
  
    [SerializeField]
    private GameObject resourceObjUI;
    [SerializeField]
    private TextMeshProUGUI resourceObjamount;

    public UnityAction<bool> TimeOut;
    public UnityAction<int> AmountUpdate;


    private void OnEnable()
    {
        AmountUpdate += ResourceUI;
        TimeOut += ActivateButton;
    }
    private void OnDisable()
    {
        AmountUpdate -= ResourceUI;
        TimeOut -= ActivateButton;
    }
    private void ActivateButton(bool activated)
    {
        Debug.Log("diactivating button");

        assignButton.SetActive(activated);
    }

    private void ResourceUI(int amount)
    {
        Debug.Log("Updating amount UI case");

        if (amount<=0) 
            resourceObjUI.SetActive(false);
        else
            resourceObjamount.text = amount.ToString();
    }

}
