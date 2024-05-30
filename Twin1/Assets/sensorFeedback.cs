using UnityEngine;

public class SensorScript : MonoBehaviour
{
    public Transform sensor;
    public Transform parentTransform;
    public float distance = 10.0f;
    public Material pointMaterial;

    void Start()
    {
        CreatePoint();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            CreatePoint();
        }
    }

    public void CreatePoint() // Make this method public
    {
        Vector3 pointPosition = sensor.position + sensor.forward * distance;
        GameObject point = new GameObject("Point");
        point.transform.position = pointPosition;
        point.transform.SetParent(parentTransform, true);

        SphereCollider sphereCollider = point.AddComponent<SphereCollider>();
        MeshRenderer renderer = point.AddComponent<MeshRenderer>();
        MeshFilter filter = point.AddComponent<MeshFilter>();
        filter.mesh = CreateSphereMesh();
        renderer.material = pointMaterial;

        point.transform.localScale = new Vector3(0.005f, 0.005f, 0.005f);
    }

    Mesh CreateSphereMesh()
    {
        GameObject tempSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        Mesh mesh = tempSphere.GetComponent<MeshFilter>().mesh;
        Destroy(tempSphere);
        return mesh;
    }
}
