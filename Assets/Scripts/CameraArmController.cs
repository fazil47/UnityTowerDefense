using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraArmController : MonoBehaviour
{
    [SerializeField] private float rotateSpeed = 1f;

    void Update()
    {
        transform.RotateAround(transform.position, Vector3.up, Input.mouseScrollDelta.y * rotateSpeed * Time.deltaTime);
    }
}