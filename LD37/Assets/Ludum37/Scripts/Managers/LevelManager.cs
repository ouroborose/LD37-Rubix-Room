using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour {
    private static LevelManager s_instance;
    public static LevelManager Instance { get { return s_instance; } }

    public LevelDataHolder[] m_levelDatas;
    public LevelPalette[] m_levelPalettes;

    public Level m_activeLevel;

    protected void Awake()
    {
        s_instance = this;

        for (int i = 0, n = m_levelPalettes.Length; i < n; ++i)
        {
            m_levelPalettes[i].Init();
        }

        // create first level
        if (m_levelDatas.Length > 0)
        {
            m_activeLevel = new Level();
            LevelData data = m_levelDatas[0].m_data;
            m_activeLevel.Init(data, m_levelPalettes[data.m_paletteIndex]);
            m_activeLevel.GenerateCells();
        }
    }
}
