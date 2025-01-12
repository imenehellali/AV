using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using Unity.XR.PXR;
using Unity.XR.PXR.Input;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class SettingMenuControls : MonoBehaviour
{
    public bool startTeilnahme = false;
    [SerializeField]
    private TextMeshProUGUI _testVariable;
    [Header("Controls")]
    [SerializeField]
    private InputActionReference Menu;
    [SerializeField]
    private InputActionReference StartGame;


    [Header("Wall Panels")]
    public GameObject _rMenuToClickPanel;
    public GameObject _participantTryPanel;

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

    public void StartSettingAgain()
    {
        if (_selectionPanel.activeSelf) _selectionPanel.SetActive(false);
        if (_step0.activeSelf) _step0.SetActive(false);

        if (_step1.activeSelf) _step1.SetActive(false);
        if (_inGameInstrPanel.activeSelf) _inGameInstrPanel.SetActive(false);

        if (_settingPanel.activeSelf) _settingPanel.SetActive(false);
        if (_settingStep0.activeSelf) _settingStep0.SetActive(false);

        _CGidx = AllParticiipantDataManager.Instance.getCGidx();
        _ADidx = AllParticiipantDataManager.Instance.getADidx();

        if (_participantTryPanel != null && _participantTryPanel.activeSelf) _participantTryPanel.SetActive(false);
        if (_rMenuToClickPanel != null && !_rMenuToClickPanel.activeSelf) _rMenuToClickPanel.SetActive(true);

    }
    private void Start()
    {

        _selectionPanel.SetActive(false);
        _step0.SetActive(false);

        _step1.SetActive(false);

        _inGameInstrPanel.SetActive(false);

        _settingPanel.SetActive(false);
        _settingStep0.SetActive(false);

        _CGidx = AllParticiipantDataManager.Instance.getCGidx();
        _ADidx = AllParticiipantDataManager.Instance.getADidx();

        _participantTryPanel.SetActive(false);
        

    }

    private void OnEnable()
    {

        Menu.action.started += OpenSettingMenu;
        StartGame.action.started += LaunchGame;

    }
    private void OnDisable()
    {
        Menu.action.started -= OpenSettingMenu;
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
        Debug.Log("entered launch game menu");

        _testVariable.text = "triggered ME from setting menu controls Launching";
        if (callbackContext.ReadValueAsButton())
        {
            GameSettings.Instance.LoadNextScene();
            this.enabled = false;
        }

    }
    private void OpenSettingMenu(InputAction.CallbackContext callbackContext)
    {
        Debug.Log("entered open setting menu");
        if (callbackContext.ReadValueAsButton() && !_inGameInstrPanel.activeSelf)
        {
            _testVariable.text = "triggered ME from setting menu controls";

            if (_rMenuToClickPanel.activeSelf && !_step0.activeSelf)
            {
                _selectionPanel.SetActive(true);
                _step0.SetActive(true);
                _rMenuToClickPanel.SetActive(false);
            }
            else if (!_rMenuToClickPanel.activeSelf && !_step0.activeSelf)
            {
                _step1.SetActive(false);
                _inGameInstrPanel.SetActive(false);
                _settingPanel.SetActive(false);
                _settingStep0.SetActive(false);
                _participantTryPanel.SetActive(false);

                _selectionPanel.SetActive(true);
                _step0.SetActive(true);
            }
            else if (_userTherapist && !_settingPanel.activeSelf)
            {
                _selectionPanel.SetActive(false);
                _step0.SetActive(false);
                _step1.SetActive(false);
                _inGameInstrPanel.SetActive(false);
                _settingStep0.SetActive(false);
                _participantTryPanel.SetActive(false);

                _settingPanel.SetActive(true);
            }
                
            else if (_userTherapist && _settingPanel.activeSelf)
            {
                _selectionPanel.SetActive(false);
                _step0.SetActive(false);
                _step1.SetActive(false);
                _inGameInstrPanel.SetActive(false);
                _settingStep0.SetActive(false);
                _participantTryPanel.SetActive(false);

                _settingPanel.SetActive(false);
            }
        }


    }

    public void ParticipantGroup(bool cg)
    {
        _selectionPanel.SetActive(false);
        string _participantID = ParticipantIDAssignment(cg);
        ParticipantSettings.Instance.PID.Invoke(_participantID);
        _step1?.SetActive(false);
        _participantTryPanel.SetActive(true);
        _inGameInstrPanel.SetActive(true);
        _inGameInstrPanel.GetComponent<InstructionPanel>().enabled = true;
        _inGameInstrPanel.GetComponent<InstructionPanel>().sceneLoaded.Invoke("StartScene");

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
    }
    public void ClickenOnMe()
    {
        Debug.Log("Clicked On me the button");
    }
    private void Update()
    {
        if (startTeilnahme)
        {
            startTeilnahme = false;
            ParticipantGroup(true);
        }

    }
}
