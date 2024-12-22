using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.XR.PXR;
using UnityEngine;

public class PathSetting : MonoBehaviour
{
    public enum RP
    {
        Start,
        R4P4,
        R3P4,
        R3P3,
        R2P3,
        R2P2,
        R1P4,
        R1P3,
        R1P2,
        R1P1,
    }
    public RP rp;
    public int remainingTrials = 2;
    public PathSetting _prevNode;
    public List<PathSetting> nextNodes;
    public Transform startPos;
    public GameObject RPLevelObjects;
    private GameObject _currRPObjs;

    private void Start()
    {
        _currRPObjs = Instantiate(RPLevelObjects);
    }
    public void ResetRPObjs()
    {
        Destroy(_currRPObjs.gameObject);
        _currRPObjs = Instantiate(RPLevelObjects);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other != null && !other.GetComponent<PXR_Manager>().IsUnityNull())
        {
            ThrillMinerManager.Instance.chosenPath.Invoke(this);
        }
    }


}
