using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

public class SceneMisMatchHandler : MonoBehaviour
{
   public enum ObjType
   {
        XR,
        _rMenuToClickPanel,
        _participantTryPanel,
   }

    public ObjType _objType;
    private void Start()
    {
        if (_objType == ObjType._rMenuToClickPanel)
        {
            FindObjectOfType<SettingMenuControls>()._rMenuToClickPanel = this.gameObject;
        }
        else if (_objType == ObjType._participantTryPanel)
        {
            FindObjectOfType<SettingMenuControls>()._participantTryPanel = this.gameObject;
        }
    }
}
