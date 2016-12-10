using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour {
    private static LevelManager s_instance;
    public static LevelManager Instance { get { return s_instance; } }

    public LevelDataHolder[] m_levelDatas;
    public LevelPalette[] m_levelPalettes;

    public int currentLevel = 0;
    public Level m_activeLevel;

    protected void Awake()
    {
        s_instance = this;

        for (int i = 0, n = m_levelPalettes.Length; i < n; ++i)
        {
            m_levelPalettes[i].Init();
        }

        // create first level
        if (currentLevel < m_levelDatas.Length)
        {
            m_activeLevel = new Level();
            LevelData data = m_levelDatas[currentLevel].m_data;
            m_activeLevel.Init(data, m_levelPalettes[data.m_paletteIndex]);
            m_activeLevel.GenerateMissingCells();
        }
    }

    protected void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            TransitionToNexLevel();
        }
    }

    [ContextMenu("Transition to next level")]
    public void TransitionToNexLevel()
    {
        currentLevel = (currentLevel + 1) % m_levelDatas.Length;
        TransitionTo(m_levelDatas[currentLevel].m_data);
    }

    public void TransitionTo(LevelData data)
    {
        StartCoroutine(DoTransition(data));
    }

    protected IEnumerator DoTransition(LevelData data)
    {
        int minWidth = Mathf.Min(m_activeLevel.m_data.m_width, data.m_width);
        int minHeight = Mathf.Min(m_activeLevel.m_data.m_height, data.m_height);
        int minDepth = Mathf.Min(m_activeLevel.m_data.m_depth, data.m_depth);

        List<LevelCell> removeList = new List<LevelCell>();

        for (int z = 0; z < minDepth; ++z)
        {
            //for (int y = minHeight-1; y >= 0; --y) // reverse direction so that top cells get removed first
            for (int y = 0; y < minHeight; ++y)
            {
                for (int x = 0; x < minWidth; ++x)
                {
                    LevelCell cell = m_activeLevel.GetCell(x, y, z);
                    LevelData.LevelCellData newCellData = data.GetCellData(x, y, z);
                    if(cell != null && cell.m_type != newCellData.m_type)
                    {
                        removeList.Add(cell);
                    }
                }
            }
        }
        for(int i = 0, n = removeList.Count; i < n; ++i)
        {
            LevelCell cellToRemove = removeList[i];
            cellToRemove.m_isMarkedForDeletion = true;
            cellToRemove.Hide(LevelCell.kTransitionTime*0.5f * (minHeight-cellToRemove.m_y));
        }

        Level newLevel = new Level();
        newLevel.Init(data, m_levelPalettes[data.m_paletteIndex]);
        newLevel.MapCells(m_activeLevel.m_levelCells);
        
        m_activeLevel = newLevel;
        m_activeLevel.GenerateMissingCells();
        yield return new WaitForEndOfFrame();
    }
}
