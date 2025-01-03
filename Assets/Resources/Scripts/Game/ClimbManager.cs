using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Movement;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;

public class ClimbManager : MonoBehaviour
{
    [SerializeField] private CharacterController _characterController;
    [SerializeField] private InputActionReference _joystickAction;
    [SerializeField] private LayerMask _climbableLayer;
    [SerializeField] private float _climbSpeed = 3f;
    [SerializeField] private DynamicMoveProvider _moveProvider;

    private bool _isClimbing;
    private Transform _ropeTransform;
    private Transform _forwardSource;

    private void OnEnable()
    {
        _joystickAction.action.performed += OnJoystickMove;
        _joystickAction.action.canceled += OnJoystickStop;

        if (_moveProvider != null)
        {
            _forwardSource = _moveProvider.forwardSource;
        }
    }

    private void OnDisable()
    {
        _joystickAction.action.performed -= OnJoystickMove;
        _joystickAction.action.canceled -= OnJoystickStop;
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
        if (_moveProvider != null)
        {
            _moveProvider.enabled = false; 
        }
    }

    private void StopClimbing()
    {
        _isClimbing = false;
        _ropeTransform = null;
        _characterController.enabled = true;
        if (_moveProvider != null)
        {
            _moveProvider.enabled = true; 
        }
    }

    private void OnJoystickMove(InputAction.CallbackContext context)
    {
        if (!_isClimbing || _forwardSource == null) return;

        var input = context.ReadValue<Vector2>();
        float verticalMovement = input.y * _climbSpeed * Time.deltaTime;

        if (_ropeTransform == null) return;

        // Move along the rope direction
        Vector3 climbDirection = Vector3.up * verticalMovement;
        transform.position += climbDirection;
    }

    private void OnJoystickStop(InputAction.CallbackContext context)
    {
        // Stop any active climbing movement
    }
}
