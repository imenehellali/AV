using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;
using static UnityEngine.InputSystem.InputAction;

public class JumpManager : MonoBehaviour
{
    [SerializeField]
    private CharacterController _characterController;
    [SerializeField]
    private TextMeshProUGUI _testVariable;

   [SerializeField]
    private InputActionReference _triggerAction;

    [SerializeField]
    private LineRenderer _lineRenderer;

    [SerializeField]
    private float _moveSpeed = 5f;

    [SerializeField]
    private DynamicMoveProvider _dynamicMoveProvider;

    private bool _isJumping = false;
    private List<Vector3> _savedArcPoints = new List<Vector3>();
    private int _currentPointIndex = 0;

    private void OnEnable()
    {
        _triggerAction.action.started += OnTriggerPressed;
    }

    private void OnDisable()
    {
        _triggerAction.action.started -= OnTriggerPressed;
    }

    private void OnTriggerPressed(InputAction.CallbackContext callbackContext)
    {
       if(callbackContext.ReadValueAsButton())
        {
            _testVariable.text = "triggered ME from Jump";

                if (!_lineRenderer.enabled || _lineRenderer.positionCount == 0)
                return;
            else
            {
                _dynamicMoveProvider.enabled = false;
                StartCoroutine(MoveAlongArc());
            }
           
        }


    }
    private IEnumerator MoveAlongArc()
    {
        if (_isJumping)
        {
            _dynamicMoveProvider.enabled = true;
            _lineRenderer.positionCount = 0;
            _lineRenderer.enabled = false;
            _savedArcPoints.Clear();
            yield break;
        }
        else
        {
            _savedArcPoints.Clear();

            for (int i = 0; i < _lineRenderer.positionCount; i++)
            {
                _savedArcPoints.Add(_lineRenderer.transform.TransformPoint(_lineRenderer.GetPosition(i)));
            }
            _isJumping = true;
            _currentPointIndex = 0;
            while (_currentPointIndex < _savedArcPoints.Count)
            {
                Vector3 currentPoint = _characterController.transform.position;
                Vector3 targetPoint = _savedArcPoints[_currentPointIndex];
                _currentPointIndex++;
                _characterController.Move(Vector3.Lerp(currentPoint, targetPoint, _moveSpeed * Time.deltaTime) - currentPoint);

                yield return null;
            }
            if (_currentPointIndex >= _savedArcPoints.Count)
            {
                _dynamicMoveProvider.enabled = true;
                _isJumping = false;
                _savedArcPoints.Clear();
            }
        }

    }

}
