using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InstrUIInterSim : MonoBehaviour
{
    [SerializeField]
    private InputActionReference Activate;
    
    private bool selected = false;

    //How to seelct with NEar/Far Plane 
    private void OnEnable()
    {
        Activate.action.started += SwitchImg;
       
    }
    private void OnDisable()
    {
        Activate.action.started -= SwitchImg;
    }

    private void SwitchImg(InputAction.CallbackContext context)
    {
       
            
    }
    private void SelectItem(InputAction.CallbackContext context)
    {

    }

}
