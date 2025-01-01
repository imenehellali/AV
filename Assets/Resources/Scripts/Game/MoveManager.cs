using UnityEngine;
using UnityEngine.InputSystem;

public class MoveManager : MonoBehaviour
{
    [SerializeField] private CharacterController _characterController;
    [SerializeField] private Transform _headTransform;
    [SerializeField] private InputActionReference _joystickAction;
    [SerializeField] private InputActionReference _headPositionAction;
    [SerializeField] private InputActionReference _headRotationAction;
    [SerializeField] private float _moveSpeed = 2f;
    [SerializeField] private float _rotationSpeed = 5f;

    private Vector3 _movementInput;

    private void OnEnable()
    {
        _joystickAction.action.performed += OnJoystickMove;
        _joystickAction.action.canceled += OnJoystickStop;
    }

    private void OnDisable()
    {
        _joystickAction.action.performed -= OnJoystickMove;
        _joystickAction.action.canceled -= OnJoystickStop;
    }

    private void Update()
    {
        UpdateHeadTransform();

        if (_movementInput != Vector3.zero)
        {
            MoveCharacter();
            RotateTowardsHeadDirection();
        }
    }

    private void OnJoystickMove(InputAction.CallbackContext context)
    {
        var input = context.ReadValue<Vector2>();
        _movementInput = new Vector3(input.x, 0, input.y);
    }

    private void OnJoystickStop(InputAction.CallbackContext context)
    {
        _movementInput = Vector3.zero;
    }

    private void UpdateHeadTransform()
    {
        Vector3 headPosition = _headPositionAction.action.ReadValue<Vector3>();
        Quaternion headRotation = _headRotationAction.action.ReadValue<Quaternion>();

        _headTransform.localPosition = headPosition;
        _headTransform.localRotation = headRotation;
    }

    private void MoveCharacter()
    {
        Vector3 move = _headTransform.forward * _movementInput.z + _headTransform.right * _movementInput.x;
        move.y = 0; // Ensure no vertical movement
        _characterController.Move(move * _moveSpeed * Time.deltaTime);
    }

    private void RotateTowardsHeadDirection()
    {
        Vector3 forward = _headTransform.forward;
        forward.y = 0; // Keep only horizontal rotation

        if (forward.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(forward);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * _rotationSpeed);
        }
    }
}
