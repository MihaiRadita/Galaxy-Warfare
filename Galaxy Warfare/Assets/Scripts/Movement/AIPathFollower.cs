using UnityEngine;

public class AIPathFollower : PathFollower
{
    private AIShipController controller = null;
    [HideInInspector] public int stopOnIndex = -1;
    [HideInInspector] public PlayerShipController playerShip = null;
    [HideInInspector] public Transform waveTransform = null;
    private bool shouldLookAtPlayer = false;

    protected override void Awake()
    {
        base.Awake();
        controller = GetComponent<AIShipController>();
        onWaypointReach.AddListener(Foo);
    }

    protected override void Start()
    {
        //base.Start();
    }

    private void Foo()
    {
        if(currentIndex == stopOnIndex)
        {
            followingPath = false;
            currentIndex = 0;
            for(int i = 0; i < waypoints.Length; i++)
            {
                Destroy(waypoints[i].gameObject);
            }
            rb.isKinematic = true;
            shouldLookAtPlayer = true;
            transform.parent = waveTransform;
            controller.animator.SetBool("Idle", true);
        }
    }

    protected override void FixedUpdate()
    {
        if(CurrentWaypoint != null)
        {
            base.FixedUpdate();
        }
        if(shouldLookAtPlayer)
        {
            controller.shipVisuals.LookAt(playerShip.shipVisuals);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        if(CurrentWaypoint != null)
        {
            Gizmos.DrawSphere(CurrentWaypoint.position, 1f);
        }
    }

    private void OnDestroy()
    {
        onWaypointReach.RemoveAllListeners();
    }
}
