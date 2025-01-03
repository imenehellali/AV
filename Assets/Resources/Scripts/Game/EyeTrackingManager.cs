using UnityEngine;
using Unity.XR.PXR;
using UnityEngine.XR;
using TMPro;

public class EyeTrackingManager : MonoBehaviour
{
    public LineRenderer lineRendererCustom;
    public LineRenderer lineRendererGivenWorld;
    public Transform Origin;

    private Vector3 combineEyeGazeVector;
    private Vector3 combineEyeGazeOriginOffset;
    private Vector3 combineEyeGazeOrigin;
    private Matrix4x4 headPoseMatrix;
    private Matrix4x4 originPoseMatrix;

    private Vector3 combineEyeGazeVectorInWorldSpace;
    private Vector3 combineEyeGazeOriginInWorldSpace;

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
        combineEyeGazeOriginOffset = Vector3.zero;
        originPoseMatrix = Origin.localToWorldMatrix;

        InitializeLineRenderer(lineRendererCustom, Color.red);
        InitializeLineRenderer(lineRendererGivenWorld, Color.yellow);

        eyeTrackingstarted = PXR_MotionTracking.StartEyeTracking(ref startInfo) == 0;
    }

    void Update()
    {
        AdjustOffsetWithControllerInput();

        PXR_EyeTracking.GetHeadPosMatrix(out headPoseMatrix);
        PXR_EyeTracking.GetCombineEyeGazeVector(out combineEyeGazeVector);
        PXR_EyeTracking.GetCombineEyeGazePoint(out combineEyeGazeOrigin);

        combineEyeGazeOrigin += combineEyeGazeOriginOffset;
        combineEyeGazeOriginInWorldSpace = originPoseMatrix.MultiplyPoint(headPoseMatrix.MultiplyPoint(combineEyeGazeOrigin));
        combineEyeGazeVectorInWorldSpace = originPoseMatrix.MultiplyVector(headPoseMatrix.MultiplyVector(combineEyeGazeVector));

        HandleGazeTarget(lineRendererCustom, combineEyeGazeOriginInWorldSpace, combineEyeGazeVectorInWorldSpace);

        if (eyeTrackingstarted && PXR_MotionTracking.GetEyeTrackingData(ref _eyeDataGetInfo, ref _eyeData) == 0)
        {
            UpdateEyeTrackingData();
            Vector3 givenWorldVector = originPoseMatrix.MultiplyVector(headPoseMatrix.MultiplyVector(
                new Vector3(_combinedEye.pose.orientation.x, _combinedEye.pose.orientation.y, _combinedEye.pose.orientation.z)));
            HandleGazeTarget(lineRendererGivenWorld, combineEyeGazeOriginInWorldSpace, givenWorldVector);
        }
    }

    private void InitializeLineRenderer(LineRenderer lineRenderer, Color color)
    {
        lineRenderer.startWidth = 0.01f;
        lineRenderer.endWidth = 0.01f;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = color;
        lineRenderer.endColor = color;
    }

    private void AdjustOffsetWithControllerInput()
    {
        if (InputDevices.GetDeviceAtXRNode(XRNode.RightEye).TryGetFeatureValue(CommonUsages.primary2DAxis, out primary2DAxis) &&
            InputDevices.GetDeviceAtXRNode(XRNode.LeftEye).TryGetFeatureValue(CommonUsages.primary2DAxis, out leftEyePrimary2DAxis))
        {
            combineEyeGazeOriginOffset.x += (primary2DAxis.x + leftEyePrimary2DAxis.x) * 0.0005f;
            combineEyeGazeOriginOffset.y += (primary2DAxis.y + leftEyePrimary2DAxis.y) * 0.0005f;
        }
    }

    private void HandleGazeTarget(LineRenderer lineRenderer, Vector3 origin, Vector3 vector)
    {
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
        _LPose.text = $"Both Orientation X: {_combinedEye.pose.orientation.x}";
        _RPose.text = $"Both Orientation Y: {_combinedEye.pose.orientation.y}";
        _CPose.text = $"Both Orientation Z: {_combinedEye.pose.orientation.z}";
        _CDPose.text = $"Custom Orientation Z: {combineEyeGazeVectorInWorldSpace}";
    }
}
