using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;
using static ResourceElement;

public class LifeSaverManager : MonoBehaviour
{
    private float levelDuration = 0f;
    private float levelTimer = 0f;
    private bool startUrgency = false;
    private float timeToStartUrgeny = 0f;


    private int _rewardAmount = 200;
    private float _time = 0f;


    [Header("Cases List")]
    [SerializeField]
    private List<GameObject> _casesObjs = new List<GameObject>();

    private Dictionary<string, Case> _cases = new Dictionary<string, Case>();
    public Dictionary<string, Case> GetCases() => _cases;
    private List<ResourceElementParticipant> _Resources = new List<ResourceElementParticipant>();

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

    public UnityAction<int, ResourceElement.Type> updateResource;
    public UnityAction<ResourceElement.Type> updateResourceUI;
    private void OnEnable()
    {
        updateResource += UpdateResources;
        updateResourceUI += UpdateResourceUI;
    }
    private void OnDisable()
    {
        updateResource -= UpdateResources;
        updateResourceUI -= UpdateResourceUI;
    }
    private void UpdateResourceUI(ResourceElement.Type type)
    {
        switch (type)
        {
            case ResourceElement.Type.Water:
                {
                    _water.gameObject.GetComponentInParent<RectTransform>().gameObject.SetActive(false);
                    break;
                }

            case ResourceElement.Type.Air:
                {
                    _air.gameObject.GetComponentInParent<RectTransform>().gameObject.SetActive(false);
                    break;
                }
            case ResourceElement.Type.Antidote:
                {
                    _antidote.gameObject.GetComponentInParent<RectTransform>().gameObject.SetActive(false);
                    break;
                }
        }
    }
    private void UpdateResources(int amount, ResourceElement.Type type)
    {
        Debug.Log($"updating player resources {amount}  -  {type}");
        switch (type)
        {
            case ResourceElement.Type.Air:
                {
                    _air.gameObject.GetComponentInParent<RectTransform>().gameObject.SetActive(true);
                    _air.text = amount.ToString();
                    break;
                }
            case ResourceElement.Type.Water:
                {
                    _water.gameObject.GetComponentInParent<RectTransform>().gameObject.SetActive(true);
                    _water.text = amount.ToString();
                    break;
                }
            case ResourceElement.Type.Antidote:
                {
                    _antidote.text = amount.ToString();
                    break;
                }
        }
    }
    public void ConsumeResource(ResourceElement.Type _type)
    {
        Debug.Log($"Invoking Consume resource LS for {_type}");
        _Resources.Find(x => x.type.Equals(_type)).ConsumeResource();
    }

    public static LifeSaverManager Instance { get; private set; }

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
        InitCases();
        InitResources();
        for (int i = 0; i < _envMaterials.Count; i++)
        {
            _envMaterials[i].gameObject.SetActive(false);
        }

    }
    private void Start()
    {
       
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
        _Resources.Add(new ResourceElementParticipant(ResourceElement.Type.Air, 2));
        _Resources.Add(new ResourceElementParticipant(ResourceElement.Type.Water, 2));
        _Resources.Add(new ResourceElementParticipant(ResourceElement.Type.Antidote, 5));

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
        while (true)
        {
            yield return new WaitForSeconds(_dur);
          _Resources[0].RegenerateAmount();
            _Resources[1].RegenerateAmount();

            Debug.Log($"Regenrated resources DONE {_Resources[1].amount}   -   {_Resources[0].amount}");
        }
    }
    private IEnumerator StartLevelTimer()
    {
        while (levelTimer < levelDuration)
        {
            levelTimer += Time.deltaTime;
            _time = levelDuration - levelTimer;
            TaskProgress.Instance.updateTimer(_time);
            if (levelTimer >= timeToStartUrgeny)
                startUrgency = true;
            yield return null;
        }
        if (levelTimer >= levelDuration)
        {
            EndLevel();

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
        StopAllCoroutines();

        for (int i = 0; i < _envMaterials.Count; i++)
        {
            Destroy(_envMaterials[i].gameObject);
        }
        _envMaterials.Clear();

        float amount = 0f;
        bool stopped = true;
        foreach (var item in _cases.Values)
        {
            stopped &= item.StoppedHelping();
            amount += _rewardAmount * item.percentageDone;
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
            MoneyManager.instance.StoreMoneyInSafeAccount(GameSettings.Instance.CurrLvlIdx - 1);
        }
        LSData.Data.SaveData();
        GameSettings.Instance.LoadNextScene();
    }

    private IEnumerator Flicker()
    {
        float _ti = _urgencyAudioClip.length;
        while (!startUrgency)
        {
            yield return null;
        }
        if (startUrgency)
        {
            _envMaterials.ForEach(_env =>
            {
                _env.gameObject.SetActive(true);
                _env.material = Instantiate(_refMaterial);
            });

            StartOpenDoor();
            while (timeToStartUrgeny > 0)
            {
                timeToStartUrgeny -= _ti;
                _audioSource.PlayOneShot(_urgencyAudioClip);
                _envMaterials.ForEach(_env => _env.material.EnableKeyword("_EMISSION"));
                yield return new WaitForSeconds(_ti);
                _envMaterials.ForEach(_env => _env.material.DisableKeyword("_EMISSION"));

            }
        }
    }

    private void StartOpenDoor()
    {
        FindObjectOfType<LSExitDoor>().PlayEnd();
    }
}
