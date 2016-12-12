using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : BaseActor {
    public Transform m_wayPointIndicator;

    public override void OnPathDestinationReached()
    {
        base.OnPathDestinationReached();

        Debug.Log("path destination reached");
        LevelCell cell = GetCell(transform.position);
        if(cell != null && cell.m_data.m_type == LevelCellType.Goal)
        {
            LevelManager.Instance.TransitionToNextLevel();
        }
    }
}
