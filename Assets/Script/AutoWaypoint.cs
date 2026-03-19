using UnityEngine;

public class AutoWaypoint : MonoBehaviour
{
    public Transform startPosition;
    public Transform endPosition;
    public int waypointNumber = 10;

    void Start()
    {

    }

    [ContextMenu("CreateWaypoints")]
    public void CreateWaypoints()
    {
        if (startPosition == null || endPosition == null)
        {
            Debug.Log(" Please assign StartPosition and EndPosition before generating waypoints.");
            return;
        }

        GameObject waypointRoot = new GameObject("Waypoints");

        for (int i = 0; i < waypointNumber; i++)
        {
            float percent = (float)i / (waypointNumber - 1);
            Vector3 pointPos = Vector3.Lerp(startPosition.position, endPosition.position, percent);

            GameObject newPoint = new GameObject("WP_" + (i + 1).ToString("00"));
            newPoint.transform.position = pointPos;
            newPoint.transform.parent = waypointRoot.transform;
        }

        Debug.Log("Waypoints successfully generated!");
    }
}