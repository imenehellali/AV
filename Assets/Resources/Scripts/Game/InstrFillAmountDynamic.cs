using System.Collections;
using System.Collections.Generic;
using Unity.XR.PXR;
using UnityEngine;
using UnityEngine.UI;

public class InstrFillAmountDynamic : MonoBehaviour
{
    [SerializeField]
    private Image _image;


    public void FillAmount(float amount)
    {
        if (_image.fillAmount < 1 && _image.fillAmount > 0)
            _image.fillAmount += amount;
    }
}
