using UnityEngine;
using UnityEngine.Events;

public abstract class PathFollower : MonoBehaviour
{
    // Components
    protected Rigidbody rb = null;
    [HideInInspector] public UnityEvent onWaypointReach = null;

    // Movement
    [Header("Movement")]
    [SerializeField] protected float speed = 100f;
    [SerializeField] protected float rotationSpeed = 30f;

    // Circuit
    [Header("Circuit")] [Space]
    public Transform[] waypoints = null;
    [SerializeField] protected float waypointPrecision = 0.5f;
    protected int currentIndex = 0;
    protected Transform CurrentWaypoint
    {
        get 
        { 
            if (waypoints != null && 0 <= currentIndex && currentIndex < waypoints.Length) 
                return waypoints[currentIndex];
            return null;
        }
    }
    protected virtual Transform NextWaypoint()
    {
        if (currentIndex + 1 >= waypoints.Length)
            currentIndex = 0;
        else
            currentIndex++;

        return CurrentWaypoint;
    }

    [HideInInspector] public bool followingPath = true;


    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (onWaypointReach == null)
            onWaypointReach = new UnityEvent();
    }

    protected virtual void Start()
    {
        if(CurrentWaypoint != null)
        {
            transform.SetPositionAndRotation(CurrentWaypoint.position, CurrentWaypoint.rotation);
            NextWaypoint();
        }
    }

    protected virtual void FixedUpdate()
    {
        if (followingPath)
        {
            // If so, then we reached the destination.
            if (Vector3.Distance(rb.position, CurrentWaypoint.position) <= waypointPrecision)
            {
                onWaypointReach?.Invoke();
                NextWaypoint();
            }

            Vector3 targetDir = CurrentWaypoint.position - rb.position;
            rb.MoveRotation(Quaternion.LookRotation(Vector3.RotateTowards(transform.forward, targetDir, rotationSpeed * Time.fixedDeltaTime, 0f)));

            Vector3 desiredVelocity = speed * transform.forward;
            rb.AddForce(desiredVelocity - rb.velocity, ForceMode.VelocityChange);
        }
    }
}
