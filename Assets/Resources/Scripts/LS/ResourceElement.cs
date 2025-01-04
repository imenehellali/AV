using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using static ResourceElement;

public class ResourceElement : MonoBehaviour
{
    public enum Type
    {
        Antidote,
        Air,
        Water,
    };
    public enum BelongTo
    {
        Case,
        Individual,
    };

    [SerializeField]
    private int amount = 0;
    private float TO = 30f;
    private bool assignable = true;


    public Type type;
    public BelongTo belongs;

    public UnityAction<bool> Assignable;
    public UnityAction<int, Type> AmountChanged;


    // Update is called once per frame

    private void Start()
    {
        Assignable.Invoke(assignable);
    }

    public ResourceElement(Type type, BelongTo belongs, int amount)
    {
        this.type = type;
        this.belongs = belongs;
        this.amount = amount;
    }
    //The button will call consume resource
    //Consume resource will notify the ui to update amount remaining
    //consume resource will also notify case to update amount per case
    //consume resource will also notify player to update its inventory NO
    public void ConsumeResource()
    {
        Debug.Log("entered Consume resource Resource lement");
        if (type.Equals(Type.Antidote) && LifeSaverManager.Instance.AllowAntidoteConsumption())
        {
            Debug.Log("entered Consume resource Antidote");
            if (amount <= 0)
            {
                assignable = false;
                Assignable.Invoke(assignable);
            }
            else
            {
                if (belongs.Equals(BelongTo.Case))
                    StartCoroutine(StartTO());
                else
                {
                    --amount;
                    AmountChanged.Invoke(amount, type);
                }
            }
        }
        if (type.Equals(Type.Air) && LifeSaverManager.Instance.AllowAirConsumption())
        {
            Debug.Log("entered Consume resource Air");
            if (amount <= 0)
            {
                assignable = false;
                Assignable?.Invoke(assignable);
            }
            else
            {
                if (belongs.Equals(BelongTo.Case))
                    StartCoroutine(StartTO());
                else
                {
                    --amount;
                    AmountChanged?.Invoke(amount, type);
                }
            }
        }
        if (type.Equals(Type.Water) && LifeSaverManager.Instance.AllowWaterConsumption())
        {
            Debug.Log("entered Consume resource Water");
            if (amount <= 0)
            {
                assignable = false;
                Assignable.Invoke(assignable);
            }
            else
            {
                if (belongs.Equals(BelongTo.Case))
                    StartCoroutine(StartTO());
                else
                {
                    --amount;
                    AmountChanged.Invoke(amount, type);
                }
            }
        }

    }
    private IEnumerator StartTO()
    {
        Debug.Log("starting time out");

        if (assignable)
        {
            assignable = false;
            Assignable.Invoke(assignable);
            --amount;
            AmountChanged?.Invoke(amount, type);
        }
        yield return new WaitForSeconds(TO);
        assignable = true;
        Assignable.Invoke(assignable);
    }
    private IEnumerator RegenerateResource()
    {
        yield return new WaitForSeconds(TO);
        amount += 2;
        AmountChanged.Invoke(amount, type);
    }
    private void Update()
    {
        if (belongs.Equals(BelongTo.Individual) &&
            (type.Equals(Type.Water) || type.Equals(Type.Water)))
        {
            Debug.Log("Will update a resource");
            StartCoroutine(RegenerateResource());
        }
    }
}
