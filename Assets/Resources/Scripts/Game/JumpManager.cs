using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Interactors.Casters;

public class JumpManager : MonoBehaviour
{
   
    [SerializeField]
    private CharacterController _characterController;

    [SerializeField]
    private InputActionReference _joystickAction;

    [SerializeField]
    private LineRenderer _lineRenderer;

    [SerializeField]
    private float _jumpSpeed = 5f;

    [SerializeField]
    private float _gravity = -9.81f;

    private bool _isJumping = false;
    private bool _isGrounded = true;
    private float _verticalVelocity = 0f;
    private List<Vector3> _savedArcPoints = new List<Vector3>();

    [SerializeField]
    private TextMeshProUGUI _debugVar;

    private void OnEnable()
    {
        _joystickAction.action.performed += OnJoystickPerformed;
        _joystickAction.action.canceled += OnJoystickCanceled;
        _joystickAction.action.Enable();
    }

    private void OnDisable()
    {
        _joystickAction.action.performed -= OnJoystickPerformed;
        _joystickAction.action.canceled -= OnJoystickCanceled;
        _joystickAction.action.Disable();
    }
    private void Update()
    {
        if (!_isJumping)
        {
            _isGrounded = _characterController.isGrounded;

            if (_isGrounded && _verticalVelocity < 0)
            {
                _verticalVelocity = 0f; 
            }
        }
    }
    private void OnJoystickPerformed(InputAction.CallbackContext context)
    {
        if (_isJumping || !_isGrounded) return;

        if (_lineRenderer.enabled && _lineRenderer.positionCount > 0)
        {
            _savedArcPoints.Clear();
            for (int i = 0; i < _lineRenderer.positionCount; i++)
            {
                _savedArcPoints.Add(_lineRenderer.transform.TransformPoint(_lineRenderer.GetPosition(i)));
            }
            _debugVar.text = $"numPoints:  {_lineRenderer.positionCount}";
        }
    }

    private void OnJoystickCanceled(InputAction.CallbackContext context)
    {
        if (context.canceled && !_isJumping && _savedArcPoints.Count > 0)
        {
            StartCoroutine(JumpAlongArc());
        }
    }

    private IEnumerator JumpAlongArc()
    {
        _isJumping = true;

        for (int i = 0; i < _savedArcPoints.Count; i++)
        {
            Vector3 targetPoint = _savedArcPoints[i];

            while (Vector3.Distance(_characterController.transform.position, targetPoint) > 0.1f)
            {
                Vector3 direction = (targetPoint - _characterController.transform.position).normalized;
                Vector3 movement = direction * _jumpSpeed * Time.deltaTime;
                _verticalVelocity += _gravity * Time.deltaTime;
                movement.y += _verticalVelocity * Time.deltaTime;

                _characterController.Move(movement);
                if (_isGrounded && _verticalVelocity < 0)
                {
                    _verticalVelocity = 0f;
                }

                yield return null;
            }
        }

        _savedArcPoints.Clear();
        _isJumping = false;
        _isGrounded =  true;
        _verticalVelocity = 0f;
    }
}
