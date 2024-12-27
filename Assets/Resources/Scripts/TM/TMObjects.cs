using System.Collections;
using System.Collections.Generic;
using Unity.XR.PXR;
using UnityEngine;

public class TMObjects : MonoBehaviour
{
    public enum Type
    {
        Mystery,
        BlueDiamond,
        PinkDiamond,
        BlueCoin,
        PinkCoin,
        DeathTrap
    }
    public PathSetting belongsTo;
    public int _amount;
    public Type type;

    private void Start()
    {
       
        if (type == Type.Mystery)
        {
            _amount = Random.Range(-100, 101);
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        //Raise amount to TM Manager 
        if (other != null && other.gameObject.GetComponent<PXR_Manager>() != null)
        {
            if(type==Type.DeathTrap)
            {
                ThrillMinerManager.Instance.questFailed.Invoke(belongsTo);
            }
            else
            {
                //Raise amount to TM Manager 
                ThrillMinerManager.Instance.addAmount(_amount);
                //Consume
                Destroy(this.gameObject);
            }
            
        }
    }

    private void SlowRotate()
    {
        float rotationSpeed = 45.0f;
        transform.rotation *= Quaternion.Euler(0, rotationSpeed * Time.deltaTime, 0);

    }

    private void Update()
    {
        if(type!=Type.DeathTrap)
        {
            SlowRotate();
        }
    }
}
