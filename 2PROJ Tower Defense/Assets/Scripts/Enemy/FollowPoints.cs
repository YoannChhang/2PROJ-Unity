public class MoveAlongPath : MonoBehaviour
{
    public Waypoints path;
    public float speed = 5f;
    public float minDistance = 0.1f;

    private int currentWaypoint = 0;

    private void Update()
    {
        if (currentWaypoint < path.waypoints.Length)
        {
            Vector3 targetPosition = path.waypoints[currentWaypoint];

            if (Vector3.Distance(transform.position, targetPosition) < minDistance)
            {
                currentWaypoint++;
            }
            else
            {
                transform.position = Vector3.Lerp(transform.position, targetPosition, speed * Time.deltaTime);
            }
        }
        else
        {
            // If we have reached the last waypoint, we can destroy the game object or disable it
            gameObject.SetActive(false);
        }
    }
}
