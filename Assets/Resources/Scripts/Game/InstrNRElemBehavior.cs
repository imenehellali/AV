using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class InstrNRElemBehavior :ETObject
{
    [SerializeField]
    private InputActionReference Activate;
    private Image _highlightSelection;
    [SerializeField]
    private Type _type;

    public enum Type
    {
        Alcohol,
        NonAlcohol,
    }


    private void OnEnable()
    {
        Activate.action.started += ShootElement;
        _highlightSelection=GetComponentInParent<Image>();  
    }

    private void ShootElement(InputAction.CallbackContext context)
    {
        Destroy(this.gameObject);
    }

    private void OnDisable()
    {
        Activate.action.started -= ShootElement;
    }
    public override void IsFocused()
    {
        base.IsFocused();
         _highlightSelection.color = _type.Equals(Type.NonAlcohol) ? Color.cyan : Color.magenta;
    }

    public override void UnFocused()
    {
        base.UnFocused();
        _highlightSelection.color = Color.white;
    }
}
