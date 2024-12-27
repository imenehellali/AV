
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.XR.PXR;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class ThrillMinerManager : MonoBehaviour
{
    //While(ThrillMinerManager.Instance.Exists --> XROrig Do not destroyOnLoad --> else XROrig destroy)

    public class ChosenPath
    {
        public bool pathVisited;
        public List<PathSetting> pathNodes;
        public float percentageOfProgress;
        public float incDecOverNodes;
    }

    public static ThrillMinerManager Instance { get; private set; }

    private int temporaryAmount = 0;
    private float remainingTime = 300f;
    [SerializeField]
    private PathSetting _startPath;
    [SerializeField]
    private AudioSource _audioSource;
    [SerializeField]
    private AudioClip _reachedR4P4;

    [Header("Scene Background Sound")]
    [SerializeField]
    private AudioSource _bgAudioSource;
    [SerializeField]
    private AudioClip _bgClip;


    public UnityAction<int> addAmount;
    public UnityAction<PathSetting> questFailed;
    public UnityAction<PathSetting, PathSetting> chosenPath;

    public List<ChosenPath> _pathList = new List<ChosenPath>();


    private void OnEnable()
    {
        addAmount += AddTempAmount;
        questFailed += QuestFailed;
        chosenPath += AdvancePath;
    }
    private void OnDisable()
    {
        addAmount -= AddTempAmount;
        questFailed -= QuestFailed;
        chosenPath -= AdvancePath;
    }
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void BGTMSceneHandler()
    {
        if (!_bgAudioSource.isPlaying)
            _bgAudioSource.PlayOneShot(_bgClip);
    }
    private IEnumerator PlaySounds(AudioClip _instr, AudioClip _count, bool R4P4)
    {
        if (R4P4)
        {
            _audioSource.PlayOneShot(_reachedR4P4);
            yield return _reachedR4P4.length;
        }
        _audioSource.PlayOneShot(_instr);
        yield return _instr.length;
        _audioSource.PlayOneShot(_count);
    }
    private IEnumerator Advance(PathSetting _from, PathSetting _to)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(_to.rp.ToString(), LoadSceneMode.Additive);
        _to.ResetRPObjs();
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
        if (asyncLoad.isDone)
        {
            // Reposition participant
            FindAnyObjectByType<PXR_Manager>().gameObject.transform.SetPositionAndRotation(_to.startPos.position, _to.startPos.rotation);
            StartCoroutine(PlaySounds(_to._rpInstr, _to._tryCount[_to.remainingTrials], _to.rp == PathSetting.RP.R4P4));
            //Unload Other scenePaths
            _from.nextNodes.ForEach(x =>
            {
                if (x.rp != _to.rp)
                {
                    string _sceneName = x.rp.ToString();
                    x.RemoveRPObjs();
                    SceneManager.UnloadSceneAsync(SceneManager.GetSceneByName(_sceneName).buildIndex);
                }
            });

            // Unload prev scene
            if (_from.rp != PathSetting.RP.Start)
            {
                SceneManager.UnloadSceneAsync(_from.rp.ToString());
                _from.RemoveRPObjs();
            }


            // Load other possible scenes
            _to.nextNodes.ForEach(x =>
            {
                string _sceneName = x.rp.ToString();
                SceneManager.LoadSceneAsync(SceneManager.GetSceneByName(_sceneName).buildIndex, LoadSceneMode.Additive);
            });
        }
    }
    private void AdvancePath(PathSetting _from, PathSetting _to)
    {
        _from.AddVisit();
        _from.isPassed = true;  
        _to.isPassed = false;
        if (_to.rp == PathSetting.RP.Start)
        {
            EndLevel(_from);
            return;
        }
        StartCoroutine(Advance(_from, _to));

    }
    private void AddTempAmount(int amount)
    {
        temporaryAmount += amount;
    }
    private void QuestFailed(PathSetting node)
    {
        --node.remainingTrials;
        node.AddSpentTime();
        node.isPassed = false;

        if (node.remainingTrials >= 0 && node.rp != PathSetting.RP.R4P4 && remainingTime >= -0.5f)
        {
            //Restart Current Node
            node.ResetRPObjs();
            FindAnyObjectByType<PXR_Manager>().gameObject.transform.SetPositionAndRotation(node.startPos.position, node.startPos.rotation);
        }
        else if (node.remainingTrials < 0 && node.rp != PathSetting.RP.R4P4 && remainingTime >= -0.5f)
        {
            RestartPath(node);
        }
        else if (node.remainingTrials < 0 && node.rp == PathSetting.RP.R4P4 && remainingTime >= -0.5f)
        {
            temporaryAmount = 0;
            RestartPath(node);
        }
    }

    private void RestartPath(PathSetting node)
    {
        //Set position to start --> TMShould be already loaded and never unloaded anyways
        FindAnyObjectByType<PXR_Manager>().gameObject.transform.SetPositionAndRotation(new Vector3(0f, 0.1f, 0f), Quaternion.identity);
        BGTMSceneHandler();

        //Unload next possible Scene nodes
        node.nextNodes.ForEach(x =>
        {
            string _sceneName = x.rp.ToString();
            x.RemoveRPObjs();
            SceneManager.UnloadSceneAsync(SceneManager.GetSceneByName(_sceneName).buildIndex);

        });

        //Unload the current node
        SceneManager.UnloadSceneAsync(node.rp.ToString());
        node.RemoveRPObjs();
        //Load Initial Paths Choices  --> Load R1P3, R1P4, R1P2, R1P1
        _startPath.nextNodes.ForEach(_x =>
        {
            SceneManager.LoadSceneAsync(SceneManager.GetSceneByName(_x.rp.ToString()).buildIndex, LoadSceneMode.Additive);
        });

    }

    public void StartLevel()
    {
        BGTMSceneHandler();
        
    }
 /*   public void TestChosenPathAsync()
    {
        ParticipantSettings.Instance.PID.Invoke("CG00010");
      
        var R4P4 = new PathSetting
        {
            rp = PathSetting.RP.R4P4,
            VisitCount = 4,
            avgSpentTime = new List<float> { 11,13 },
            remainingTrials = 1,
            isPassed = true
        }; // Success

        var R1P1 = new PathSetting
        {
            rp = PathSetting.RP.R1P1,
            VisitCount = 2,
            avgSpentTime = new List<float> { 6 },
            remainingTrials = 0,
            isPassed = true
        }; // Learning factor applied

        var R1P2 = new PathSetting
        {
            rp = PathSetting.RP.R1P2,
            VisitCount = 1,
            avgSpentTime = new List<float> { },
            remainingTrials = 2,
            isPassed = true
        }; // Not visited

        var R1P3 = new PathSetting
        {
            rp = PathSetting.RP.R1P3,
            VisitCount = 1,
            avgSpentTime = new List<float> { 6 },
            remainingTrials = 0,
            isPassed = true
        };

        var R1P4 = new PathSetting
        {
            rp = PathSetting.RP.R1P4,
            VisitCount = 1,
            avgSpentTime = new List<float> { },
            remainingTrials = 1,
            isPassed = true
        };

        var R2P2 = new PathSetting
        {
            rp = PathSetting.RP.R2P2,
            VisitCount = 1,
            avgSpentTime = new List<float> { 15 },
            remainingTrials = 1
        };

        var R3P3 = new PathSetting
        {
            rp = PathSetting.RP.R3P3,
            VisitCount = 1,
            avgSpentTime = new List<float> { },
            remainingTrials = 2
        }; // Not visited

        var R3P4 = new PathSetting
        {
            rp = PathSetting.RP.R3P4,
            VisitCount = 2,
            avgSpentTime = new List<float> { 20, 18 },
            remainingTrials = 1,
            isPassed = true
        };

        // Simulate paths
        var _allPaths = new Dictionary<int, List<PathSetting>>
    {
        { 0, new List<PathSetting> { R1P1, R1P2, R1P3, R1P4, R3P4,R4P4} }, // Ends successfully
        { 1, new List<PathSetting> { R1P1, R2P2, R3P3, R4P4 } }, // Reaches R4P4 but fails
        { 2, new List<PathSetting> { R1P1, R1P3, R1P4, R4P4 } },
    };

        var _pathList = new List<ThrillMinerManager.ChosenPath>();

        foreach (var singlePath in _allPaths.Values)
        {
            ThrillMinerManager.ChosenPath newPath = new ThrillMinerManager.ChosenPath
            {
                pathNodes = singlePath,
                percentageOfProgress = 0f,
                incDecOverNodes = 0f
            };

            float totalTime = 0f;
            int successfullyPassedNodes = 0; // Track nodes where isPassed == true

            for (int i = 0; i < singlePath.Count; i++)
            {
                PathSetting currentNode = singlePath[i];
                totalTime += currentNode.avgSpentTime.Count > 0 ? currentNode.avgSpentTime.Average() : 0;

                if (currentNode.isPassed) successfullyPassedNodes++; // Count only successfully passed nodes

                if (i > 0)
                {
                    PathSetting previousNode = singlePath[i - 1];
                    float ratio = previousNode.avgSpentTime.Count > 0
                        ? (currentNode.avgSpentTime.Count > 0 ? currentNode.avgSpentTime.Average() / previousNode.avgSpentTime.Average() : 0)
                        : 0;
                    newPath.incDecOverNodes += ratio;
                }
            }

            // Update the percentage of progress based on successfully passed nodes
            newPath.percentageOfProgress = (float)successfullyPassedNodes / singlePath.Count;
            newPath.incDecOverNodes /= (singlePath.Count - 1);
            _pathList.Add(newPath);
        }

        // Run TMStats logic
        TMStats.ChosenPath(_pathList);
        Debug.Log("Done with the chosen path");
       
        ParticipantSettings.Instance.SaveRawParticipantData();

    }
    */

    public void EndLevel(PathSetting R4P4)
    {
        Dictionary<int, List<PathSetting>> _allPaths = R4P4.GetAllPaths().Result;

        foreach (var singlePath in _allPaths.Values)
        {
            singlePath.Remove(_startPath);

            ChosenPath newPath = new ChosenPath
            {
                pathNodes = singlePath,
                percentageOfProgress = 0f,
                incDecOverNodes = 0f
            };

            float totalTime = 0f;
            int successfullyPassedNodes = 0; // Track nodes where isPassed == true

            for (int i = 0; i < singlePath.Count; i++)
            {
                PathSetting currentNode = singlePath[i];
                totalTime += currentNode.avgSpentTime.Count > 0 ? currentNode.avgSpentTime.Average() : 0;

                if (currentNode.isPassed) successfullyPassedNodes++; // Count only successfully passed nodes

                if (i > 0)
                {
                    PathSetting previousNode = singlePath[i - 1];
                    float ratio = previousNode.avgSpentTime.Count > 0
                        ? (currentNode.avgSpentTime.Count > 0 ? currentNode.avgSpentTime.Average() / previousNode.avgSpentTime.Average() : 0)
                        : 0;
                    newPath.incDecOverNodes += ratio;
                }
            }

            // Update the percentage of progress based on successfully passed nodes
            newPath.percentageOfProgress = (float)successfullyPassedNodes / singlePath.Count;
            newPath.incDecOverNodes /= (singlePath.Count - 1);
            _pathList.Add(newPath);
        }
    }


}
