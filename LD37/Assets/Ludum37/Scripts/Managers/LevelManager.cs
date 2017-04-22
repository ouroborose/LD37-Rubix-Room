using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour {
    private static LevelManager s_instance;
    public static LevelManager Instance { get { return s_instance; } }

    public LevelDataHolder[] m_levelDatas;
    public LevelPalette[] m_levelPalettes;

    public int m_currentLevelIndex = 0;
    public Level m_activeLevel;
    public bool m_isTransitioning { get; private set; }

    protected void Awake()
    {
        s_instance = this;

        m_isTransitioning = false;

        for (int i = 0, n = m_levelPalettes.Length; i < n; ++i)
        {
            m_levelPalettes[i].Init();
        }

        // create first level
        if (m_currentLevelIndex < m_levelDatas.Length)
        {
            TransitionTo(m_levelDatas[m_currentLevelIndex].m_data);
        }
    }

    protected void Update()
    {
        if(Application.isEditor)
        {
            if (Input.GetKeyDown(KeyCode.Comma))
            {
                TransitionToPreviousLevel();
            }

            if (Input.GetKeyDown(KeyCode.Period))
            {
                TransitionToNextLevel();
            }
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetCurrentLevel();
        }
    }

    public void MovePlayerToStart()
    {
        LevelCell playerStart = m_activeLevel.m_playerStart;
        if (playerStart != null)
        {
            Player player = FindObjectOfType<Player>();
            if (player != null)
            {
                player.TeleportTo(playerStart.transform.position, playerStart.transform.rotation);
                player.DiscoverGroundCell();
            }
        }
    }

    [ContextMenu("Transition to previous level")]
    public void TransitionToPreviousLevel()
    {
        m_currentLevelIndex = m_currentLevelIndex - 1;
        if(m_currentLevelIndex < 0)
        {
            m_currentLevelIndex += m_levelDatas.Length;
        }
        TransitionTo(m_levelDatas[m_currentLevelIndex].m_data);
    }

    [ContextMenu("Transition to next level")]
    public void TransitionToNextLevel()
    {
        m_currentLevelIndex = (m_currentLevelIndex + 1) % m_levelDatas.Length;
        TransitionTo(m_levelDatas[m_currentLevelIndex].m_data);
    }

    [ContextMenu("Reset current level")]
    public void ResetCurrentLevel()
    {
        TransitionTo(m_levelDatas[m_currentLevelIndex].m_data);
    }

    public void TransitionTo(LevelData data)
    {
        StartCoroutine(DoTransition(data));
    }

    protected IEnumerator DoTransition(LevelData data)
    {
        m_isTransitioning = true;
        s_transitionDelays.Clear();
        Game.Instance.m_player.Stop();
        Game.Instance.m_player.transform.parent = null;

        Level newLevel = new Level();
        newLevel.Init(data, m_levelPalettes[data.m_paletteIndex]);

        if(m_activeLevel != null)
        {
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
                        cell.Remove(GetTransitionDelay(cell.m_data.m_y));// LevelCell.kTransitionTime * 0.5f * (m_activeLevel.m_data.m_height - cell.m_data.m_y));
                    }
                    else
                    {
                        newLevel.AddCell(cell);
                    }
                }
                else
                {
                    cell.Remove(GetTransitionDelay(cell.m_data.m_y)); //LevelCell.kTransitionTime * 0.5f * (m_activeLevel.m_data.m_height - cell.m_data.m_y));
                }
            }
        }
        
        m_activeLevel = newLevel;
        float delayTime = m_activeLevel.GenerateMissingCells();
        yield return new WaitForSeconds(delayTime + LevelCell.kTransitionTime + 0.5f);
        yield return new WaitForEndOfFrame();
        MovePlayerToStart();
        m_isTransitioning = false;
    }

    protected static Dictionary<int, float> s_transitionDelays = new Dictionary<int, float>();
    public static float GetTransitionDelay(int y)
    {
        float delay;
        if(!s_transitionDelays.TryGetValue(y, out delay))
        {
            delay = s_transitionDelays.Count * LevelCell.kTransitionTime * 0.5f;
            s_transitionDelays.Add(y, delay);
        }
        return delay;
    }
}
