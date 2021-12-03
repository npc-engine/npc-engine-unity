using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LerpRotate : MonoBehaviour
{
    public Vector3 axis;
    public Vector3 forward;
    public Transform target;
    public float speed = 1;

    public float threshold = 0.1f;

    private bool rotate = false;

    private void Start()
    {
        target = Camera.main.transform;
    }

    public void StartRotate()
    {
        rotate = true;
    }

    public void StopRotate()
    {
        rotate = false;
    }


    // Update is called once per frame
    void Update()
    {
        if (rotate)
        {

            Vector3 currentVector = transform.TransformVector(forward);
            var targetVector = target.position - transform.position;
            // Rotation between current vector and target vector around axis
            var projectedCurrentVector = Vector3.ProjectOnPlane(currentVector, axis);
            var projectedTargetVector = Vector3.ProjectOnPlane(targetVector, axis);
            float fromToAngle = Vector3.SignedAngle(projectedCurrentVector, projectedTargetVector, axis);
            if (fromToAngle > threshold)
            {
                transform.RotateAround(transform.position, axis, fromToAngle * speed * Time.deltaTime);
            }
            else if (fromToAngle < -threshold)
            {
                transform.RotateAround(transform.position, axis, fromToAngle * speed * Time.deltaTime);
            }
        }
    }
}
