using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ResourceElement;

public class ResourceElementCase : ResourceElement
{
    [SerializeField]
    private ResourceElementUOHandler _resourceUI;
    [SerializeField]
    private Case _case;

    public ResourceElementCase(Type type, int amount) : base(type, amount)
    {
        _resourceUI.AmountUpdate.Invoke(amount);
    }
    public void UpdateResource(int amount)
    {
        assignable = true;
        this.amount = amount;
        _resourceUI.AmountUpdate.Invoke(amount);
    }

    public new void ConsumeResource()
    {
        Debug.Log("entered Consume resource case");
        if (type.Equals(Type.Antidote) && LifeSaverManager.Instance.AllowAntidoteConsumption())
        {
            if (assignable)
            {
                assignable = false;

                if (amount > 0)
                {
                    --amount;
                    _resourceUI.TimeOut.Invoke(false);
                    _resourceUI.AmountUpdate.Invoke(amount);
                    if (gameObject.activeSelf) StartCoroutine(StartTO());
                    _case.UpdateCase.Invoke(type);
                }
                else
                {
                    _resourceUI.TimeOut.Invoke(false);

                }

            }

        }
        else if (type.Equals(Type.Air) && LifeSaverManager.Instance.AllowAirConsumption())
        {
            Debug.Log($"got inside the Air with assignable  {assignable}");
            if (assignable)
            {
                Debug.Log($"got inside assignable with current amount    {amount}");

                assignable = false;

                if (amount > 0)
                {
                    Debug.Log("Will start invoking UI for air Case");
                    --amount;
                    _resourceUI.TimeOut.Invoke(false);
                    _resourceUI.AmountUpdate.Invoke(amount);
                    if(gameObject.activeSelf) StartCoroutine(StartTO());
                    _case.UpdateCase.Invoke(type);
                }
                else
                {
                    _resourceUI.TimeOut.Invoke(false);
                }
            }

        }
        else if (type.Equals(Type.Water) && LifeSaverManager.Instance.AllowWaterConsumption())
        {
            if (assignable)
            {
                assignable = false;

                if (amount > 0)
                {
                    --amount;
                    _resourceUI.TimeOut.Invoke(false);
                    _resourceUI.AmountUpdate.Invoke(amount);
                    if (gameObject.activeSelf) StartCoroutine(StartTO());
                    _case.UpdateCase.Invoke(type);
                }
                else
                {
                    _resourceUI.TimeOut.Invoke(false);
                }
            }

        }

        LifeSaverManager.Instance.ConsumeResource(type);
    }
    //Invoking UI
    private IEnumerator StartTO()
    {
        Debug.Log("starting time out");
        yield return new WaitForSeconds(TO);
        assignable = true;
        _resourceUI.TimeOut.Invoke(true);
    }
}
