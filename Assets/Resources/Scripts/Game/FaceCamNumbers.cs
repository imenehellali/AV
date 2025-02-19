using UnityEditor.Rendering;
using UnityEngine;

public class FaceCamNumbers : MonoBehaviour
{
    private Transform _cameraTransform;
    void Start()
    {
        _cameraTransform = Camera.main != null ?Camera.main.transform : null;
    }

    void Update()
    {
        if (_cameraTransform == null) return;

        Vector3 _camForward = _cameraTransform.forward;
        _camForward.Normalize();

        transform.rotation = Quaternion.LookRotation(_camForward, Vector3.up);
    }
}
