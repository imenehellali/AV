using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.XR.PXR;
using Unity.XR.PXR.Input;
using UnityEngine;
using UnityEngine.InputSystem;

public class SettingMenuControls : MonoBehaviour
{
    public bool startTeilnahme=false;

    [Header("Controls")]
    [SerializeField]
    private InputActionReference Menu;
    [SerializeField]
    private InputActionReference StartGame;

    [Header("Wall Panels")]
    [SerializeField]
    private GameObject _rMenuToClickPanel;
    [SerializeField]
    private GameObject _participantTryPanel;

    [Header("Both Users")]
    [SerializeField]
    private GameObject _selectionPanel;
    [SerializeField]
    private GameObject _step0;

    [Header("Participant Bound Panels")]
    [SerializeField]
    private GameObject _step1;
    [SerializeField]
    private GameObject _inGameInstrPanel;

    [Header("Therapist Bound Panels")]
    [SerializeField]
    private GameObject _settingPanel;
    [SerializeField]
    private GameObject _settingStep0;
    [SerializeField]
    private GameObject _settingStep1;
    [SerializeField]
    private GameObject _settingStep2;
    [SerializeField]
    private GameObject _settingStep3;
    [SerializeField]
    private GameObject _settingStep4;
    [SerializeField]
    private AudioSource _setStep1AS;

    private int _CGidx = 0;
    private int _ADidx = 0;

   
    private bool _userTherapist = false;

    private void Awake()
    {
        _rMenuToClickPanel.SetActive(true);
        _participantTryPanel.SetActive(false);

        _selectionPanel.SetActive(false);
        _step0.SetActive(false);

        _step1.SetActive(false);
        _inGameInstrPanel.SetActive(false);

        _settingPanel.SetActive(false);
        _settingStep0.SetActive(false);

        _CGidx = AllParticiipantDataManager.Instance.getCGidx();
        _ADidx = AllParticiipantDataManager.Instance.getADidx();
    }
    private void OnEnable()
    {
        if(!Menu.IsUnityNull())
        Menu.action.started += OpenSettingMenu;
        if (!StartGame.IsUnityNull())
            StartGame.action.started += LaunchGame;

    }
    private void OnDisable()
    {
        if (!Menu.IsUnityNull())
            Menu.action.started -= OpenSettingMenu;
        if (!StartGame.IsUnityNull())
            StartGame.action.started -= LaunchGame; 
    }
    public void TherapistEntry()
    {
        _selectionPanel.SetActive(false);
        _step0.SetActive(false);

        _settingPanel.SetActive(true);
        _settingStep0.SetActive(true);
        _userTherapist = true;

    }

    public void SettingMenuStep0()
    {
        _settingStep1.SetActive(true);
        _settingStep0.SetActive(false);
        _setStep1AS.Play();
    }
    public void SettingMenuStep1()
    {
        _settingStep1.SetActive(false);
        _settingStep2.SetActive(true);
    }
    public void SettingMenuStep2()
    {
        _settingStep2.SetActive(false);
        _settingStep3.SetActive(true);
    }
    public void SettingMenuStep3()
    {
        _settingStep3.SetActive(false);
        _settingStep4.SetActive(true);
    }
    public void ParticipantEntry()
    {
        _step0.SetActive(false);

        _step1.SetActive(true);
        _userTherapist = false;
    }
    private void LaunchGame(InputAction.CallbackContext callbackContext)
    {

    }
    private void OpenSettingMenu(InputAction.CallbackContext callbackContext)
    {
        if (_rMenuToClickPanel.activeSelf)
        {
            _selectionPanel.SetActive(true);
            _step0.SetActive(true);
            _rMenuToClickPanel.SetActive(false);
        }
        else if (_userTherapist && !_settingPanel.activeSelf)
        {
            _settingPanel.SetActive(true);
        }
            
        else if (_userTherapist && _settingPanel.activeSelf)
            _settingPanel.SetActive(false);
        else if (!_userTherapist && _inGameInstrPanel.activeSelf)
            _inGameInstrPanel.SetActive(true);
        else if (!_userTherapist && !_inGameInstrPanel.activeSelf)
            _inGameInstrPanel.SetActive(false);

    }

    public void ParticipantGroup(bool cg)
    {
        _selectionPanel.SetActive(false);
        string _participantID = ParticipantIDAssignment(cg);
        ParticipantSettings.Instance.PID.Invoke(_participantID);
        _step1.SetActive(false);
        _participantTryPanel.SetActive(true);
    }

    //PID remove from setup logic and fetch from here
    private string ParticipantIDAssignment(bool cg)
    {
        //Load all the lists and idx
        string PID = "";
        if (cg)
        {
            ++_CGidx;
            PID = $"CG{_CGidx.ToString("D3")}";
            AllParticiipantDataManager.Instance.IncCGidxAsync();
        }
        else
        {
            ++_ADidx;
            PID = $"AD{_ADidx.ToString("D3")}";
            AllParticiipantDataManager.Instance.IncADidxAsync();
        }
        return PID;
    }

    public void GoBackAfterSetting()
    {
       _userTherapist = false;
        _participantTryPanel.SetActive(false);
        _inGameInstrPanel.SetActive(false);
        _settingPanel.SetActive(false);
        _step1.SetActive(false);
        _settingStep0.SetActive(false);
        _settingStep4.SetActive(false);

        _selectionPanel.SetActive(true);
        _step0.SetActive(true);
        FindAnyObjectByType<InstructionPanel>().gameObject.SetActive(true);
    }
    public void ClickenOnMe()
    {
        Debug.Log("Clicked On me the button");
    }
    private void Update()
    {
        if(startTeilnahme)
        {
            startTeilnahme = false;
            ParticipantGroup(true);
        }
    }
}
