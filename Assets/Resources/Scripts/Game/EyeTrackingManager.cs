using UnityEngine;
using Unity.XR.PXR;
using UnityEngine.XR;
using TMPro;
using UnityEngine.InputSystem.XR;
using Unity.XR.CoreUtils;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine.Experimental.GlobalIllumination;
using System.Runtime.CompilerServices;

public class EyeTrackingManager : MonoBehaviour
{

    public LineRenderer lineRendererLeft;
    public LineRenderer lineRendererRight;
    public LineRenderer lineRendererPico;

    public Transform Origin;
    private Vector3 combineEyeGazeVector;

    [SerializeField]
    private Transform _cameraOffset;
    private Pose _OriginOffset;

    private Vector3 combineEyeGazeOrigin;
    private Vector3 combineEyeGazeOriginOffset;
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

    private InputDevice _inputDevice;

    [SerializeField] private TextMeshProUGUI _LOpeness, _ROpeness, _LPose, _RPose, _CPose, _CDPose;

    private bool eyeTrackingstarted = false;
    private bool dataValid = false;


    private void Awake()
    {

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
        var devices = new List<InputDevice>();
        InputDevices.GetDevicesWithCharacteristics(InputDeviceCharacteristics.EyeTracking, devices);

        if (devices.Count > 0)
            _inputDevice = devices[0];

        _OriginOffset = _cameraOffset.GetLocalPose();

        lineRendererPico.enabled = false;
        lineRendererLeft.enabled = false;
        lineRendererRight.enabled = false;

        eyeTrackingstarted = PXR_MotionTracking.StartEyeTracking(ref startInfo) == 0;

        combineEyeGazeOriginOffset = Vector3.zero;
        combineEyeGazeVector = Vector3.zero;
        combineEyeGazeOrigin = Vector3.zero;
        originPoseMatrix = Origin.localToWorldMatrix;
    }

