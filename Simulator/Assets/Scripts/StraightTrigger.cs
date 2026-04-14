using UnityEngine;

public class StraightArrowTrigger : ArrowTriggerBase
{
    protected override void SpawnArrows()
    {
        SpawnArrowLine(transform.position, transform.forward, 1, 2f);
    }
}
