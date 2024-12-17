using UnityEngine;
using Unity.XR.PXR;
using UnityEngine.XR;
using TMPro;

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

    private bool wasPressed;
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
    }

    void Update()
    {
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

        /*
        SpotLight.transform.position = combineEyeGazeOriginInWorldSpace;
        SpotLight.transform.rotation = Quaternion.LookRotation(combineEyeGazeVectorInWorldSpace, Vector3.up);
        */
        GazeTargetControl(combineEyeGazeOriginInWorldSpace, combineEyeGazeVectorInWorldSpace);
        
    }


    void GazeTargetControl(Vector3 origin,Vector3 vector)
    {
        Ray ray = new Ray(origin,vector);

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
                if(selectedObj.GetComponent<ETObject>()!=null)
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