    // 7 Fucking Methods !! From Real Tracking To Approximations
    // All Possible Eye Tracking Methods of XR, Camera, Pico
    // ALll Used AS Fallback Methods Inside Update
    // Fallback Not only When Device, Driver, Tracker Not Available But Also If Rot == 0 Or Pos == 0
    // Ensuring Continuious Delivery!!       COME ON WOOOORKKKKKKKK
    private bool XRCenterEye()
    {
        bool datarEceived = false;

        Vector3 centerEyePosition = Vector3.zero;
        Quaternion centerEyeRotation = Quaternion.identity;

        bool hasEyePosition = InputDevices.GetDeviceAtXRNode(XRNode.CenterEye).TryGetFeatureValue(CommonUsages.centerEyePosition, out centerEyePosition);
        bool hasEyeRotation = InputDevices.GetDeviceAtXRNode(XRNode.CenterEye).TryGetFeatureValue(CommonUsages.centerEyeRotation, out centerEyeRotation);
        datarEceived = hasEyePosition && hasEyeRotation;
        if (hasEyePosition && hasEyeRotation)
        {

            combineEyeGazeOrigin = centerEyePosition + _OriginOffset.position + new Vector3(0f, 1.7f, 0f);
            combineEyeGazeVector = (centerEyeRotation * Vector3.forward).normalized;

            _LOpeness.text = $"Has eye center position {combineEyeGazeOrigin}";
            _ROpeness.text = $"Has  eye center rotation  {combineEyeGazeVector}";

            datarEceived &= HandleGazeTarget(lineRendererPico, combineEyeGazeOrigin, combineEyeGazeVector);
            datarEceived &= combineEyeGazeVector != Vector3.zero;
        }
        return datarEceived;
    }
    private bool PICOEye()
    {
        bool dataReceived = false;

        if (eyeTrackingstarted && PXR_MotionTracking.GetEyeTrackingData(ref _eyeDataGetInfo, ref _eyeData) == 0
            && _eyeData.eyeDatas[2].isPoseValid == 0)
        {

            dataReceived = true;
            Vector3 _eyePos = new Vector3(_eyeData.eyeDatas[2].pose.position.x,
                _eyeData.eyeDatas[2].pose.position.y, _eyeData.eyeDatas[2].pose.position.z) + _OriginOffset.position + new Vector3(0f, 1.7f, 0f);

            Vector3 _eyeDir = ((new Quaternion(_eyeData.eyeDatas[2].pose.orientation.x,
                _eyeData.eyeDatas[2].pose.orientation.y,
                _eyeData.eyeDatas[2].pose.orientation.z,
                _eyeData.eyeDatas[2].pose.orientation.w)) * Vector3.forward).normalized;

            Vector3 _LeyePos = Origin.position + new Vector3(0f, 1.7f, 0f) + new Vector3(_eyeData.eyeDatas[0].pose.position.x,
                 _eyeData.eyeDatas[0].pose.position.y,
                  _eyeData.eyeDatas[0].pose.position.z);
            Vector3 _ReyePos = Origin.position + new Vector3(0f, 1.7f, 0f) + new Vector3(_eyeData.eyeDatas[1].pose.position.x,
                 _eyeData.eyeDatas[1].pose.position.y,
                  _eyeData.eyeDatas[1].pose.position.z);

            Vector3 _LeyeDir = ((new Quaternion(_eyeData.eyeDatas[0].pose.orientation.x,
               _eyeData.eyeDatas[0].pose.orientation.y,
               _eyeData.eyeDatas[0].pose.orientation.z,
               _eyeData.eyeDatas[0].pose.orientation.w)) * Vector3.forward).normalized;

            Vector3 _ReyeDir = ((new Quaternion(_eyeData.eyeDatas[1].pose.orientation.x,
               _eyeData.eyeDatas[1].pose.orientation.y,
               _eyeData.eyeDatas[1].pose.orientation.z,
               _eyeData.eyeDatas[1].pose.orientation.w)) * Vector3.forward).normalized;

            _LOpeness.text = $"Pico Center Eye Position {_eyePos}";
            _ROpeness.text = $"Pico Cetner Eye Direction Forward {_eyeDir}";

            _LPose.text = $"Pico Left Eye position {_LeyePos}";
            _RPose.text = $"Pico Right Eye position{_ReyePos}";

            _CPose.text = $"Pico Left Eye orientation {_LeyeDir}";
            _CDPose.text = $"Pico Right Eye orientation {_ReyeDir}";

            dataReceived = HandleGazeTarget(lineRendererLeft, _LeyePos, _LeyeDir);
            dataReceived |= HandleGazeTarget(lineRendererRight, _ReyePos, _ReyeDir);
        }
        return dataReceived;
    }
    private bool XRPerEye()
    {
        bool dataReceived = false;
        Vector3 _LeyePos = Vector3.zero;
        Vector3 _ReyePos = Vector3.zero;

        Quaternion _LeyeRot = Quaternion.identity;
        Quaternion _ReyeRot = Quaternion.identity;

        bool _gotLEyeP = InputDevices.GetDeviceAtXRNode(XRNode.LeftEye).TryGetFeatureValue(CommonUsages.leftEyePosition, out _LeyePos);
        bool _gotREyeP = InputDevices.GetDeviceAtXRNode(XRNode.RightEye).TryGetFeatureValue(CommonUsages.rightEyePosition, out _ReyePos);
        bool _gotLEyeR = InputDevices.GetDeviceAtXRNode(XRNode.LeftEye).TryGetFeatureValue(CommonUsages.leftEyeRotation, out _LeyeRot);
        bool _gotREyeR = InputDevices.GetDeviceAtXRNode(XRNode.RightEye).TryGetFeatureValue(CommonUsages.rightEyeRotation, out _ReyeRot);

        _CPose.text = $"Got XR Left Eye position {_gotLEyeP}";
        _CDPose.text = $"Got XR Right Eye position {_gotREyeP}";

        dataReceived = _gotLEyeP || _gotREyeP || _gotLEyeR || _gotREyeR;
        if (dataReceived)
        {

            Vector3 _origLeft = Origin.position + new Vector3(0f, 1.7f, 0f) + _LeyePos;
            Vector3 _origRight = Origin.position + new Vector3(0f, 1.7f, 0f) + _ReyePos;

            Vector3 _vectorLeft = (_LeyeRot * Vector3.forward).normalized;
            Vector3 _vectorRight = (_ReyeRot * Vector3.forward).normalized;

            _LPose.text = $"Left Eye position {_LeyePos}";
            _RPose.text = $"Right Eye position{_ReyePos}";
            _LOpeness.text = $"Left Eye Rotation: {_LeyeRot.eulerAngles}";
            _ROpeness.text = $"Right Eye Rotation: {_ReyeRot.eulerAngles}";

            dataReceived = HandleGazeTarget(lineRendererLeft, _origLeft, _vectorLeft);
            dataReceived |= HandleGazeTarget(lineRendererRight, _origLeft, _vectorLeft);

        }
        return dataReceived;
    }
    private bool XRCameraCenterHead()
    {
        bool dataReceived = false;
        Vector3 headPosition = Vector3.zero;
        Quaternion headRotation = Quaternion.identity;

        if (InputDevices.GetDeviceAtXRNode(XRNode.Head).TryGetFeatureValue(CommonUsages.devicePosition, out headPosition) &&
           InputDevices.GetDeviceAtXRNode(XRNode.Head).TryGetFeatureValue(CommonUsages.deviceRotation, out headRotation))
        {
            dataReceived = true;
            combineEyeGazeOrigin = headPosition + _OriginOffset.position + new Vector3(0f, 1.7f, 0f);
            combineEyeGazeVector = (headRotation * Vector3.forward).normalized;

            dataReceived &= combineEyeGazeOrigin != Vector3.zero && combineEyeGazeVector != Vector3.zero;

            _LPose.text = $"Head camera positon from Head Data {headPosition}";
            _RPose.text = $"Head camera rotation from Head Data {headRotation}";
            _LOpeness.text = $"Head camera position with offset: {combineEyeGazeOrigin}";
            _ROpeness.text = $"Head camera Diretion: {combineEyeGazeVector}";

            dataReceived &= HandleGazeTarget(lineRendererPico, combineEyeGazeOrigin, combineEyeGazeVector);
        }
        return dataReceived;
    }

