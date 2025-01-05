using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class GhostBusterManager : MonoBehaviour
{


    //Quest Variables
    private float _Q1Time = 0f;
    private float _Q2Time = 0f;
    private float _Q3Time = 0f;
    private float _Q4Time = 0f;
    private float levelDuration = 300f;
    private float _time = 0f;

    //Room Effects variables
    [Header("Room Effect Materials")]
    [SerializeField]
    private Color _greenEffectRoomMaterial;
    [SerializeField]
    private Color _redEffectRoomMaterial;
    [SerializeField]
    private Color _blackEffectRoomMaterial;
    [SerializeField]
    private Material _roomEffectMat;


    //Room Sound Effect Variables
    [Header("Room Effect sounds")]
    [SerializeField]
    private AudioSource audioSource;
    [SerializeField]
    private AudioClip _correctActionClip;
    [SerializeField]
    private AudioClip _wrongActionClip;
    [SerializeField]
    private AudioClip _Q1InstructionClip;
    [SerializeField]
    private AudioClip _Q2InstructionClip;
    [SerializeField]
    private AudioClip _Q3InstructionClip;
    [SerializeField]
    private AudioClip _Q4InstructionClip;

    //Actions Variables
    [Header("Action Variables")]
    private const int _correctActionCost = 20;
    private const int _wrongActionCost = -20;


    //Ghost Variable
    private Vector3 _ghostSpawnWPos = Vector3.zero;
    public UnityAction<GhostBustBehavior> killedGhost;

    [Header("Ghosts Items")]
    [SerializeField]
    private List<GameObject> ghostsPref = new List<GameObject>();// _drunkenRedGhost;
    private float _ghostSpawnTO = 5f;


    public static  GhostBusterManager Instance { get; private set; }
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        AssignQTime();
    }
    private void Start()
    {
        levelDuration = GameSettings.Instance.LevelDurations[GameSettings.Instance.CurrLvlIdx-1];
        Debug.Log($"GB Duration:   {levelDuration}");
    }

    private void OnEnable()
    {
        killedGhost += Q1Ghostbusting;
        killedGhost -= Q2Ghostbusting;
        killedGhost += Q3Ghostbusting;
        killedGhost -= Q4Ghostbusting;  
    }
    private void OnDisable()
    {
        killedGhost += Q1Ghostbusting;
        killedGhost -= Q2Ghostbusting;
        killedGhost += Q3Ghostbusting;
        killedGhost -= Q4Ghostbusting;
    }
   
    public void StartTask()
    {
        MoneyManager.instance.ResetMoney();
        StartCoroutine(Q1());
    }
    private IEnumerator Q1()
    {
        audioSource.PlayOneShot(_Q1InstructionClip);
        StartCoroutine(SpawnRoomEffect(true));
        yield return new WaitForSeconds(_Q1InstructionClip.length);
        StartCoroutine(StartQ1());
    }
    private IEnumerator Q2()
    {
        audioSource.PlayOneShot(_Q2InstructionClip);
        StartCoroutine(SpawnRoomEffect(false));
        yield return new WaitForSeconds(_Q2InstructionClip.length);
        StartCoroutine(StartQ2());
    }
    private IEnumerator Q3()
    {
        audioSource.PlayOneShot(_Q3InstructionClip);
        yield return new WaitForSeconds(_Q3InstructionClip.length);
        StartCoroutine(StartQ3());
    }
    private IEnumerator Q4()
    {
        audioSource.PlayOneShot(_Q4InstructionClip);
        yield return new WaitForSeconds(_Q4InstructionClip.length);
        StartCoroutine(StartQ4());
    }


    private void Q1Ghostbusting(GhostBustBehavior ghostBehavior)
    {
        if(ghostBehavior.ghostRed && ghostBehavior.ghostDead)
        {
            ghostBehavior.gameObject.GetComponent<GhostBustBehavior>()._in = "   Ouch Killed me";
            audioSource.Stop();
            audioSource.PlayOneShot(_correctActionClip);
            MoneyManager.instance.UpdateMoney(_correctActionCost);
            Destroy(ghostBehavior.gameObject);
        }
        else 
        {
            ghostBehavior.gameObject.GetComponent<GhostBustBehavior>()._in = "   killed me wrong";
            audioSource.Stop();
            audioSource.PlayOneShot(_wrongActionClip);
            MoneyManager.instance.UpdateMoney(_wrongActionCost);
            Destroy(ghostBehavior.gameObject);
        }
    }
    //Shoot the Red Ghost + room red
    private IEnumerator StartQ1()
    {
        StartCoroutine(SpawnGhost(_Q1Time, _ghostSpawnTO, 10f));
        while (_Q1Time > 0)
        {
            _Q1Time -= Time.deltaTime;
            _time = _Q1Time+ _Q2Time + _Q3Time + _Q4Time;
            TaskProgress.Instance.updateTimer(_time); yield return null;
        }
        if (_Q1Time <= 0)
        {
            StopAllCoroutines();
            StartCoroutine(Q2());
        }

    }

    private void Q2Ghostbusting(GhostBustBehavior ghostBehavior)
    {
        if (!ghostBehavior.ghostRed && ghostBehavior.ghostDead)
        {
            ghostBehavior.gameObject.GetComponent<GhostBustBehavior>()._in = "   Ouch Killed me";
            audioSource.Stop();
            audioSource.PlayOneShot(_correctActionClip);
            MoneyManager.instance.UpdateMoney(_correctActionCost);
            Destroy(ghostBehavior.gameObject);
        }
        else 
        {
            ghostBehavior.gameObject.GetComponent<GhostBustBehavior>()._in = "   killed me wrong";
            audioSource.Stop();
            audioSource.PlayOneShot(_wrongActionClip);
            MoneyManager.instance.UpdateMoney(_wrongActionCost);
            Destroy(ghostBehavior.gameObject);
        }
    }
    //Shoot the blue ghost + room lit blue
    private IEnumerator StartQ2()
    {
        StartCoroutine(SpawnGhost(_Q2Time, _ghostSpawnTO, 20f));
        while (_Q2Time > 0)
        {
            _Q2Time -= Time.deltaTime;
            _time = _Q2Time + _Q3Time + _Q4Time;
            TaskProgress.Instance.updateTimer(_time);
            yield return null;
        }
        if (_Q2Time <= 0)
        {
            StopAllCoroutines();
            StartCoroutine(Q3());
        }
    }

    private void Q3Ghostbusting(GhostBustBehavior ghostBehavior)
    {
        if (!ghostBehavior.ghostDrunken && ghostBehavior.ghostDead)
        {
            ghostBehavior.gameObject.GetComponent<GhostBustBehavior>()._in = "   Ouch Killed me";
            audioSource.Stop();
            audioSource.PlayOneShot(_correctActionClip);
            MoneyManager.instance.UpdateMoney(_correctActionCost);
            Destroy(ghostBehavior.gameObject);
        }
        else 
        {
            ghostBehavior.gameObject.GetComponent<GhostBustBehavior>()._in = "   killed me wrong";
            audioSource.Stop();
            audioSource.PlayOneShot(_wrongActionClip);
            MoneyManager.instance.UpdateMoney(_wrongActionCost);
            Destroy(ghostBehavior.gameObject);
        }
    }
    //shoot sober ghost - flickering light with random interval - money sound randomly
    private IEnumerator StartQ3()
    {
        _ghostSpawnTO -= 2;
        StartCoroutine(RoomEffectQ3());
        StartCoroutine(SoundEffectQ3());
        StartCoroutine(SpawnGhost(_Q3Time, _ghostSpawnTO, 20f));
        while (_Q3Time > 0)
        {
            _Q3Time -= Time.deltaTime;
            _time = _Q4Time + _Q3Time;
            TaskProgress.Instance.updateTimer(_time);
            yield return null;
        }
        if (_Q3Time <= 0)
        {
            StopAllCoroutines();
            StartCoroutine(Q4());
        }
    }

    private void Q4Ghostbusting(GhostBustBehavior ghostBehavior)
    {
        if (ghostBehavior.ghostDrunken && !ghostBehavior.ghostRed && ghostBehavior.ghostDead)
        {
            ghostBehavior.gameObject.GetComponent<GhostBustBehavior>()._in = "   Ouch Killed me";
            audioSource.Stop();
            audioSource.PlayOneShot(_correctActionClip);
            MoneyManager.instance.UpdateMoney(_correctActionCost);
            Destroy(ghostBehavior.gameObject);
        }
        else
        {
            ghostBehavior.gameObject.GetComponent<GhostBustBehavior>()._in = "   killed me wrong";
            audioSource.Stop();
            audioSource.PlayOneShot(_wrongActionClip);
            MoneyManager.instance.UpdateMoney(_wrongActionCost);
            Destroy(ghostBehavior.gameObject);
        }
    }
    //shoot green drunken ghost - flickering light with random interval - money sound randomly
    private IEnumerator StartQ4()
    {
        _ghostSpawnTO -= 2;
        StartCoroutine(SoundEffectQ4());
        StartCoroutine(RoomEffectQ4());
        StartCoroutine(SpawnGhost(_Q4Time, _ghostSpawnTO, 30f));
        while (_Q4Time > 0)
        {
            _Q4Time -= Time.deltaTime;
            TaskProgress.Instance.updateTimer(_Q4Time); 
            yield return null;
        }
        //Save all data here 
        if (_Q4Time <= 0)
        {
            EndLevel();
            
        }
    }
    private void EndLevel()
    {
        StopAllCoroutines();
        GBData.Data.SaveData();
        MoneyManager.instance.StoreMoneyInSafeAccount(GameSettings.Instance.CurrLvlIdx - 1);
        GameSettings.Instance.LoadNextScene();
    }

    private void AssignQTime()
    {
        //levelDuration = GameSettings.Instance.LevelDurations[GameSettings.Instance.CurrentLevelIndex - 1];
        _Q1Time = Mathf.FloorToInt(levelDuration / 4);
        _Q2Time = Mathf.FloorToInt(levelDuration / 4);
        _Q3Time = Mathf.FloorToInt(levelDuration / 4);
        _Q4Time = levelDuration - _Q1Time - _Q2Time - _Q3Time;
    }

    private IEnumerator SpawnGhost(float QTime, float spawnTO, float rotationSpeedQ)
    {
        float currTime = QTime;

        while (currTime > 0)
        {
            yield return new WaitForSeconds(spawnTO);
            currTime -= spawnTO;
            //Spawn random position around player in upper hemisphere
            _ghostSpawnWPos = Random.insideUnitSphere * 3f;
            _ghostSpawnWPos.y = Mathf.Clamp(_ghostSpawnWPos.y, 0f, 3f);
            int _rand = Random.Range(0, 6);
            GameObject _ghost = Instantiate(ghostsPref[_rand], _ghostSpawnWPos, Quaternion.identity);
            --currTime;
        }
    }
    
    private IEnumerator SpawnRoomEffect(bool red)
    {
        _roomEffectMat.color= red ? _redEffectRoomMaterial : _greenEffectRoomMaterial;
        yield return new WaitForSeconds(4);
        _roomEffectMat.color= _blackEffectRoomMaterial;
    }
    private IEnumerator SpawnRoomEffectQ4(Color _color)
    {
        _roomEffectMat.color= _color;
        yield return new WaitForSeconds(4);
        _roomEffectMat.color=_blackEffectRoomMaterial;
    }
    private IEnumerator RoomEffectQ3()
    {
        float _q3Time = _Q3Time;
        while (_q3Time > 0)
        {

            int _randTO = Random.Range(0, 5);
            int _randEffect = Random.Range(0, 2);
            StartCoroutine(SpawnRoomEffect(_randEffect == 0 ? true : false));
            _q3Time -= _randTO;
            yield return new WaitForSeconds(_randTO);
        }

    }
    private IEnumerator RoomEffectQ4()
    {
        float _q4Time = _Q4Time;
        while (_q4Time > 0)
        {
            int _randTO = Random.Range(0, 10);
            int _randEffect = Random.Range(0, 3);
            switch (_randEffect)
            {
                case 0:
                    StartCoroutine(SpawnRoomEffectQ4(_redEffectRoomMaterial));
                    break;
                case 1:
                    StartCoroutine(SpawnRoomEffectQ4(_greenEffectRoomMaterial));
                    break;
                case 2:
                    StartCoroutine(SpawnRoomEffectQ4(_blackEffectRoomMaterial));
                    break;
            }
            _q4Time -= _randTO;
            yield return new WaitForSeconds(_randTO);
        }

    }
    private IEnumerator SoundEffectQ3()
    {
        float _q3Time = _Q3Time;
        while (_q3Time > 0)
        {
            int _randTO = Random.Range(0, 5);
            int _randEffect = Random.Range(0, 2);
            audioSource.PlayOneShot(_randEffect == 0 ? _correctActionClip : _wrongActionClip);
            _q3Time -= _randTO;
            yield return new WaitForSecondsRealtime(_randTO);
        }

    }
    private IEnumerator SoundEffectQ4()
    {
        float _q4Time = _Q4Time;
        while (_q4Time > 0)
        {
            int _randTO = Random.Range(0, 3);
            int _randEffect = Random.Range(0, 2);
            audioSource.PlayOneShot(_randEffect == 0 ? _correctActionClip : _wrongActionClip);
            _q4Time -= _randTO;
            yield return new WaitForSecondsRealtime(_randTO);
        }

    }
}
