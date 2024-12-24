using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HummingBirdHandler : MonoBehaviour
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
        // Calculate normalized speed factor
        float speedFactor = speed / Vector3.Distance(A.position,B.position) * Time.deltaTime;

        // Increment or decrement t based on direction
        t += movingToB ? speedFactor : -speedFactor;

        if (t >= 1.0f || t <= 0.0f)
        {
            movingToB = !movingToB;
            t = Mathf.Clamp01(t);
        }

        Vector3 newPosition = Vector3.Lerp(A.position, B.position, t);
        transform.position = newPosition;

        Vector3 direction = (movingToB ? B.position : A.position) - transform.position;
        transform.forward = direction.normalized;
    }

}
