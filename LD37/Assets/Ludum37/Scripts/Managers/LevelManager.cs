using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour {
    private static LevelManager s_instance;
    public static LevelManager Instance { get { return s_instance; } }

    public LevelDataHolder[] m_levelDatas;
    public LevelPalette[] m_levelPalettes;

    public int m_currentLevel = 0;
    public Level m_activeLevel;

    protected void Awake()
    {
        s_instance = this;

        for (int i = 0, n = m_levelPalettes.Length; i < n; ++i)
        {
            m_levelPalettes[i].Init();
        }

        // create first level
        if (m_currentLevel < m_levelDatas.Length)
        {
            m_activeLevel = new Level();
            LevelData data = m_levelDatas[m_currentLevel].m_data;
            m_activeLevel.Init(data, m_levelPalettes[data.m_paletteIndex]);
            m_activeLevel.GenerateMissingCells();
        }
    }

    protected void Update()
    {
        if (Input.GetKeyDown(KeyCode.Comma))
        {
            TransitionToPreviousLevel();
        }

        if (Input.GetKeyDown(KeyCode.Period))
        {
            TransitionToNextLevel();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetCurrentLevel();
        }
    }

    [ContextMenu("Transition to previous level")]
    public void TransitionToPreviousLevel()
    {
        m_currentLevel = m_currentLevel - 1;
        if(m_currentLevel < 0)
        {
            m_currentLevel += m_levelDatas.Length;
        }
        TransitionTo(m_levelDatas[m_currentLevel].m_data);
    }

    [ContextMenu("Transition to next level")]
    public void TransitionToNextLevel()
    {
        m_currentLevel = (m_currentLevel + 1) % m_levelDatas.Length;
        TransitionTo(m_levelDatas[m_currentLevel].m_data);
    }

    [ContextMenu("Reset current level")]
    public void ResetCurrentLevel()
    {
        TransitionTo(m_levelDatas[m_currentLevel].m_data);
    }

    public void TransitionTo(LevelData data)
    {
        StartCoroutine(DoTransition(data));
    }

    protected IEnumerator DoTransition(LevelData data)
    {
        Level newLevel = new Level();
        newLevel.Init(data, m_levelPalettes[data.m_paletteIndex]);

        foreach (var pair in m_activeLevel.m_levelCells)
        {
            LevelCell cell = pair.Value;
            if (cell == null)
            {
                continue;
            }
            
            if (data.IsInBounds(cell.m_data.m_x, cell.m_data.m_y, cell.m_data.m_z))
            {
                LevelCellData newCellData = data.GetCellData(cell.m_data.m_x, cell.m_data.m_y, cell.m_data.m_z);
                if (cell.m_data.m_type != newCellData.m_type)
                {
                    cell.m_isMarkedForDeletion = true;
                    cell.Hide(LevelCell.kTransitionTime * 0.5f * (m_activeLevel.m_data.m_height - cell.m_data.m_y));
                }
                else
                {
                    newLevel.AddCell(cell);
                }
            }
            else
            {
                cell.m_isMarkedForDeletion = true;
                cell.Hide(LevelCell.kTransitionTime * 0.5f * (m_activeLevel.m_data.m_height - cell.m_data.m_y));
            }
        }
        
        m_activeLevel = newLevel;
        m_activeLevel.GenerateMissingCells();
        yield return new WaitForEndOfFrame();
    }
}
