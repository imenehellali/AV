using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;

public class PopulateScene : MonoBehaviour
{
    void Start()
    {
        XRInteractionManager xriManager = ThrillMinerManager.Instance._xrManager;

        List<TeleportationAnchor> _teleportsAncs = gameObject.GetComponentsInChildren<TeleportationAnchor>(false).ToList();
      
        if (_teleportsAncs.Count > 0) _teleportsAncs.ForEach(teleport => { teleport.interactionManager = xriManager; });

    }

}
