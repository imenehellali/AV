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
    private ResourceElement _element;
    [SerializeField]
    private GameObject resourceObjUI;
    [SerializeField]
    private TextMeshProUGUI resourceObjamount;
    private void Start()
    {
        _element = GetComponent<ResourceElement>();
    }

    private void OnEnable()
    {
        if (_element != null)
        {
            _element.Assignable += ActivateButton;
            _element.AmountChanged -= DisactivateResourceUI;
        }
    }

    private void OnDisable()
    {
        if (_element != null)
        {
            _element.Assignable -= ActivateButton;
            _element.AmountChanged-=DisactivateResourceUI;
        }
    }
   
    private void ActivateButton(bool activated)
    {
        assignButton.SetActive(activated);
    }

    private void DisactivateResourceUI(int amount, ResourceElement.Type type)
    {
        if (amount<=0) 
            resourceObjUI.SetActive(false);
        else
            resourceObjamount.text = amount.ToString();
    }

}
