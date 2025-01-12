using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ResourceElementCase : MonoBehaviour
{
    public enum Type
    {
        Antidote,
        Air,
        Water,
    };

    public const float TO = 10f;
    public Type type;
    public int amount;

    public bool assignable = true;
    private bool canInvokeResource = true;

    [SerializeField]
    private ResourceElementUOHandler _resourceUI;
    [SerializeField]
    private Case _case;

    public ResourceElementCase(Type type, int amount)
    {
        this.type = type;
        this.amount = amount;
        _resourceUI.AmountUpdate.Invoke(amount);
    }
    public void UpdateResource(int amount)
    {
        canInvokeResource = true;
        assignable = true;
        this.amount = amount;
        _resourceUI.AmountUpdate.Invoke(amount);
    }
    public void StopResource(string caseName)
    {
        if(  ResourceElementCase.Type.Water.Equals(type)||
            ResourceElementCase.Type.Air.Equals(type) ||
            ResourceElementCase.Type.Antidote.Equals(type))
        {
            Debug.Log($"topping resource from {type} belongign to {caseName}");
            assignable = false;
            canInvokeResource = false;
           if(amount>0) 
                _resourceUI.StopResource();
        }
        
    }
    public void ConsumeResource()
    {
        if (canInvokeResource)
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
                        if (gameObject.activeSelf) StartCoroutine(StartTO());
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
    }
    //Invoking UI
    private IEnumerator StartTO()
    {
        if(canInvokeResource)
        {
            Debug.Log("starting time out");
            yield return new WaitForSeconds(TO);
            if (canInvokeResource)
            {
                assignable = true;
                _resourceUI.TimeOut.Invoke(true);
            }
        }
      
    }
}
