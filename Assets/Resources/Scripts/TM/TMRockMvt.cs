
using UnityEngine;

public class TMRockMvt : MonoBehaviour
{
    [SerializeField]
    private float speed = 2.0f; 

    [SerializeField]
    private Transform A; 
    [SerializeField]
    private Transform B; 

    private float t = 0.0f; 
    private bool movingToB = true; 

    private void Update()
    {
        t += (movingToB ? Time.deltaTime : -Time.deltaTime) * speed / Vector3.Distance(A.position, B.position);

        // Toggle direction and reset t within bounds
        if (t > 1.0f)
        {
            t = 1.0f;
            movingToB = false;
        }
        else if (t < 0.0f)
        {
            t = 0.0f;
            movingToB = true;
        }

        transform.position = Vector3.Lerp(A.position, B.position, t);
    }
}
