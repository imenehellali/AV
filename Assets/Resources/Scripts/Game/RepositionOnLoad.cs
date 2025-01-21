using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;

public class RepositionOnLoad : MonoBehaviour
{
    public UnityAction<string> repositionOnLoad;
    public static RepositionOnLoad Instance { get; private set; }
    [SerializeField]
    private GameObject _settingManager;
    [SerializeField]
    private GameObject _instrPanel;
    [SerializeField]
    private SettingMenuControls _settingControls;
    public GameObject _participant;
    [SerializeField]
    private RectTransform _taskProgressPanel;
    [SerializeField]
    private DynamicMoveProvider _dynamicMoveProvider;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            gameObject.transform.position = new Vector3(0f, 0.1f, 0f);
            Debug.Log("Set player from Reposition on load");

        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void OnEnable()
    {
        repositionOnLoad += RepositionXROnLoad;
    }
    private void OnDisable()
    {
        repositionOnLoad -= RepositionXROnLoad;
    }
    //if we have diff positions for each level then we can handle it here
    private void RepositionXROnLoad(string levelName)
    {
       
        if (levelName.Equals("EndScene"))
        {
            _participant.GetComponent<NavMeshAgent>().enabled = false;
            _participant.transform.position = new Vector3(0f, 0.1f, 0f);
            _participant.transform.rotation = Quaternion.identity;
            _participant.GetComponent<NavMeshAgent>().enabled = true;
            Debug.Log($"Set Participant Position to {_participant.transform.position}");

            _settingManager.SetActive(false);
            _settingControls.enabled = false;
            _instrPanel.SetActive(true);
        }
        else if (levelName.Equals("StartScene"))
        {
            _participant.transform.position = new Vector3(0f, 0.1f, 0f);
            _participant.transform.rotation = Quaternion.identity;

            _taskProgressPanel.position = new Vector3(-9.30000019f, 3.54999995f, -3.06999993f);
            _taskProgressPanel.rotation = Quaternion.identity;
            _taskProgressPanel.Rotate(new Vector3(0, 270f, 0));

            if (!_settingManager.gameObject.activeSelf)
            {
                _settingManager.SetActive(true);
                _settingControls.enabled = true;
                _settingControls.StartSettingAgain();

                _instrPanel.SetActive(false);
                Debug.Log("restarted settings from loaders becaus it was not enabled");
            }
            
        }
        else if (levelName.Equals("PUWScene"))
        {
            _participant.GetComponent<NavMeshAgent>().enabled = false;
            _participant.transform.position = new Vector3(1.19100022f, 0.00999999046f, 1.58800006f);
            _participant.transform.rotation = Quaternion.identity;
            _participant.GetComponent<NavMeshAgent>().enabled=true; 

            _taskProgressPanel.position = new Vector3(-1.0446161f, 2.17185879f, 8.8579998f);
            _taskProgressPanel.rotation = Quaternion.identity;

            _settingManager.SetActive(false);
            _settingControls.enabled = false;
            _instrPanel.SetActive(true);

        }
        else if (levelName.Equals("GBScene"))
        {
            _participant.GetComponent<NavMeshAgent>().enabled = false;
            _participant.transform.position = new Vector3(0f, 0.1f, 0f);
            _participant.transform.rotation = Quaternion.identity;
            _participant.GetComponent<NavMeshAgent>().enabled = true;

            _taskProgressPanel.position = new Vector3(-5.30937386f, 2.5f, 6.61999989f);
            _taskProgressPanel.rotation = Quaternion.identity;


            _settingManager.SetActive(false);
            _settingControls.enabled = false;
            _instrPanel.SetActive(true);
        }
        else if (levelName.Equals("LSScene"))
        {
            _participant.GetComponent<NavMeshAgent>().enabled = false;
            _participant.transform.position = new Vector3(0f, 0.1f, 0f);
            _participant.transform.rotation = Quaternion.identity; 
            _participant.GetComponent<NavMeshAgent>().enabled = true;

            _taskProgressPanel.position = new Vector3(3.44938493f, 1.62993073f, -3.97199988f);
            _taskProgressPanel.rotation = Quaternion.identity;
            _taskProgressPanel.Rotate(0f, 180f, 0f);

            _settingManager.SetActive(false);
            _settingControls.enabled = false;
            _instrPanel.SetActive(true);
        }
        
        if (!_dynamicMoveProvider.enabled)
            _dynamicMoveProvider.enabled = true;
    }
}
