using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : BaseActor {
    public Indicator m_wayPointIndicator;
    public Color m_pathFindingProcessingColor;
    public Color m_pathFindingSuccessColor;
    public Color m_pathFindingFailColor;
    
    public override void Stop()
    {
        base.Stop();
        if(m_wayPointIndicator != null)
        {
            m_wayPointIndicator.Hide();
        }
    }

    public override void OnPathDestinationReached()
    {
        base.OnPathDestinationReached();

        if (m_wayPointIndicator != null)
        {
            m_wayPointIndicator.Hide();
        }

        LevelCell cell = GetCell(m_desiredPosition);
        if(cell != null && cell.m_data.m_type == LevelCellType.Goal)
        {
            cell.Hide(m_moveTimer*0.25f);
            LeanTween.delayedCall(m_moveTimer + 0.1f, LevelManager.Instance.TransitionToNextLevel);
        }
    }

    protected override void OnPathFindingStarted(Vector3 destWorldPos)
    {
        base.OnPathFindingStarted(destWorldPos);
        if (m_wayPointIndicator != null)
        {
            m_wayPointIndicator.transform.position = GetCellWorldPosition(destWorldPos);
            m_wayPointIndicator.Show();
            m_wayPointIndicator.SetColor(m_pathFindingProcessingColor);
        }
    }

    protected override void OnPathFindingSuccess()
    {
        base.OnPathFindingSuccess();
        if (m_wayPointIndicator != null)
        {
            m_wayPointIndicator.SetColor(m_pathFindingSuccessColor);
        }
    }

    protected override void OnPathFindingFailed()
    {
        base.OnPathFindingFailed();
        if (m_wayPointIndicator != null)
        {
            m_wayPointIndicator.Hide(0.5f);
            m_wayPointIndicator.SetColor(m_pathFindingFailColor);
        }
    }
}
