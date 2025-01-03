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
        Debug.Log("Blocking spin button");
    }
    public void UnblockButton()
    {
        spinButton.interactable = true;
        Debug.Log(" unblocking spin button ");
    }

}
