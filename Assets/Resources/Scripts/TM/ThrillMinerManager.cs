
using System.Collections;
using System.Collections.Generic;
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


    public UnityAction<int> addAmount;
    public UnityAction<PathSetting> questFailed;
    public UnityAction<PathSetting> chosenPath;

    [SerializeField]
    private List<GameObject> allPathNodes = new List<GameObject>();
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
    private void AdvancePath(PathSetting node)
    {
        // Reposition participant
        FindAnyObjectByType<PXR_Manager>().gameObject.transform.SetPositionAndRotation(node.startPos.position, node.startPos.rotation);

        //Unload Other scenePaths
        node._prevNode.nextNodes.ForEach(x =>
        {
            if (x.rp != node.rp)
            {
                string _sceneName = x.rp.ToString();
                SceneManager.UnloadSceneAsync(SceneManager.GetSceneByName(_sceneName).buildIndex);
            }
        });

        // Unload prev scene
        SceneManager.UnloadSceneAsync(node._prevNode.rp.ToString());

        // Load other possible scenes
        node.nextNodes.ForEach(x =>
        {
            string _sceneName = x.rp.ToString();
            SceneManager.LoadSceneAsync(SceneManager.GetSceneByName(_sceneName).buildIndex, LoadSceneMode.Additive);
        });
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
        else if (node.remainingTrials<0 && node.rp!=PathSetting.RP.R4P4 && remainingTime>=-0.5f)
        {
            RestartPath(node);
        }
        else if (node.remainingTrials < 0 && node.rp== PathSetting.RP.R4P4 && remainingTime >= -0.5f)
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
            if (x.rp != node.rp)
            {
                string _sceneName = x.rp.ToString();
                SceneManager.UnloadSceneAsync(SceneManager.GetSceneByName(_sceneName).buildIndex);
            }
        });

        //Unload the current node
        SceneManager.UnloadSceneAsync(node.rp.ToString());
    }

    public void StarLevel()
    {

    }
}
