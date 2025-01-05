using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ResourceElementParticipant : ResourceElement
{
    public ResourceElementParticipant(Type type, int amount) : base(type, amount)
    {
        LifeSaverManager.Instance.updateResource.Invoke(amount, type);
    }

    public new void UpdateResource(Type type, int amount)
    {
        base.UpdateResource(type, amount);
        LifeSaverManager.Instance.updateResource.Invoke(amount, type);
    }

    public new void ConsumeResource()
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
                    LifeSaverManager.Instance.updateResource.Invoke(amount, type);
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

                    LifeSaverManager.Instance.updateResource.Invoke(amount, type);
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
                    LifeSaverManager.Instance.updateResource.Invoke(amount, type);
                    assignable = true;

                }
                else
                {
                    LifeSaverManager.Instance.updateResourceUI.Invoke(type);
                }
            }

        }

    }

    public void RegenerateAmount()
    {
        amount += 2;
        LifeSaverManager.Instance.updateResource.Invoke(amount, type);
        Debug.Log($"with assinable from Resource participant   {assignable}");
    }

}
