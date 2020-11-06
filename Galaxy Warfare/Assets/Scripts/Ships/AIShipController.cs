using UnityEngine;

public class AIShipController : ShipController
{
    private ScoreBoard scoreBoard = null;
    public Animator animator = null;

    private void Start()
    {
        scoreBoard = FindObjectOfType<ScoreBoard>();
    }

    protected override void OnDeath()
    {
        base.OnDeath();
        scoreBoard.ScoreHit();
    }
}
