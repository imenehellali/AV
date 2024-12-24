using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.XR.PXR;
using UnityEngine;

public class TMTeleportHandler : MonoBehaviour
{
    [SerializeField]
    private PathSetting _from;
    [SerializeField]
    private PathSetting _to;

    private void OnTriggerEnter(Collider other)
    {
        if (other != null && !other.GetComponent<PXR_Manager>().IsUnityNull())
        {
            ThrillMinerManager.Instance.chosenPath.Invoke(_from,_to);
        }
    }
}
