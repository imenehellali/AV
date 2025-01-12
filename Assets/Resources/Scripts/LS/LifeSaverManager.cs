using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class LifeSaverManager : MonoBehaviour
{

    public static LifeSaverManager Instance { get; private set; }

    private float levelDuration = 0f;
    private float levelTimer = 0f;
    private bool startUrgency = false;
    private float timeToStartUrgeny =0f;


    private int _rewardAmount = 200;
    private float _time = 0f;


    [Header("Cases List")]
    [SerializeField]
    private List<GameObject> _casesObjs = new List<GameObject>();

    private Dictionary<string, Case> _cases = new Dictionary<string, Case>();
    public Dictionary<string, Case> GetCases() => _cases;
    public List<ResourceElementParticipant> _Resources = new List<ResourceElementParticipant>();

    [Header("Resource List")]
    [SerializeField]
    private TextMeshProUGUI _antidote;
    [SerializeField]
    private TextMeshProUGUI _air;
    [SerializeField]
    private TextMeshProUGUI _water;


    [Header("Environmnet Variables")]
    [SerializeField]
    private List<MeshRenderer> _envMaterials;
    [SerializeField]
    private Material _refMaterial;
    [SerializeField]
    private AudioSource _audioSource;
    [SerializeField]
    private AudioClip _urgencyAudioClip;


    public bool AllowAntidoteConsumption() => _Resources[2].amount >= 0 ? true : false;
    public bool AllowAirConsumption() => _Resources[0].amount >= 0 ? true : false;
    public bool AllowWaterConsumption() => _Resources[1].amount >= 0 ? true : false;

    public UnityAction<int, ResourceElementParticipant.Type> updateParticipantResource;
    public UnityAction<ResourceElementParticipant.Type> updateResourceUI;
    private void OnEnable()
    {
        updateParticipantResource += UpdateResources;
        updateResourceUI += UpdateResourceUI;
    }
    private void OnDisable()
    {
        updateParticipantResource -= UpdateResources;
        updateResourceUI -= UpdateResourceUI;
    }
    private void UpdateResourceUI(ResourceElementParticipant.Type type)
    {
        if (SceneManager.GetSceneByName("LSScene").isLoaded)
        {
            switch (type)
            {
                case ResourceElementParticipant.Type.Water:
                    {
                        _water.gameObject.GetComponentInParent<RectTransform>().gameObject.SetActive(false);
                        break;
                    }

                case ResourceElementParticipant.Type.Air:
                    {
                        _air.gameObject.GetComponentInParent<RectTransform>().gameObject.SetActive(false);
                        break;
                    }
                case ResourceElementParticipant.Type.Antidote:
                    {
                        _antidote.gameObject.GetComponentInParent<RectTransform>().gameObject.SetActive(false);
                        break;
                    }
            }
        }
    }
    private void UpdateResources(int amount, ResourceElementParticipant.Type type)
    {
        if (SceneManager.GetSceneByName("LSScene").isLoaded)
        {
            Debug.Log($"updating player resources {amount}  -  {type}");
            switch (type)
            {
                case ResourceElementParticipant.Type.Air:
                    {
                        _air.gameObject.GetComponentInParent<RectTransform>().gameObject.SetActive(true);
                        _air.text = amount.ToString();
                        break;
                    }
                case ResourceElementParticipant.Type.Water:
                    {
                        _water.gameObject.GetComponentInParent<RectTransform>().gameObject.SetActive(true);
                        _water.text = amount.ToString();
                        break;
                    }
                case ResourceElementParticipant.Type.Antidote:
                    {
                        _antidote.text = amount.ToString();
                        break;
                    }
            }
        }

    }
    public void ConsumeResource(ResourceElementCase.Type _type)
    {
        if (SceneManager.GetSceneByName("LSScene").isLoaded)
        {
            Debug.Log($"Invoking Consume resource LS for {_type}");
            if (_type.Equals(ResourceElementCase.Type.Air))
                _Resources[0].ConsumeResource();
            else if (_type.Equals(ResourceElementCase.Type.Water))
                _Resources[1].ConsumeResource();
            else if (_type.Equals(ResourceElementCase.Type.Antidote))
                _Resources[2].ConsumeResource();
        }
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
        levelDuration = GameSettings.Instance.LevelDurations[GameSettings.Instance.CurrLvlIdx - 1];
        Debug.Log($"LS duration {levelDuration}");
        timeToStartUrgeny = levelDuration - 40f;
        Debug.Log($"time to start urgenc  {timeToStartUrgeny}");
    }
    private void Start()
    {
        if (!enabled)
            enabled = true;
        InitCases();
        InitResources();
        for (int i = 0; i < _envMaterials.Count; i++)
        {
            _envMaterials[i].gameObject.SetActive(false);
        }
        Debug.Log($"is LS enabled? {enabled}");
    }
    private void InitCases()
    {
        _cases.Add("CH1", _casesObjs[0].GetComponent<Case>().InitCase(false, true, 3, 1, 0, 210f)); //check time 1:30sec //CH1
        _cases.Add("CH2", _casesObjs[1].GetComponent<Case>().InitCase(false, true, 2, 0, 2, 240f)); //CH2 2min
        _cases.Add("CH3", _casesObjs[2].GetComponent<Case>().InitCase(false, true, 2, 0, 1, 250f)); //CH3 3min
        _cases.Add("CH4", _casesObjs[3].GetComponent<Case>().InitCase(false, false, 0, 3, 2, 300f)); //CH4 3:30

        _cases.Add("CA1", _casesObjs[4].GetComponent<Case>().InitCase(true, false, 4, 2, 0, 210f)); //check time 1:30sec //CA1
        _cases.Add("CA2", _casesObjs[5].GetComponent<Case>().InitCase(true, false, 1, 0, 2, 240f)); //CA2 2min
        _cases.Add("CA3", _casesObjs[6].GetComponent<Case>().InitCase(true, false, 1, 1, 1, 270f)); //CA3 3min
        _cases.Add("CA4", _casesObjs[7].GetComponent<Case>().InitCase(true, false, 0, 1, 4, 300f)); //CA4 3:30

        Debug.Log("finished initiating cases");
    }
    private void InitResources()
    {
        _Resources[0].UpdateResource(ResourceElementParticipant.Type.Air, 2);
        _Resources[1].UpdateResource(ResourceElementParticipant.Type.Water, 2);
        _Resources[2].UpdateResource(ResourceElementParticipant.Type.Antidote, 5);


        Debug.Log("finished initiating resources");
    }
    public void StartLevel()
    {
        MoneyManager.instance.ResetMoney();
        StartCoroutine(Flicker());
        StartCoroutine(StartLevelTimer());
        foreach (Case _case in _cases.Values)
        {
            _case.Startcase();
        }
        Debug.Log("Will start the coroutine of regenerating resource");
        StartCoroutine(RegenerateResource());
    }

    private IEnumerator RegenerateResource()
    {
        float _dur = 30f;

        while (levelTimer < timeToStartUrgeny)
        {
            yield return new WaitForSeconds(_dur);
            _Resources[0].RegenerateAmount();
            _Resources[1].RegenerateAmount();

            Debug.Log($"Regenrated resources DONE {_Resources[1].amount}   -   {_Resources[0].amount}");
        }
    }
    private IEnumerator StartLevelTimer()
    {
        while (levelTimer < levelDuration-3f)
        {
            levelTimer += Time.deltaTime;
            _time = levelDuration - levelTimer;
            TaskProgress.Instance.updateTimer(_time);
            if (levelTimer >= timeToStartUrgeny)
                startUrgency = true;
            yield return null;
        }
        if (levelTimer >= levelDuration-5f)
        {
            GameSettings.Instance.XRBoundOFLoading.SetActive(true);
            GameSettings.Instance._dynamicMove.enabled = false;
            EndLevel();
            StopCoroutine(StartLevelTimer());
        }
    }
    //Functions to call from Game to behvae
    //
    //
    //
    //This will be called for every assignment for every resource indeoendantly 

    public void StopHelping(string CName)
    {
        Debug.Log("Stopped helping");

        if (levelDuration >= 30f)
            _cases[CName].StopHelping();
        else
            Debug.Log($"sorry too late to stop {CName} now");
    }


    //
    //
    //
    //
    private void EndLevel()
    {
        

        float amount = 0f;
        bool stopped = true;
        foreach (var item in _cases.Values)
        {
            stopped &= item.StoppedHelping();
            amount += _rewardAmount * item.percentageDone;
        }
        
        for (int i = 0; i < _envMaterials.Count; i++)
        {
            _envMaterials[i].gameObject.SetActive(false);
        }
       
        //if psycho remove 80% of its game account money 
        if (stopped && amount == 0f)
        {
            amount = MoneyManager.instance.GetGameAccount() * 0.8f * (-1f);
            MoneyManager.instance.UpdateGameAccount(amount);
        }
        else if (stopped && amount > 0f)
        {
            MoneyManager.instance.UpdateMoney(amount);
        }
        MoneyManager.instance.StoreMoneyInSafeAccount(GameSettings.Instance.CurrLvlIdx - 1);
        LSData.Data.SaveData(_cases);
        GameSettings.Instance.LoadNextScene();
    }

    private IEnumerator Flicker()
    {
        float _ti = _urgencyAudioClip.length;
        float _timerOfUrgency =40f;
        while (!startUrgency)
        {
            yield return null;
        }
        if (startUrgency)
        {
            startUrgency = false;
            _envMaterials.ForEach(_env =>
            {
                _env.gameObject.SetActive(true);
                _env.material = Instantiate(_refMaterial);
            });

            LSExitDoor _obj = FindObjectOfType<LSExitDoor>();
            if (_obj != null)
                _obj.PlayEnd();

            while (_timerOfUrgency >= 2f)
            {
                Debug.Log($"time To Start Urgency;  {_timerOfUrgency}");
                _timerOfUrgency -= _ti;
                _audioSource.PlayOneShot(_urgencyAudioClip);
                _envMaterials[0].gameObject.SetActive(true);
                _envMaterials[1].gameObject.SetActive(true);
                _envMaterials[2].gameObject.SetActive(true);
                _envMaterials[3].gameObject.SetActive(true);
                yield return new WaitForSeconds(_ti);
                _envMaterials[0].gameObject.SetActive(false);
                _envMaterials[1].gameObject.SetActive(false);
                _envMaterials[2].gameObject.SetActive(false);
                _envMaterials[3].gameObject.SetActive(false);

            }
            if (_timerOfUrgency < 2)
            {
                foreach (KeyValuePair<string,Case> _case in _cases)
                {
                    _case.Value.StopCase(_case.Key);

                }
                foreach (ResourceElementParticipant _resource in _Resources)
                {
                    _resource.StopResource();

                }
                StopCoroutine(RegenerateResource());
                StopCoroutine(Flicker());
            }

        }
    }

}