    private bool XRFixationPoint()
    {
        bool dataReceived = false;
        if (_inputDevice.isValid)
        {
            Vector3 fixationPoint = Vector3.zero;
            InputFeatureUsage<Vector3> fixationPointUsage = new InputFeatureUsage<Vector3>("FixationPoint");
            dataReceived = _inputDevice.TryGetFeatureValue(fixationPointUsage, out fixationPoint);
            if (dataReceived)
            {
                Vector3 eyePosition = _OriginOffset.position;

                _LPose.text = $"Fixation positon {fixationPoint}";
                _RPose.text = $"origin camera offset {eyePosition}";

                dataReceived &= HandleGazeTarget(lineRendererPico, eyePosition, fixationPoint);
                dataReceived &= fixationPoint != Vector3.zero;
            }
        }
        return dataReceived;
    }
    private bool CameraCenterCutom()
    {
        bool dataReceived = false;
        Vector3 headPosition = _OriginOffset.position + new Vector3(0f, 1.7f, 0f);
        Quaternion headRotation = _OriginOffset.rotation;

        Debug.Log($"Head camera positon from CAMERA Original {headPosition} , rotation {headRotation}");
        _LPose.text = $"Head camera positon from CAMERA Original {headPosition}";
        _RPose.text = $"Head camera rotation from CAMERA Original {headRotation}";

        combineEyeGazeVector = (headRotation * Vector3.forward).normalized;

        dataReceived = true && headPosition != Vector3.zero && combineEyeGazeVector != Vector3.zero;
        dataReceived &= HandleGazeTarget(lineRendererPico, headPosition, combineEyeGazeVector);

        return dataReceived;
    }
    private bool PXRTracking()
    {
        bool dataReceived = false;
        if (InputDevices.GetDeviceAtXRNode(XRNode.RightHand).TryGetFeatureValue(CommonUsages.primary2DAxis, out primary2DAxis))
        {

            combineEyeGazeOriginOffset.x += primary2DAxis.x * 0.001f;
            combineEyeGazeOriginOffset.y += primary2DAxis.y * 0.001f;
            dataReceived = true;

        }
        dataReceived &= PXR_EyeTracking.GetHeadPosMatrix(out headPoseMatrix);
        dataReceived &= PXR_EyeTracking.GetCombineEyeGazeVector(out combineEyeGazeVector);
        dataReceived &= PXR_EyeTracking.GetCombineEyeGazePoint(out combineEyeGazeOrigin);

        //Translate Eye Gaze point and vector to world space
        combineEyeGazeOrigin += combineEyeGazeOriginOffset;
        combineEyeGazeOriginInWorldSpace = originPoseMatrix.MultiplyPoint(headPoseMatrix.MultiplyPoint(combineEyeGazeOrigin));
        combineEyeGazeVectorInWorldSpace = originPoseMatrix.MultiplyVector(headPoseMatrix.MultiplyVector(combineEyeGazeVector));

        _LPose.text = $"PXR EyeTracking offset {combineEyeGazeOriginOffset}";
        _RPose.text = $"PXR EyeTracking origin {combineEyeGazeOrigin}";
        _LOpeness.text = $"PXR EyeTracking origin World: {combineEyeGazeOriginInWorldSpace}";
        _ROpeness.text = $"PXR EyeTracking Dir World: {combineEyeGazeVectorInWorldSpace}";

        dataReceived &= combineEyeGazeOriginInWorldSpace != Vector3.zero && combineEyeGazeVectorInWorldSpace != Vector3.zero;

        dataReceived &= HandleGazeTarget(lineRendererPico, combineEyeGazeOriginInWorldSpace, combineEyeGazeVectorInWorldSpace);
        return dataReceived;
    }
    void Update()
    {
        _LPose.text = "";
        _RPose.text = "";
        _LOpeness.text = "";
        _ROpeness.text = "";
        _CPose.text = "";
        _CDPose.text = "";

        /*dataValid = PXRTracking();

        if (!dataValid)
            dataValid = PICOEye();
        if (!dataValid)
            dataValid = XRFixationPoint();
        if (!dataValid)
            dataValid = XRCenterEye();
        if (!dataValid)*/

        //dataValid = PICOEye();

        //if(!dataValid)
        //dataValid = 
        //dataValid = 
        XRPerEye();
        /*
        if (!dataValid)
            dataValid = XRCameraCenterHead();
        if (!dataValid)
            dataValid = CameraCenterCutom();
        */

    }


