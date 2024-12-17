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
    private Renderer _material;
    private Vector4 _color;
    [Header("Ghost Properties")]
    public bool ghostDrunken;
    public bool ghostRed;
    public bool ghostDead;

    //Movement Variables
    private float radius = 3f;  // Radius of the movement circle
    private float rotationSpeed = 30f;
    private void Start()
    {
        GazeObject=GetComponent<ETObject>();
        Activate.action.started += ShootGhost;
        _material.enabled = false;
        StartCoroutine(MoveAndDestroyGhost(this.transform,rotationSpeed));
    }
    private IEnumerator MoveAndDestroyGhost(Transform ghost, float rotationSpeed)
    {
        float circleDuration = 360f / rotationSpeed;
        float _elapsedTime = 0f;
        // Save the initial position of the ghost for circular movement
        Vector3 initialPosition = ghost.position;
        Quaternion initialRotation = ghost.rotation;

        while (_elapsedTime < circleDuration)
        {
            _elapsedTime += Time.deltaTime;

            // Rotate the ghost around its own forward axis
            ghost.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);

            // Move the ghost in a circular path in its own local forward direction
            float angle = rotationSpeed * _elapsedTime;
            Vector3 offset = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * radius;
            ghost.position = initialPosition + ghost.forward * offset.magnitude;

            // Ensure ghost stays in the upper hemisphere
            ghost.position = new Vector3(ghost.position.x, Mathf.Clamp(ghost.position.y, 0f, 3f), ghost.position.z);

            yield return null;
        }
        //remove from list and destroy if time elapsed but ghost still alive
        if (_elapsedTime >= circleDuration)
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
        if (GazeObject.IsGazeLocked())
        {
            ghostDead = true;
            GhostBusterManager.Instance.killedGhost.Invoke(this);
        }
    }

    public void ResetGlow()
    {
        _material.enabled=false;
    }
    public void SetGlow()
    {
        _material.enabled=true; 
    }
}
