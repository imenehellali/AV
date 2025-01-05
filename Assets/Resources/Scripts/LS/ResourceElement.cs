using System.Collections;
using UnityEngine;

public class ResourceElement : MonoBehaviour
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
    public ResourceElement(Type type, int amount)
    {
        this.type = type;
        this.amount = amount;
    }
    public void UpdateResource(Type type, int amount)
    {
        this.amount = amount;
        this.type = type;
    }
    public void ConsumeResource()
    {
    }

}
