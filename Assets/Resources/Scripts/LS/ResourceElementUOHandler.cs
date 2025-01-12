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
    public UnityAction StopResource;


    private void OnEnable()
    {
        AmountUpdate += ResourceUI;
        TimeOut += ActivateButton;
        StopResource += KillObj;
    }
    private void OnDisable()
    {
        AmountUpdate -= ResourceUI;
        TimeOut -= ActivateButton;
        StopResource -= KillObj;
    }
    private void ActivateButton(bool activated)
    {
        Debug.Log("diactivating button");

        assignButton.SetActive(activated);
    }
    private void KillObj()
    {
        if (assignButton.activeSelf)
            assignButton.SetActive(false);
        if(resourceObjUI.activeSelf)
            resourceObjUI.SetActive(false);
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
