using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;
using Unity.XR.CoreUtils;
using UnityEngine.AI;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Locomotion;

public class JumpManager : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI _testVariable;

    [SerializeField]
    private InputActionReference _triggerAction;
    [SerializeField]
    private InputActionReference LeftHandMoveInput;
    [SerializeField]
    private DynamicMoveProvider _dynamicMoveProvider;
    [SerializeField]
    private Transform XROrig;

    [SerializeField]
    private float _jumpHeight = 1.5f;
    private float jumpSpeed = 0.2f;
    private Vector2 input = Vector2.zero;
    private Vector3 desiredMove = Vector3.zero;


    [SerializeField]
    private float _gravity = -9.8f;

    private bool _isJumping = false;
    private int _remainingJumps = 3;

    private float _yVelocity = 0f;
    private float moveStep = 0.5f;

    private void OnEnable()
    {
        _triggerAction.action.started += OnTriggerPressed;
        LeftHandMoveInput.action.performed += GetMoveInput;
        Debug.Log($"is grounded : {XROrig.gameObject.GetComponent<NavMeshAgent>().isOnNavMesh}");
    }

    private void OnDisable()
    {
        _triggerAction.action.started -= OnTriggerPressed;
        LeftHandMoveInput.action.performed -= GetMoveInput;
    }

    private void Update()
    {
        if (!_isJumping || _yVelocity < 0f)
        {
            _yVelocity = 0f;
            _isJumping = false;
            _remainingJumps = 3;

            _dynamicMoveProvider.enabled = true;
        }

        else if
             (_isJumping && _yVelocity > 0f)
        {
            _yVelocity += _gravity * Time.deltaTime * jumpSpeed;
            desiredMove.y = _yVelocity;
            NavMeshHit hit;
            if (NavMesh.SamplePosition(desiredMove, out hit, .5f, NavMesh.AllAreas))
            {
                _isJumping = false;
            }
            else
            {

                XROrig.position = desiredMove;
            }
        }

    }

    private void OnTriggerPressed(InputAction.CallbackContext callbackContext)
    {
        if (callbackContext.ReadValueAsButton() && !XROrig.gameObject.GetComponent<ClimbManager>()._isClimbing
            && XROrig.gameObject.GetComponent<ClimbManager>().ropeTransform != null)
        {
            XROrig.gameObject.GetComponent<NavMeshAgent>().enabled = true;
            NavMeshHit hit;
            if (NavMesh.SamplePosition(XROrig.position, out hit, 2f, NavMesh.AllAreas))
            {

                XROrig.position = hit.position;
            }

            XROrig.gameObject.GetComponent<ClimbManager>().ropeTransform = null;
            _dynamicMoveProvider.enabled = true;
        }
        else if (callbackContext.ReadValueAsButton() && _remainingJumps > 0)
        {
            Debug.Log($"Jump triggered. Remaining jumps: {_remainingJumps}");
            _dynamicMoveProvider.enabled = false;
            _isJumping = true;
            _remainingJumps--;
            _yVelocity += Mathf.Sqrt(_jumpHeight * -.33f * _gravity);
            desiredMove = new Vector3(XROrig.position.x, XROrig.position.y + _yVelocity, XROrig.position.z);
            Debug.Log($"new jump position {desiredMove}");
            XROrig.position = desiredMove;

        }
        
    }
    private void GetMoveInput(InputAction.CallbackContext context)
    {
        if (_isJumping && _dynamicMoveProvider.enabled == false)
        {
            input = context.ReadValue<Vector2>();
            Debug.Log($"Calling input value in air : {input}");
            ComputeDesiredMove();
        }
        else
        {
            input = Vector2.zero;
        }

    }
    private void ComputeDesiredMove()
    {
        if (input != Vector2.zero)
        {
            Pose m_HeadTransform = Camera.main.transform.GetWorldPose();

            Vector3 forwardDirection = m_HeadTransform.forward;
            Vector3 rightDirection = m_HeadTransform.right;
            desiredMove += forwardDirection * input.y * moveStep + rightDirection * input.x * moveStep;

            Debug.Log($"desired move POSITION from jump in air :  {desiredMove}");
        }
    }

}
