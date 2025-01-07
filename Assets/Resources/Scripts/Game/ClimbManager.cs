using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Movement;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;

public class ClimbManager : MonoBehaviour
{
    [SerializeField] private Transform _xrOrig;
    [SerializeField] private InputActionReference _joystickAction;
    [SerializeField] private float _climbSpeed = 3f;
    [SerializeField] private DynamicMoveProvider _moveProvider;

    public bool _isClimbing = false;
    public Transform ropeTransform = null;

    private Vector3 desiredMove = Vector3.zero;
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
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"entered something  {other.gameObject.layer}");
        if (other.gameObject.layer==3 && ropeTransform==null)
        {
            Debug.Log("detected a climbable");
            _isClimbing = true;
            ropeTransform = other.transform;    

            if (_moveProvider.enabled) _moveProvider.enabled = false;
            _xrOrig.gameObject.GetComponent<NavMeshAgent>().enabled = false;
        }

    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == 3 && ropeTransform!=null)
        {
            _isClimbing = false;
           
            _xrOrig.gameObject.GetComponent<NavMeshAgent>().enabled = true;
            NavMeshHit hit;
            if (NavMesh.SamplePosition(_xrOrig.position, out hit, 2f, NavMesh.AllAreas))
            {
                // Move the XR Origin to the nearest NavMesh point
                _xrOrig.position = hit.position;
            }
            _moveProvider.enabled = true;
            ropeTransform = null;
        }
    }
    private void OnTriggerStay(Collider other)
    {
       
    }
    private void OnJoystickMove(InputAction.CallbackContext context)
    {
        if (ropeTransform!=null)
        {
            _isClimbing = true;
            Debug.Log("from climbing manager");
            var input = context.ReadValue<Vector2>();
            float verticalMovement = input.y * _climbSpeed * Time.deltaTime;
            _xrOrig.position+= Vector3.up * verticalMovement;
        }
    }
   
    private void OnJoystickStop(InputAction.CallbackContext context)
    {
        if (context.ReadValueAsButton() && ropeTransform != null)
        {
            _isClimbing = false;
        }
    }
}
