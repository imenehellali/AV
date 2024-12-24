using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    public List<PathSetting> nextNodes=new List<PathSetting>();
    public List<PathSetting> prevNodes = new List<PathSetting>();

    public Transform startPos;
    public GameObject RPLevelObjects;
    private GameObject _currRPObjs;

    public void ResetRPObjs()
    {
        if (rp!=RP.Start)
        {
            if (_currRPObjs != null)
            {
                Destroy(_currRPObjs);
                _currRPObjs = null;
            }
            _currRPObjs = Instantiate(RPLevelObjects);
            List<TMObjects> _rpObjs = _currRPObjs.GetComponentsInChildren<TMObjects>(false).ToList();
            Debug.Log($"count of TMObjs {_rpObjs.Count}");
            _rpObjs.ForEach(rp => { rp.belongsTo = this; });
        }
      
    }
    public void RemoveRPObjs()
    {
        if (rp != RP.Start)
        {
            if (_currRPObjs != null)
            {
                Destroy(_currRPObjs);
                _currRPObjs = null;
            }
        }

    }

}
