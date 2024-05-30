using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Potentiometer : MonoBehaviour
{
    [SerializeField] private float minInputValue = 0f;
    [SerializeField] private float maxInputValue = 1023f;
    [SerializeField] private float minRotationDegree = 0f;
    [SerializeField] private float maxRotationDegree = 270f;

    private float initialRotationX;
    private float initialRotationY;
    private float initialRotationZ;

    [SerializeField] private float testValue = 0f;
    [SerializeField] private bool useTestValue = false;

    private void Awake()
    {
        initialRotationX = transform.localEulerAngles.x;
        initialRotationY = transform.localEulerAngles.y;
        initialRotationZ = transform.localEulerAngles.z;
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        RotateObject(UDPManager.Instance.AngleA);
    }

    private void RotateObject(float value)
    {
        int mappedRotation = (int) MapValue(value, minInputValue, maxInputValue, minRotationDegree, maxRotationDegree);

        Vector3 rotationVector = new Vector3(initialRotationX, mappedRotation, initialRotationZ);

        transform.localRotation = Quaternion.Euler(rotationVector);
    }

    private float MapValue(float value, float fromMin, float fromMax, float toMin, float toMax)
    {
        return Mathf.Lerp(toMin, toMax, Mathf.InverseLerp(fromMin, fromMax, value));
    }
}
