
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Unity.XR.PXR;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class ThrillMinerManager : MonoBehaviour
{
    //While(ThrillMinerManager.Instance.Exists --> XROrig Do not destroyOnLoad --> else XROrig destroy)
    public static ThrillMinerManager Instance { get; private set; }

    private int temporaryAmount = 0;
    private float remainingTime = 300f;
    [SerializeField]
    private PathSetting _startPath;

    public UnityAction<int> addAmount;
    public UnityAction<PathSetting> questFailed;
    public UnityAction<PathSetting, PathSetting> chosenPath;

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
                x.ResetRPObjs();
                SceneManager.LoadSceneAsync(SceneManager.GetSceneByName(_sceneName).buildIndex, LoadSceneMode.Additive);
            });
        }
    }
    private void AdvancePath(PathSetting _from, PathSetting _to)
    {
        StartCoroutine(Advance(_from, _to));
    }
    private void AddTempAmount(int amount)
    {
        temporaryAmount += amount;
    }
    private void QuestFailed(PathSetting node)
    {
        --node.remainingTrials;
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
            _x.ResetRPObjs();
            SceneManager.LoadSceneAsync(SceneManager.GetSceneByName(_x.rp.ToString()).buildIndex, LoadSceneMode.Additive);
        });

    }

    public void StartLevel()
    {
       
    }
}
