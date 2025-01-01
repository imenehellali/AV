
using UnityEngine;
using TMPro;
using UnityEngine.Events;
public class TaskProgress : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI timer;
    [SerializeField] private TextMeshProUGUI moneyDisplay;
    [SerializeField] private GameObject instructionPanel;

    public static TaskProgress Instance;
    public UnityAction<float> updateTimer;
    public UnityAction updateMoney;
    private void Awake()
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
    }
    private void OnEnable()
    {
         MoneyManager.instance.OnMoneyWon+=UpdateMoneyDisplay;
         GameSettings.Instance.OnTimeUp+=UpdateTimer;
        updateTimer += UpdateTimer;
        updateMoney += UpdateMoneyDisplay;
       

    }
    private void OnDisable()
    {
        MoneyManager.instance.OnMoneyWon -= UpdateMoneyDisplay;
        GameSettings.Instance.OnTimeUp -= UpdateTimer;
        updateTimer -= UpdateTimer;
        updateMoney -= UpdateMoneyDisplay;
    }
    
    
    private void UpdateMoneyDisplay()=>moneyDisplay.text = "\u20AC" +((int)MoneyManager.instance.GetMoney()).ToString("N2");
    

    private void UpdateTimer(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60F);
        int seconds = Mathf.FloorToInt(time % 60F);
        timer.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    public void ToggleInstructionPanel()
    {
        instructionPanel.SetActive(true);
    }
  
}
