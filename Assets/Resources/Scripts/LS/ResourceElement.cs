using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
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
        Assignable?.Invoke(assignable);
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
        if(type.Equals(Type.Antidote) && LifeSaverManager.Instance.AllowAntidoteConsumption())
        {
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
        else if (type.Equals(Type.Air) && LifeSaverManager.Instance.AllowAirConsumption())
        {
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
        else if (type.Equals(Type.Water) && LifeSaverManager.Instance.AllowWaterConsumption())
        {
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

    }
    private IEnumerator StartTO()
    {
        if (assignable)
        {
            assignable = false;
            Assignable?.Invoke(assignable);
            --amount;
            AmountChanged?.Invoke(amount, type);
        }
        yield return new WaitForSeconds(TO);
        assignable = true;
        Assignable?.Invoke(assignable);
    }
    private IEnumerator RegenerateResource()
    {
        yield return new WaitForSeconds(TO);
        amount += 2;
        AmountChanged?.Invoke(amount, type);
    }
    private void Update()
    {
        if (belongs.Equals(BelongTo.Individual) &&
            (type.Equals(Type.Water) || type.Equals(Type.Water)))
        {

            StartCoroutine(RegenerateResource());
        }
    }
}
