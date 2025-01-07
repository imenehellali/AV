using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class RepositionOnLoad : MonoBehaviour
{
    public UnityAction<string> repositionOnLoad;
    public static RepositionOnLoad Instance {  get; private set; }
    [SerializeField]
    private GameObject _settingManager;
    [SerializeField]
    private GameObject _instrPanel;
    [SerializeField]
    private SettingMenuControls _settingControls;
    public GameObject _participant;
    [SerializeField]
    private RectTransform _taskProgressPanel;
    private void Awake()
    {
        if(Instance == null)
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
       _participant.transform.position = new Vector3(0f, 0.1f, 0f);
        _participant.transform.rotation = Quaternion.identity;
        if (!levelName.Equals("StartScene"))
        {
           _settingManager.SetActive(false);
            _settingControls.enabled = false;
            _instrPanel.SetActive(true);
        }
        if(levelName.Equals("PUWScene"))
        {
            _participant.transform.position = new Vector3(-1.61399996f, 0.100000001f, 0.449999988f);
            _taskProgressPanel.position = new Vector3(-1.0446161f, 2.17185879f, 8.8579998f);
            _taskProgressPanel.rotation = Quaternion.identity;
            _participant.GetComponent<JumpManager>().enabled = false; 
            _participant.GetComponent<ClimbManager>().enabled = false;  

        }
        else if (levelName.Equals("GBScene"))
        {
            _taskProgressPanel.position = new Vector3(-5.30937386f, 2.5f, 6.61999989f);
            _taskProgressPanel.rotation = Quaternion.identity;
            _taskProgressPanel.Rotate(0f, 180f, 0f);
            _participant.GetComponent<JumpManager>().enabled = false;
            _participant.GetComponent<ClimbManager>().enabled = false;
        }
        else if (levelName.Equals("LSScene"))
        {
            _taskProgressPanel.position = new Vector3(3.44938493f, 1.62993073f, -3.97199988f);
            _taskProgressPanel.rotation = Quaternion.identity;
            _taskProgressPanel.Rotate(0f, 180f, 0f);
            _participant.GetComponent<JumpManager>().enabled = false;
            _participant.GetComponent<ClimbManager>().enabled = false;
        }
        else if (levelName.Equals("TMScene"))
        {
            _taskProgressPanel.position = new Vector3(2.82274318f, 2.5f, 5.48126125f);
            _taskProgressPanel.rotation = Quaternion.identity;
            _participant.GetComponent<JumpManager>().enabled = true;
            _participant.GetComponent<ClimbManager>().enabled = true;
        }
    }
}
