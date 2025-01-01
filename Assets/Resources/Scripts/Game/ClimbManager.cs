using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class ClimbManager : MonoBehaviour
{
    [SerializeField] private CharacterController _characterController;
    [SerializeField] private InputActionReference _joystickAction;
    [SerializeField] private LayerMask _climbableLayer;
    [SerializeField] private float _climbSpeed = 3f;

    private bool _isClimbing;
    private Transform _ropeTransform;

    private void OnEnable()
    {
        _joystickAction.action.performed += OnJoystickMove;
    }

    private void OnDisable()
    {
        _joystickAction.action.performed -= OnJoystickMove;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (((1 << other.gameObject.layer) & _climbableLayer) == 0) return;

        StartClimbing(other.transform);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.transform != _ropeTransform) return;

        StopClimbing();
    }

    private void StartClimbing(Transform rope)
    {
        _isClimbing = true;
        _ropeTransform = rope;
        _characterController.enabled = false;
    }

    private void StopClimbing()
    {
        _isClimbing = false;
        _ropeTransform = null;
        _characterController.enabled = true;
    }

    private void OnJoystickMove(InputAction.CallbackContext context)
    {
        if (!_isClimbing) return;

        var input = context.ReadValue<Vector2>();
        float verticalMovement = input.y * _climbSpeed * Time.deltaTime;

        if (_ropeTransform == null) return;

        transform.position += Vector3.up * verticalMovement;
    }
}
