using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
public class GhostBustBehavior : MonoBehaviour
{
    [SerializeField]
    private InputActionReference Activate;
    [SerializeField]
    private SkinnedMeshRenderer _material;
    [Header("Ghost Properties")]
    public bool ghostDrunken;
    public bool ghostRed;
    //Movement Variables
    private float radius = 3f;
    private float rotationSpeed = 30f;

    //Variables for the GBStats
    public List<float> focusDurations = new List<float>();
    public float _elapsedTime = 0f;

    private float circleDuration = 0f;
    private Vector3 initialPosition = Vector3.zero;
    private Quaternion initialRotation = Quaternion.identity;


    private void OnEnable()
    {
        Activate.action.started += ShootGhost;

    }
    private void Start()
    {
        _material.enabled = false;
        circleDuration = 360f / rotationSpeed;
        initialPosition = gameObject.transform.position;
        initialRotation = gameObject.transform.rotation;
    }

    private void Update()
    {
        if (_elapsedTime < circleDuration)
        {
            _elapsedTime += Time.deltaTime;
            gameObject.transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);

            float angle = rotationSpeed * _elapsedTime;
            Vector3 offset = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * radius;
            gameObject.transform.position = initialPosition + gameObject.transform.forward * offset.magnitude;

            gameObject.transform.position = new Vector3(gameObject.transform.position.x, Mathf.Clamp(gameObject.transform.position.y, 1f, 3f), gameObject.transform.position.z);

        }
        if (_elapsedTime >= circleDuration)
        {
            Destroy(gameObject);
        }

    }
    private void OnDisable()
    {
        Activate.action.started -= ShootGhost;

    }

    private void ShootGhost(InputAction.CallbackContext callbackContext)
    {
        if (callbackContext.ReadValueAsButton() && _material.enabled)
        {
            GhostBusterManager.Instance.killedGhost.Invoke(ghostDrunken, ghostRed, focusDurations.LastOrDefault(), _elapsedTime);
            Destroy(gameObject);
        }

    }

    public void ResetGlow()
    {
        _material.enabled = false;
    }
    public void SetGlow()
    {
        _material.enabled = true;
    }
}
