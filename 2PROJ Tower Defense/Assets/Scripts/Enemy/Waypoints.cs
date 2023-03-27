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
}