    private bool HandleGazeTarget(LineRenderer lineRenderer, Vector3 origin, Vector3 vector)
    {
        bool selectedObjIsTarget = false;
        lineRenderer.enabled = true;
        lineRenderer.SetPosition(0, origin);
        lineRenderer.SetPosition(1, origin + vector * 100f);
        Ray ray = new Ray(origin, vector);
        if (Physics.SphereCast(origin, 2f, vector, out hitinfo))
        {
            if (selectedObj != null && selectedObj != hitinfo.transform)
            {
                if (selectedObj.tag.Equals("Target") || (selectedObj.GetComponent<ETObject>() != null && selectedObj.GetComponent<ETObject>().IsGazeLocked()))
                    selectedObj.GetComponent<ETObject>().UnFocused();
                selectedObj = null;

            }
            else if (selectedObj == null)
            {
                selectedObj = hitinfo.transform;
                if (selectedObj.tag.Equals("Target") || (selectedObj.GetComponent<ETObject>() != null && !selectedObj.GetComponent<ETObject>().IsGazeLocked()))
                {
                    selectedObj.GetComponent<ETObject>().IsFocused();
                    selectedObjIsTarget = true;
                }
            }
        }
        else
        {
            if (selectedObj != null)
            {
                if (selectedObj.tag.Equals("Target") || (selectedObj.GetComponent<ETObject>() != null && selectedObj.GetComponent<ETObject>().IsGazeLocked()))
                    selectedObj.GetComponent<ETObject>().UnFocused();
                selectedObj = null;
            }
        }
        return selectedObjIsTarget;
    }

}
