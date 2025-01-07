using System.Collections;
using System.Collections.Generic;
using Unity.XR.CoreUtils;
using Unity.XR.PXR;
using UnityEngine;
using UnityEngine.XR.OpenXR.Input;

public class InstrSimAudioManager : MonoBehaviour
{
    [SerializeField]
    private AudioSource audioSource;
    [SerializeField]
    private SimL simL;
    [SerializeField]
    private GameObject simL3Canvas;
    public enum SimL
    {
        L1,
        L3,
        Default,
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponent<XROrigin>() != null)
        {
            audioSource.Play();
            if (simL == SimL.L3)
            {
                simL3Canvas.SetActive(true);
                InstrGBSimManager.setContinueGBSim.Invoke(true);
            }
            else if (simL == SimL.L1)
            {

            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.GetComponent<XROrigin>() != null)
        {
            audioSource.Stop();
            if (simL == SimL.L3)
            {
                simL3Canvas.SetActive(false);
            }
        }
    }

}
