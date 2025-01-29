using Unity.XR.CoreUtils;
using UnityEngine.AI;
using UnityEngine.Assertions;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Movement;

namespace UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets
{
    /// <summary>
    /// A version of continuous movement that automatically controls the frame of reference that
    /// determines the forward direction of movement based on user preference for each hand.
    /// For example, can configure to use head relative movement for the left hand and controller relative movement for the right hand.
    /// </summary>
    public class DynamicMoveProvider : ContinuousMoveProvider
    {
        /// <summary>
        /// Defines which transform the XR Origin's movement direction is relative to.
        /// </summary>
        /// <seealso cref="leftHandMovementDirection"/>
        /// <seealso cref="rightHandMovementDirection"/>
        public enum MovementDirection
        {
            /// <summary>
            /// Use the forward direction of the head (camera) as the forward direction of the XR Origin's movement.
            /// </summary>
            HeadRelative,

            /// <summary>
            /// Use the forward direction of the hand (controller) as the forward direction of the XR Origin's movement.
            /// </summary>
            HandRelative,
        }

        [Space, Header("Movement Direction")]
        [SerializeField]
        [Tooltip("Directs the XR Origin's movement when using the head-relative mode. If not set, will automatically find and use the XR Origin Camera.")]
        Transform m_HeadTransform;

        /// <summary>
        /// Directs the XR Origin's movement when using the head-relative mode. If not set, will automatically find and use the XR Origin Camera.
        /// </summary>
        public Transform headTransform
        {
            get => m_HeadTransform;
            set => m_HeadTransform = value;
        }

        [SerializeField]
        [Tooltip("Directs the XR Origin's movement when using the hand-relative mode with the left hand.")]
        Transform m_LeftControllerTransform;

        /// <summary>
        /// Directs the XR Origin's movement when using the hand-relative mode with the left hand.
        /// </summary>
        public Transform leftControllerTransform
        {
            get => m_LeftControllerTransform;
            set => m_LeftControllerTransform = value;
        }

        [SerializeField]
        [Tooltip("Directs the XR Origin's movement when using the hand-relative mode with the right hand.")]
        Transform m_RightControllerTransform;

        public Transform rightControllerTransform
        {
            get => m_RightControllerTransform;
            set => m_RightControllerTransform = value;
        }

        [SerializeField]
        [Tooltip("Whether to use the specified head transform or left controller transform to direct the XR Origin's movement for the left hand.")]
        MovementDirection m_LeftHandMovementDirection;

        /// <summary>
        /// Whether to use the specified head transform or controller transform to direct the XR Origin's movement for the left hand.
        /// </summary>
        /// <seealso cref="MovementDirection"/>
        public MovementDirection leftHandMovementDirection
        {
            get => m_LeftHandMovementDirection;
            set => m_LeftHandMovementDirection = value;
        }

        [SerializeField]
        [Tooltip("Whether to use the specified head transform or right controller transform to direct the XR Origin's movement for the right hand.")]
        MovementDirection m_RightHandMovementDirection;

        /// <summary>
        /// Whether to use the specified head transform or controller transform to direct the XR Origin's movement for the right hand.
        /// </summary>
        /// <seealso cref="MovementDirection"/>
        public MovementDirection rightHandMovementDirection
        {
            get => m_RightHandMovementDirection;
            set => m_RightHandMovementDirection = value;
        }

        Transform m_CombinedTransform;
        Pose m_LeftMovementPose = Pose.identity;
        Pose m_RightMovementPose = Pose.identity;

        /// <inheritdoc />
        protected override void Awake()
        {
            base.Awake();

            m_CombinedTransform = new GameObject("[Dynamic Move Provider] Combined Forward Source").transform;
            m_CombinedTransform.SetParent(transform, false);
            m_CombinedTransform.localPosition = Vector3.zero;
            m_CombinedTransform.localRotation = Quaternion.identity;

            forwardSource = m_CombinedTransform;
        }

        /// <inheritdoc />

        protected override Vector3 ComputeDesiredMove(Vector2 input)
        {
            // Return zero movement if there is no input
            if (input == Vector2.zero)
                return Vector3.zero;

            // Ensure the head transform is initialized
            if (m_HeadTransform == null)
            {
                var xrOrigin = mediator.xrOrigin;
                if (xrOrigin != null)
                {
                    var xrCamera = xrOrigin.Camera;
                    if (xrCamera != null)
                        m_HeadTransform = xrCamera.transform;
                }
            }

            // Determine the forward source based on the left controller
            switch (m_LeftHandMovementDirection)
            {
                case MovementDirection.HeadRelative:
                    if (m_HeadTransform != null)
                        m_LeftMovementPose = m_HeadTransform.GetWorldPose();
                    break;

                case MovementDirection.HandRelative:
                    if (m_LeftControllerTransform != null)
                        m_LeftMovementPose = m_LeftControllerTransform.GetWorldPose();
                    break;

                default:
                    Assert.IsTrue(false, $"Unhandled {nameof(MovementDirection)}={m_LeftHandMovementDirection}");
                    break;
            }

            // Smoothly update position and rotation using Lerp and Slerp
            Vector3 targetPosition = m_LeftMovementPose.position;
            Quaternion targetRotation = m_LeftMovementPose.rotation;

            m_CombinedTransform.position = Vector3.Lerp(m_CombinedTransform.position, targetPosition, 0.5f);
            m_CombinedTransform.rotation = Quaternion.Slerp(m_CombinedTransform.rotation, targetRotation, 0.5f);

            // Calculate movement based on the left controller's forward direction
            Vector3 forwardDirection = m_CombinedTransform.forward;
            Vector3 rightDirection = m_CombinedTransform.right;
            Vector3 desiredMove = forwardDirection * input.y + rightDirection * input.x;
            desiredMove *= moveSpeed * Time.deltaTime;

            // Constrain movement to NavMesh
            Vector3 constrainedPosition = transform.position + desiredMove;
            NavMeshHit hit;

            if (NavMesh.SamplePosition(constrainedPosition, out hit, 1.0f, NavMesh.AllAreas))
            {
                desiredMove = (hit.position - transform.position).normalized * desiredMove.magnitude;
            }
            else
            {
                desiredMove = Vector3.zero; // Stop movement if outside NavMesh
            }

            return desiredMove;
        }
    }
}
