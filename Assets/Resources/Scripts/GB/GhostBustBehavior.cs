using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;
public class GhostBustBehavior : MonoBehaviour
{
    [SerializeField]
    private InputActionReference Activate;
    private ETObject GazeObject;
    public string _in = "started";
    [SerializeField]
    private SkinnedMeshRenderer _material;
    [Header("Ghost Properties")]
    public bool ghostDrunken;
    public bool ghostRed;
    public bool ghostDead;

    //Movement Variables
    private float radius = 3f;
    private float rotationSpeed = 30f;

    //Variables for the GBStats
    public List<float> focusDurations = new List<float>();
    public float _elapsedTime = 0f;


    private float circleDuration = 0f;
    private Vector3 initialPosition = Vector3.zero;
    private Quaternion initialRotation = Quaternion.identity;
    private void Start()
    {
        GazeObject = GetComponent<ETObject>();
        Activate.action.started += ShootGhost;
        _material.enabled = false;
        circleDuration = 360f / rotationSpeed;
        initialPosition = gameObject.transform.position;
        initialRotation = gameObject.transform.rotation;
    }

    private void Update()
    {
        if (_elapsedTime < circleDuration && !ghostDead)
        {
            _elapsedTime += Time.deltaTime;
            gameObject.transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);

            float angle = rotationSpeed * _elapsedTime;
            Vector3 offset = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * radius;
            gameObject.transform.position = initialPosition + gameObject.transform.forward * offset.magnitude;

            gameObject.transform.position = new Vector3(gameObject.transform.position.x, Mathf.Clamp(gameObject.transform.position.y, 1f, 3f), gameObject.transform.position.z);

        }
        else if (_elapsedTime >= circleDuration && !ghostDead)
        {
            Destroy(this.gameObject);
        }

    }
    private void OnDestroy()
    {
        Activate.action.started -= ShootGhost;
    }

    private void ShootGhost(InputAction.CallbackContext callbackContext)
    {
        if (callbackContext.ReadValueAsButton())
        {
            if (GazeObject.IsGazeLocked())
            {
                ghostDead = true;
                GhostBusterManager.Instance.killedGhost.Invoke(this);
            }
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
