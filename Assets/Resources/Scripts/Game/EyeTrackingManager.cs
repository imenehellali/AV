using UnityEngine;
using Unity.XR.PXR;
using UnityEngine.XR;
using TMPro;
using Unity.XR.PICO.TOBSupport;

public class EyeTrackingManager : MonoBehaviour
{

    // start test variable
    // public LineRenderer lineRenderer;
    //end test variables

    public Transform Origin;

    //public Transform Greenpoint;
    //public GameObject SpotLight;

    private Vector3 combineEyeGazeVector;
    private Vector3 combineEyeGazeOriginOffset;
    private Vector3 combineEyeGazeOrigin;
    private Matrix4x4 headPoseMatrix;
    private Matrix4x4 originPoseMatrix;

    private Vector3 combineEyeGazeVectorInWorldSpace;
    private Vector3 combineEyeGazeOriginInWorldSpace;

    private uint leftEyeStatus;
    private uint rightEyeStatus;

    private Vector2 primary2DAxis;
    private Vector2 leftEyePrimary2DAxis;

    private RaycastHit hitinfo;

    private Transform selectedObj;

    private EyeTrackingStartInfo startInfo;
    private EyeTrackingData _eyeData;
    private EyeTrackingDataGetInfo _eyeDataGetInfo;
    private PerEyeData _leftEye;
    private PerEyeData _rightEye;
    private PerEyeData _combinedEye;
    private float timer = 0f;

    [SerializeField]
    private TextMeshProUGUI _LOpeness;
    [SerializeField]
    private TextMeshProUGUI _ROpeness;
    [SerializeField]
    private TextMeshProUGUI _LPose;
    [SerializeField]
    private TextMeshProUGUI _RPose;
    [SerializeField]
    private TextMeshProUGUI _CPose;
    [SerializeField]
    private TextMeshProUGUI _CDPose;

    public static EyeTrackingManager Instance {  get; private set; }    

    private bool eyeTrackingstarted=false;
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
        startInfo = new EyeTrackingStartInfo();
        startInfo.needCalibration = 0; // Set to 1 if calibration is needed
        startInfo.mode = EyeTrackingMode.PXR_ETM_BOTH; // Set the eye tracking mode

