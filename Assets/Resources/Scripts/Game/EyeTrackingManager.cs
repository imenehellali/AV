using UnityEngine;
using Unity.XR.PXR;
using UnityEngine.XR;
using TMPro;
using UnityEngine.InputSystem.XR;

public class EyeTrackingManager : MonoBehaviour
{
    public LineRenderer lineRendererXR;
    public LineRenderer lineRendererPico;
    public LineRenderer lineRendererWS;
    public LineRenderer lineRendererWWC;


    public Transform Origin;
    private Vector3 combineEyeGazeVector;
    private Vector3 combineEyeGazeOriginOffset;
    private Vector3 combineEyeGazeOrigin;

    private Vector2 primary2DAxis;
    private Vector2 leftEyePrimary2DAxis;

    private RaycastHit hitinfo;
    private Transform selectedObj;

    private EyeTrackingStartInfo startInfo;
    private EyeTrackingData _eyeData;
    private EyeTrackingDataGetInfo _eyeDataGetInfo;
    private PerEyeData _leftEye, _rightEye, _combinedEye;

    [SerializeField] private TextMeshProUGUI _LOpeness, _ROpeness, _LPose, _RPose, _CPose, _CDPose;

    private bool eyeTrackingstarted = false;

    public static EyeTrackingManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        int eyeTrackingGranted = PXR_MotionTracking.WantEyeTrackingService();
        startInfo = new EyeTrackingStartInfo { needCalibration = 0, mode = EyeTrackingMode.PXR_ETM_BOTH };
        _eyeDataGetInfo = new EyeTrackingDataGetInfo
        {
            displayTime = 0,
            flags = EyeTrackingDataGetFlags.PXR_EYE_DEFAULT | EyeTrackingDataGetFlags.PXR_EYE_POSITION | EyeTrackingDataGetFlags.PXR_EYE_ORIENTATION
        };
    }

    void Start()
    {
        _CPose.text=$"PUSDur:  {GameSettings.Instance.BetweenSceneDuration} levels count {GameSettings.Instance.LevelSequence.Length} ";
        combineEyeGazeOriginOffset = Origin.transform.position;
        combineEyeGazeOriginOffset.y += 1.5f;

        InitializeLineRenderer(lineRendererXR, Color.red);
        lineRendererXR.enabled = false;
        InitializeLineRenderer(lineRendererPico, Color.yellow);
        lineRendererPico.enabled = false;
        InitializeLineRenderer(lineRendererWS, Color.cyan);
        lineRendererWS.enabled = false;
        InitializeLineRenderer(lineRendererWS, Color.green);
        lineRendererWWC.enabled= false;

        eyeTrackingstarted = PXR_MotionTracking.StartEyeTracking(ref startInfo) == 0;
    }

    private void XRCenterEye()
    {
        Vector3 centerEyePosition = combineEyeGazeOriginOffset;
        Quaternion centerEyeRotation = Quaternion.identity;
        bool hasEyePosition = InputDevices.GetDeviceAtXRNode(XRNode.CenterEye).TryGetFeatureValue(CommonUsages.centerEyePosition, out centerEyePosition);
        bool hasEyeRotation = InputDevices.GetDeviceAtXRNode(XRNode.CenterEye).TryGetFeatureValue(CommonUsages.centerEyeRotation, out centerEyeRotation);
        Debug.Log($"has eye position {hasEyePosition}  and has eye rotation  {hasEyeRotation}  and eye tracking started {eyeTrackingstarted}");

        if (hasEyePosition && hasEyeRotation)
        {
            Debug.Log("From XR center Eye real Tracking ");

            combineEyeGazeOrigin += centerEyePosition;//+ combineEyeGazeOriginOffset;
            combineEyeGazeVector = centerEyeRotation * Vector3.forward;

            HandleGazeTarget(lineRendererXR, combineEyeGazeOrigin, combineEyeGazeVector);

            _CDPose.text = $"Custom Orientation both eyes: {combineEyeGazeVector}";

        }
    }
   
    private void XRPerEye()
    {
        Vector3 _LeyePos = Vector3.zero;
        Vector3 _ReyePos = Vector3.zero;
        Quaternion _LeyeRot = Quaternion.identity;
        Quaternion _ReyeRot = Quaternion.identity;
        bool _gotEye = InputDevices.GetDeviceAtXRNode(XRNode.LeftEye).TryGetFeatureValue(CommonUsages.leftEyePosition, out _LeyePos);
        _gotEye &= InputDevices.GetDeviceAtXRNode(XRNode.RightEye).TryGetFeatureValue(CommonUsages.rightEyePosition, out _ReyePos);
        _gotEye &= InputDevices.GetDeviceAtXRNode(XRNode.LeftEye).TryGetFeatureValue(CommonUsages.leftEyeRotation, out _LeyeRot);
        _gotEye &= InputDevices.GetDeviceAtXRNode(XRNode.RightEye).TryGetFeatureValue(CommonUsages.rightEyeRotation, out _ReyeRot);


        if (_gotEye)
        {
            Debug.Log("From XR eye approximation tracking");

            combineEyeGazeOrigin = (_LeyePos + _ReyePos) / 2.0f;
            combineEyeGazeVector = ((_LeyeRot * Vector3.forward) + (_ReyeRot * Vector3.forward)).normalized;

            HandleGazeTarget(lineRendererWS, (combineEyeGazeOriginOffset + combineEyeGazeOrigin), combineEyeGazeVector);
        }

    }
    void Update()
    {

        if (eyeTrackingstarted && PXR_MotionTracking.GetEyeTrackingData(ref _eyeDataGetInfo, ref _eyeData) == 0)
        {
            Debug.Log("From Pico Eye tracking");

            UpdateEyeTrackingData();
            Vector3 givenWorldVector = new Vector3(_eyeData.eyeDatas[2].pose.position.x,
                _eyeData.eyeDatas[2].pose.position.y, _eyeData.eyeDatas[2].pose.position.z);
            HandleGazeTarget(lineRendererPico, combineEyeGazeOrigin, givenWorldVector);
        }

       
        Vector3 headPosition = Vector3.zero;
        Quaternion headRotation = Quaternion.identity;

        if (InputDevices.GetDeviceAtXRNode(XRNode.Head).TryGetFeatureValue(CommonUsages.devicePosition, out headPosition) &&
            InputDevices.GetDeviceAtXRNode(XRNode.Head).TryGetFeatureValue(CommonUsages.deviceRotation, out headRotation))
        {
            Debug.Log("From middle forehead approximation tracking");

            combineEyeGazeOrigin = headPosition + (headRotation * new Vector3(0, 0.1f, 0.05f));
            combineEyeGazeVector = headRotation * Vector3.forward;

            HandleGazeTarget(lineRendererWWC, (combineEyeGazeOriginOffset + combineEyeGazeOrigin), combineEyeGazeVector);
        }
    }

    private void InitializeLineRenderer(LineRenderer lineRenderer, Color color)
    {
        lineRenderer.startWidth = 0.01f;
        lineRenderer.endWidth = 0.01f;
        lineRenderer.startColor = color;
        lineRenderer.endColor = color;
    }
    private void HandleGazeTarget(LineRenderer lineRenderer, Vector3 origin, Vector3 vector)
    {
        lineRenderer.enabled=true;
        Ray ray = new Ray(origin, vector);
        lineRenderer.SetPosition(0, origin);

        if (Physics.Raycast(origin, vector, out hitinfo, float.MaxValue))
        {
            lineRenderer.SetPosition(1, hitinfo.point);
            HandleSelectedObject(hitinfo.collider.transform);
        }
        else
        {
            lineRenderer.SetPosition(1, origin + vector * 1000f);
            ResetSelectedObject();
        }
    }

    private void HandleSelectedObject(Transform newObj)
    {
        if (selectedObj != null && selectedObj != newObj && selectedObj.GetComponent<ETObject>() != null)
        {
            selectedObj.GetComponent<ETObject>().UnFocused();
        }
        selectedObj = newObj;
        selectedObj?.GetComponent<ETObject>()?.IsFocused();
    }

    private void ResetSelectedObject()
    {
        if (selectedObj != null && selectedObj.GetComponent<ETObject>() != null)
        {
            selectedObj.GetComponent<ETObject>().UnFocused();
        }
        selectedObj = null;
    }

    private void UpdateEyeTrackingData()
    {
        _leftEye = _eyeData.eyeDatas[0];
        _rightEye = _eyeData.eyeDatas[1];
        _combinedEye = _eyeData.eyeDatas[2];

        _LOpeness.text = $"Openness Left: {_leftEye.openness}";
        _ROpeness.text = $"Openness Right: {_rightEye.openness}";
        _LPose.text = $"Both Orientation: {_combinedEye.pose.orientation.x}  {_combinedEye.pose.orientation.x}  {_combinedEye.pose.orientation.x}";
        _RPose.text = $"Both Position: {_combinedEye.pose.position.x}  {_combinedEye.pose.position.y}  {_combinedEye.pose.position.z}";
    }
}
