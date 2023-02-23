using UnityEngine;

public class Waypoints : MonoBehaviour
{
    public Vector3[] waypoints;

    private int selectedWaypoint = -1;
    private float waypointHandleSize = 0.5f;
    private Color waypointHandleColor = Color.green;

    private void OnDrawGizmos()
    {
        Gizmos.color = waypointHandleColor;

        for (int i = 0; i < waypoints.Length; i++)
        {
            if (i == selectedWaypoint)
            {
                Gizmos.color = Color.yellow;
            }
            else
            {
                Gizmos.color = waypointHandleColor;
            }

            Gizmos.DrawWireSphere(waypoints[i], waypointHandleSize);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;

        for (int i = 0; i < waypoints.Length; i++)
        {
            Gizmos.DrawSphere(waypoints[i], waypointHandleSize);
        }
    }

    private void OnMouseDown()
    {
        Vector3 mousePosition = Camera.current.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0f;

        for (int i = 0; i < waypoints.Length; i++)
        {
            if (Vector3.Distance(mousePosition, waypoints[i]) < waypointHandleSize)
            {
                selectedWaypoint = i;
                break;
            }
        }
    }

    private void OnMouseDrag()
    {
        if (selectedWaypoint != -1)
        {
            Vector3 mousePosition = Camera.current.ScreenToWorldPoint(Input.mousePosition);
            mousePosition.z = 0f;

            waypoints[selectedWaypoint] = mousePosition;
        }
    }

    private void OnMouseUp()
    {
        selectedWaypoint = -1;
    }
}
