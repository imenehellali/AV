using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BlockSpinButton : MonoBehaviour
{
    [SerializeField] private Button spinButton;
    
    public void BlockButton()
    {
        spinButton.interactable = false;
    }
    public void UnblockButton()
    {
        spinButton.interactable = true; 
    }

}
