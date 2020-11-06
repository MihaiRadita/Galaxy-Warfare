using TMPro;
using UnityEngine;
using UnityStandardAssets.Utility;

[RequireComponent(typeof(Rigidbody))]
public class PlayerPathFollower : PathFollower
{
    // Movement
    [Header("Player")] [Space]
    [SerializeField] private TextMeshProUGUI speedometer = null;
    [SerializeField] private WaypointCircuit circuit = null;

    protected override void Awake()
    {
        base.Awake();
        waypoints = circuit.waypointList.items;
    }

    private void LateUpdate()
    {
        speedometer.text = $"{Mathf.RoundToInt(rb.velocity.magnitude * 3.6f)} KPH";
    }

    private void OnDeath()
    {
        followingPath = false;
        rb.velocity = Vector3.zero;
    }
}
