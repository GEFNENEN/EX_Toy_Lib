using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoRotation : MonoBehaviour
{
    public Vector3 rotationAxis = Vector3.up; // 旋转轴
    
    public int speed = 10; // 旋转速度

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(rotationAxis, speed * Time.deltaTime, Space.Self);
    }
}
