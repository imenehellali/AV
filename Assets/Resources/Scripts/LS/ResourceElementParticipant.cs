using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ResourceElementParticipant : MonoBehaviour
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
    private bool canInvoke = true;
    public ResourceElementParticipant(Type type, int amount)
    {
        canInvoke = true;
        this.type = type;
        this.amount = amount;
    }

    public void UpdateResource(Type type, int amount)
    {
        canInvoke = true;
        assignable = true;
        this.amount = amount;
        this.type = type;
        LifeSaverManager.Instance.updateParticipantResource.Invoke(amount, type);
    }
    public void StopResource()
    {
        canInvoke = false;
    }
    public void ConsumeResource()
    {
        if (canInvoke)
        {


            Debug.Log("entered Consume resource player");
            if (type.Equals(Type.Antidote) && LifeSaverManager.Instance.AllowAntidoteConsumption())
            {
                if (assignable)
                {
                    assignable = false;

                    if (amount > 0)
                    {
                        --amount;
                        LifeSaverManager.Instance.updateParticipantResource.Invoke(amount, type);
                        assignable = true;

                    }
                    else
                    {
                        LifeSaverManager.Instance.updateResourceUI.Invoke(type);
                    }

                }

            }
            else if (type.Equals(Type.Air) && LifeSaverManager.Instance.AllowAirConsumption())
            {
                if (assignable)
                {
                    assignable = false;

                    if (amount > 0)
                    {
                        --amount;

                        LifeSaverManager.Instance.updateParticipantResource.Invoke(amount, type);
                        assignable = true;


                    }
                    else
                    {
                        LifeSaverManager.Instance.updateResourceUI.Invoke(type);
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
                        LifeSaverManager.Instance.updateParticipantResource.Invoke(amount, type);
                        assignable = true;

                    }
                    else
                    {
                        LifeSaverManager.Instance.updateResourceUI.Invoke(type);
                    }
                }

            }
        }

    }

    public void RegenerateAmount()
    {
        if (canInvoke)
        {
            amount += 2;
            LifeSaverManager.Instance.updateParticipantResource.Invoke(amount, type);
            Debug.Log($"with assinable from Resource participant   {assignable}");
        }

    }

}
