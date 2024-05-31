using UnityEngine;

public class FollowCursor : MonoBehaviour
{
    public Transform referenceObject; // Reference object to compare distance
    public SpriteRenderer colliderSprite; // Collider sprite to show/hide
    public Transform jointA;
    public Transform jointB;
    public Transform jointC;

    private Camera mainCamera;
    private bool isDragging;
    private float sendInterval = 0.1f; // Interval in seconds
    private float timeSinceLastSend;

    void Start()
    {
        mainCamera = Camera.main;
        timeSinceLastSend = 0f;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            isDragging = true;
            Cursor.visible = false; // Hide the cursor
        }
        else if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
            Cursor.visible = true; // Show the cursor
        }

        if (isDragging)
        {
            Vector3 mousePosition = Input.mousePosition;
            Ray ray = mainCamera.ScreenPointToRay(mousePosition);
            Plane plane = new Plane(Vector3.forward, new Vector3(0, 0, transform.position.z)); // Assuming the object is on a plane with constant z

            if (plane.Raycast(ray, out float distance))
            {
                Vector3 worldPosition = ray.GetPoint(distance);
                Vector3 newPosition = new Vector3(worldPosition.x, worldPosition.y, transform.position.z); // Maintain the object's original z position

                float maxDistance = 1f;
                bool validMove = true;

                if (newPosition.x < -1)
                {
                    colliderSprite.enabled = true;
                    validMove = false;
                    newPosition.x = -1;
                }
                else
                {
                    colliderSprite.enabled = false;
                }

                if (newPosition.y < 3)
                {
                    validMove = false;
                    newPosition.y = 3;
                }

                if (validMove && Vector3.Distance(newPosition, referenceObject.position) <= maxDistance)
                {
                    transform.position = newPosition;
                }
                else
                {
                    Vector3 direction = (newPosition - referenceObject.position).normalized;
                    Vector3 closestValidPosition = referenceObject.position + direction * maxDistance;

                    if (closestValidPosition.x < -1)
                    {
                        closestValidPosition.x = -1;
                    }

                    if (closestValidPosition.y < 3)
                    {
                        closestValidPosition.y = 3;
                    }

                    Vector3 screenPoint = mainCamera.WorldToScreenPoint(closestValidPosition);
                    Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
                    transform.position = closestValidPosition;
                }
            }
        }

        // Update the timer
        timeSinceLastSend += Time.deltaTime;

        // Send the angles of the joints if enough time has passed
        if (timeSinceLastSend >= sendInterval)
        {
            SendJointAngles();
            timeSinceLastSend = 0f;
        }
    }

    private void SendJointAngles()
    {
        float angleA = MapArmA(jointA.localEulerAngles.z);
        float angleB = MapArmB(jointB.localEulerAngles.z);
        float angleC = MapArmC(jointC.localEulerAngles.z);

        string message = $"Servo|A:{Mathf.RoundToInt(angleA)}|B:{Mathf.RoundToInt(angleB)}|C:{Mathf.RoundToInt(angleC)}|D:";
        UDPManager.Instance.SendUDPMessage(message);
    }

    private float NormalizeAngle(float angle)
    {
        angle = angle % 360;

        if (angle > 180)
        {
            angle -= 360;
        }
        else if (angle < -180)
        {
            angle += 360;
        }
        return angle;
    }

    private float MapArmB(float unityAngle)
    {
        // Normalize and map from 150 to 0 in Unity to 0 to 150 in ESP32
        float normalizedAngle = NormalizeAngle(unityAngle);
        return Mathf.Clamp(150 - normalizedAngle, 0, 150);
    }

    private float MapArmA(float unityAngle)
    {
        // Normalize the angle to ensure it is within -180 to 180 degrees
        float normalizedAngle = NormalizeAngle(unityAngle);

        // Map from 0 to 90 in Unity to 90 to 0 in ESP32
        if (normalizedAngle >= -90 && normalizedAngle <= 90)
        {
            return 90 + normalizedAngle; // Reverse the direction
        }
        else
        {
            // Handle the case where the angle might be out of expected bounds
            return Mathf.Clamp(90 + normalizedAngle, 0, 180);
        }
    }

    private float MapArmC(float unityAngle)
    {
        // Normalize the angle to ensure it is within -180 to 180 degrees
        float normalizedAngle = NormalizeAngle(unityAngle);

        // Map from 0 to 90 in Unity to 90 to 0 in ESP32
        if (normalizedAngle >= -90 && normalizedAngle <= 90)
        {
            return 90 + normalizedAngle; // Reverse the direction
        }
        else
        {
            // Handle the case where the angle might be out of expected bounds
            return Mathf.Clamp(90 + normalizedAngle, 0, 180);
        }
    }


}
