using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class EndLevelManager : MonoBehaviour
{

    public static EndLevelManager instance { get; private set; }
    [Header("Panels")]
    [SerializeField]
    private InputActionReference _startGameAgain;
    [SerializeField]
    private InputActionReference _exitGame;

    [Header("Money Variables")]
    [SerializeField]
    private TextMeshProUGUI _gameAmountUGUI;
    [SerializeField]
    private TextMeshProUGUI _newBalanceUGUI;


    [Header("Safe Account Variables, PUW, LS, GB, TM")]
    [SerializeField]
    private GameObject _accountBalancePanel;
    [SerializeField]
    private List<GameObject> _safeAccountRefs = new List<GameObject>();
    [SerializeField]
    private GameObject enteredLeaderBoard;
    [SerializeField]
    private GameObject didNotEnterLeaderBoard;

    private bool canClick = false;
    private float _gameAmount;
    private float _newBalance;
    private List<Vector3> transforms = new List<Vector3>();

    public UnityAction<float> updateBalance;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    private void OnEnable()
    {
        _startGameAgain.action.performed += OnGameStart;
        _exitGame.action.performed += OnGameQuit;
        updateBalance += UpdateBalance;
    }
    private void OnDisable()
    {
        _startGameAgain.action.performed -= OnGameStart;
        _exitGame.action.performed -= OnGameQuit;
        updateBalance -= UpdateBalance;
    }
    private void Start()
    {
        _gameAmount = MoneyManager.instance.GetGameAccount();

        enteredLeaderBoard.SetActive(false);
        didNotEnterLeaderBoard.SetActive(false);
        _accountBalancePanel.SetActive(true);

        _newBalance = _gameAmount;
        _newBalanceUGUI.text = "\u20AC" + ((int)_newBalance).ToString("N2");
        _gameAmountUGUI.text = "\u20AC" + ((int)_gameAmount).ToString("N2");

        transforms.Add(new Vector3(-544f, 182f, 0));
        transforms.Add(new Vector3(-544f, 10.9001083f, 0));
        transforms.Add(new Vector3(-544f, -160.099884f, 0));
        transforms.Add(new Vector3(-544f, -332.099884f, 0));

        List<float> safeAcounts = GameStats.GetSafeAccount();
        List<string> levels = GameSettings.Instance.LevelSequence.ToList<string>();
        for (int i = 0; i < safeAcounts.Count; i++)
        {
            GameObject _obj = null;

            if (levels[i].Equals("PUWScene"))
            {
                _obj = Instantiate(_safeAccountRefs[0], transforms[i],Quaternion.identity,_accountBalancePanel.transform);
            }
            else if (levels[i].Equals("LSScene"))
            {
                _obj = Instantiate(_safeAccountRefs[1], transforms[i], Quaternion.identity, _accountBalancePanel.transform);

            }
            else if (levels[i].Equals("GBScene"))
            {
                _obj = Instantiate(_safeAccountRefs[2], transforms[i], Quaternion.identity, _accountBalancePanel.transform);

            }
            else if (levels[i].Equals("TMScene"))
            {
                _obj = Instantiate(_safeAccountRefs[3], transforms[i], Quaternion.identity, _accountBalancePanel.transform);

            }
            _obj.GetComponent<SafeAccountUIHandler>().InitializeSafeAmount(safeAcounts[i]);
        }
        
    }
    private void OnGameStart(InputAction.CallbackContext context)
    {
        if (context.ReadValueAsButton() && canClick)
        {
            SceneLoaders.Instance.LoadLevel("StartScene");
            MoneyManager.instance.ResetGameAccount();
            ParticipantSettings.Instance.ResetParticipantData();
            GameSettings.Instance.ResetInGameSettings();
        }
    }
    private void OnGameQuit(InputAction.CallbackContext context)
    {
        if (context.ReadValueAsButton() && canClick)
        {

        }
    }
    private void UpdateBalance(float amount)
    {
        _newBalance += amount;
        _newBalanceUGUI.text = "\u20AC" + ((int)_newBalance).ToString("N2");
    }
    public void ValidateBalance()
    {
        _accountBalancePanel.SetActive(false);
        if (_newBalance < 211)
        {
            enteredLeaderBoard.SetActive(true);
            enteredLeaderBoard.GetComponent<AudioSource>().Play();
        }
        else
        {
            didNotEnterLeaderBoard.SetActive(true);
            didNotEnterLeaderBoard.GetComponent<AudioSource>().Play();
        }
        StartCoroutine(EnableEnd());
    }

    private IEnumerator EnableEnd()
    {
        yield return new WaitForSeconds(20f);
        if (didNotEnterLeaderBoard.activeSelf) didNotEnterLeaderBoard.SetActive(false);
        if (enteredLeaderBoard.activeSelf) enteredLeaderBoard.SetActive(false);
        canClick = true;
    }
}
