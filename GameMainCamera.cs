﻿using UnityEngine;

public class GameMainCamera : MonoBehaviour
{
    Vector3 _target = Vector3.zero;

    void Update()
    {
        const float rotateSpeed = 4.0f;
        const float panSpeed = 0.4f;

        bool pan = Input.GetAxis("Pan") == 1.0f;
        bool rotate = Input.GetAxis("Rotate") == 1.0f;

        if (pan)
        {
            float right = -Input.GetAxis("Mouse X") * panSpeed;
            float up = -Input.GetAxis("Mouse Y") * panSpeed;

            var vector = new Vector3(right, up, 0);
            transform.position += transform.rotation * vector;
        }
        else if (rotate)
        {
            float yaw = Input.GetAxis("Mouse X") * rotateSpeed;
            float pitch = -Input.GetAxis("Mouse Y") * rotateSpeed;

            transform.RotateAround(_target, Vector3.up, yaw);
            transform.RotateAround(_target, transform.rotation * Vector3.right, pitch);
        }

        float zoom = Input.GetAxis("Mouse ScrollWheel");
        if (zoom != 0)
        {
            float distance = (_target - transform.position).magnitude * zoom;
            transform.Translate(Vector3.forward * distance, Space.Self);
        }

    }
}
