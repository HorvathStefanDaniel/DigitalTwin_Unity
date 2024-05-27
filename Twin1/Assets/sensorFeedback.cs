using UnityEngine;

public class SensorScript : MonoBehaviour
{
    public Transform sensor;  // Assign the sensor transform in the Inspector
    public Transform parentTransform;  // Assign the parent transform in the Inspector
    public float distance = 10.0f;

    void Start()
    {
        CreatePoint();
    }

    void CreatePoint()
    {
        Vector3 pointPosition = sensor.position + sensor.forward * distance;
        GameObject point = new GameObject("Point");
        point.transform.position = pointPosition;
        point.transform.SetParent(parentTransform, true);
    }
}
