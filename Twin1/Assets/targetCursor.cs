using UnityEngine;

public class FollowCursor : MonoBehaviour
{
    public Transform referenceObject; // Reference object to compare distance
    public SpriteRenderer colliderSprite; // Collider sprite to show/hide
    private Camera mainCamera;
    private bool isDragging;

    void Start()
    {
        mainCamera = Camera.main;
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
    }
}