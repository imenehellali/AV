
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Climbing;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;
using static PathSetting;

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
    public int VisitCount = -1;
    public List<float> avgSpentTime = new List<float>();
    public bool isPassed { get; set; }

    private float avgTimeOfSuccess = 0f;
    private float timeSpent = 0f;
    private bool startTimer = false;



    public AudioClip _rpInstr;
    public List<AudioClip> _tryCount;

    public List<PathSetting> nextNodes = new List<PathSetting>();
    public List<PathSetting> prevNodes = new List<PathSetting>();
    private List<float> spentTime = new List<float>();

    public Transform startPos;
    public GameObject RPLevelObjects;
    private GameObject _currRPObjs;
    public XRInteractionManager _xrManager;
    private Dictionary<int, List<PathSetting>> _allPaths = new Dictionary<int, List<PathSetting>>();

    public float GetAverageTime()
    {
        return avgSpentTime.Count > 0 ? avgSpentTime.Average() : 0f;
    }

    public float GetTimeChangeRatio()
    {
        if (avgSpentTime.Count < 2) return 0f;
        float lastTime = avgSpentTime[^1];
        float secondLastTime = avgSpentTime[^2];
        return (lastTime - secondLastTime) / Mathf.Abs(secondLastTime);
    }
    public void AddVisit()
    {
        if (rp != RP.Start)
        {
            AddSpentTime();
            VisitCount++;
            avgTimeOfSuccess = (avgTimeOfSuccess + spentTime[spentTime.Count - 1]) / 2f;
            avgSpentTime.Add(avgTimeOfSuccess);
        }
    }
    public void AddSpentTime()
    {
        if (rp != RP.Start)
        {
            startTimer = false;
            spentTime.Add(timeSpent);
            timeSpent = 0f;
        }
    }
    private void Update()
    {
       if(startTimer) timeSpent += Time.deltaTime;
    }
    public void ResetRPObjs()
    {
        startTimer = true;
        timeSpent = 0f;

        if (rp != RP.Start)
        {
            if (remainingTrials < 0)
            {
                remainingTrials = 2;
            }

            if (_currRPObjs != null)
            {
                Destroy(_currRPObjs);
                _currRPObjs = null;
            }
            _currRPObjs = Instantiate(RPLevelObjects);
            List<TMObjects> _rpObjs = _currRPObjs.GetComponentsInChildren<TMObjects>(false)?.ToList();
            List<TeleportationAnchor> _teleportsAncs=_currRPObjs.GetComponentsInChildren<TeleportationAnchor>(false)?.ToList();
            List<ClimbInteractable> _climbs= _currRPObjs.GetComponentsInChildren<ClimbInteractable>(false)?.ToList();

           if(_rpObjs.Count>0) _rpObjs.ForEach(rp => { rp.belongsTo = this; });
           if(_teleportsAncs.Count>0) _teleportsAncs.ForEach(teleport => {teleport.interactionManager=_xrManager; }); 
           if(_climbs.Count>0) _climbs.ForEach(climbs => {climbs.interactionManager=_xrManager; }); 
        }

    }

    public void RemoveRPObjs()
    {
       
        if (rp != RP.Start)
        {
            remainingTrials = 2; 
            startTimer = false;
            timeSpent = 0f;

            if (_currRPObjs != null)
            {
                Destroy(_currRPObjs);
                _currRPObjs = null;
            }
        }

    }
    public async Task< Dictionary<int, List<PathSetting>>> GetAllPaths()
    {
        if(rp==RP.R4P4)
            await Task.Run(() => GeneratePaths(this, _allPaths));
        return _allPaths;
    }
    private async Task GeneratePaths(PathSetting startNode, Dictionary<int, List<PathSetting>> allPaths)
    {
        if (startNode.rp != RP.R4P4)
            return;

        var initialPath = new List<PathSetting>();
        await Task.Run(() => RecursivePath(startNode, initialPath, 0, allPaths));
    }

    private async Task RecursivePath(PathSetting currentNode, List<PathSetting> currentPath, int currPathId, Dictionary<int, List<PathSetting>> allPaths)
    {
        currentPath.Add(currentNode);

        if (currentNode.rp == RP.Start)
        {
            if (!allPaths.ContainsKey(currPathId))
            {
                allPaths[currPathId] = new List<PathSetting>(currentPath);
            }
            return;
        }

        if (currentNode.prevNodes.Count > 1)
        {
            foreach (var prevNode in currentNode.prevNodes)
            {
                var newPath = new List<PathSetting>(currentPath);
                await Task.Run(() => RecursivePath(prevNode, newPath, allPaths.Count, allPaths));
            }
        }
        else if (currentNode.prevNodes.Count == 1)
        {
            await Task.Run(() => RecursivePath(currentNode.prevNodes[0], currentPath, currPathId, allPaths));
        }
    }
}