        _eyeDataGetInfo = new EyeTrackingDataGetInfo();
        _eyeDataGetInfo.displayTime = 0; // Set to 0 if you want the most recent data
        _eyeDataGetInfo.flags = EyeTrackingDataGetFlags.PXR_EYE_DEFAULT
                        | EyeTrackingDataGetFlags.PXR_EYE_POSITION
                        | EyeTrackingDataGetFlags.PXR_EYE_ORIENTATION;

    }
    void Start()
    {

        combineEyeGazeOriginOffset = Vector3.zero;
        combineEyeGazeVector = Vector3.zero;
        combineEyeGazeOrigin = Vector3.zero;
        originPoseMatrix = Origin.localToWorldMatrix;

        //start test variables
        /*
        lineRenderer.startWidth = 0.01f;
        lineRenderer.endWidth = 0.01f;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = Color.red;
        lineRenderer.endColor = Color.red;
        */
        //end test variables
        eyeTrackingstarted= PXR_MotionTracking.StartEyeTracking(ref startInfo)==0?true:false;
    }

    void Update()
    {
        /*
        //Offest Adjustment
        if (
            InputDevices.GetDeviceAtXRNode(XRNode.RightEye).TryGetFeatureValue(CommonUsages.primary2DAxis, out primary2DAxis) &&
            InputDevices.GetDeviceAtXRNode(XRNode.LeftEye).TryGetFeatureValue(CommonUsages.primary2DAxis, out leftEyePrimary2DAxis))
        {
            combineEyeGazeOriginOffset.x += (primary2DAxis.x + leftEyePrimary2DAxis.x) * 0.0005f;
            combineEyeGazeOriginOffset.y += (primary2DAxis.y + leftEyePrimary2DAxis.y) * 0.0005f;
        }



        PXR_EyeTracking.GetHeadPosMatrix(out headPoseMatrix);
        PXR_EyeTracking.GetCombineEyeGazeVector(out combineEyeGazeVector);
        PXR_EyeTracking.GetCombineEyeGazePoint(out combineEyeGazeOrigin);

        //Translate Eye Gaze point and vector to world space
        combineEyeGazeOrigin += combineEyeGazeOriginOffset;
        //combineEyeGazeOrigin.y += 1;
        combineEyeGazeOriginInWorldSpace = originPoseMatrix.MultiplyPoint(headPoseMatrix.MultiplyPoint(combineEyeGazeOrigin));
        combineEyeGazeVectorInWorldSpace = originPoseMatrix.MultiplyVector(headPoseMatrix.MultiplyVector(combineEyeGazeVector));

        */
        /*
        SpotLight.transform.position = combineEyeGazeOriginInWorldSpace;
        SpotLight.transform.rotation = Quaternion.LookRotation(combineEyeGazeVectorInWorldSpace, Vector3.up);
        */
       // GazeTargetControl(combineEyeGazeOriginInWorldSpace, combineEyeGazeVectorInWorldSpace);

        if (eyeTrackingstarted)
        {
            int success = PXR_MotionTracking.GetEyeTrackingData(ref _eyeDataGetInfo, ref _eyeData);

            if (success == 0)
            {
                timer += Time.deltaTime;

                _leftEye = _eyeData.eyeDatas[0];
                _rightEye = _eyeData.eyeDatas[1];
                _combinedEye = _eyeData.eyeDatas[2];

                _LOpeness.text = $"Openess Left : {_leftEye.openness.ToString()}  during {timer}";
                _ROpeness.text = $"Openess Right : {_rightEye.openness.ToString()} lengthTable : {_eyeData.eyeDatas.Length}";

                _LPose.text = $"Orientation Left  Z : {_leftEye.pose.orientation.z.ToString()} is valid {_leftEye.isPoseValid}";
                _RPose.text = $"Orientation Right Z : {_rightEye.pose.orientation.z.ToString()} is valid {_rightEye.isPoseValid}";
                _CPose.text = $"Orientation both from custom Z : {combineEyeGazeVectorInWorldSpace}";
                _CDPose.text = $"Orientation both  Z : {_combinedEye.pose.orientation.z}";
            }
        }
    }


    void GazeTargetControl(Vector3 origin, Vector3 vector)
    {
        Ray ray = new Ray(origin, vector);

        // Draw the line during runtime
        //lineRenderer.SetPosition(0, origin);

        //Physics.SphereCast(origin,0.005f,vector,out hitinfo)
        if (Physics.Raycast(origin, vector, out hitinfo, float.MaxValue))
        {

            //lineRenderer.SetPosition(1, hitinfo.point);
            //selectedObj = hitinfo.collider.transform;

            /*if (hitinfo.collider.transform.tag.Equals("Target"))
            {
                hitinfo.collider.transform.gameObject.GetComponent<ETObject>().IsFocused();
            }
            */
            if (selectedObj != null && selectedObj != hitinfo.transform)
            {
                if (selectedObj.GetComponent<ETObject>() != null)
                    selectedObj.GetComponent<ETObject>().UnFocused();
                selectedObj = null;
            }
            else
            {
                selectedObj = hitinfo.transform;
                if (selectedObj.GetComponent<ETObject>() != null)
                    selectedObj.GetComponent<ETObject>().IsFocused();
            }

        }
        else
        {
            // lineRenderer.SetPosition(1, origin + vector* 1000f);
            if (selectedObj != null)
            {
                if (selectedObj.GetComponent<ETObject>() != null)
                    selectedObj.GetComponent<ETObject>().UnFocused();
                selectedObj = null;
            }
            //Greenpoint.gameObject.SetActive(false);
        }
    }
}
