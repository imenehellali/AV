using UnityEngine;

public class LSFanMvt : MonoBehaviour
{
    [SerializeField] private Transform _fan;
    [SerializeField] private float _speed = 100f;

    void Update()
    {
        if (_fan != null)
        {
            _fan.Rotate(Vector3.up, _speed * Time.deltaTime, Space.Self);
        }
    }
}
