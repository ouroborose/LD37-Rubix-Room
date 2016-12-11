﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelPalette", menuName = "Custom/LevelPalette")]
public class LevelPalette : ScriptableObject {
    public float m_spacingSize = 1.0f;
    public float m_scaleSize = 1.0f;
    public GameObject[] m_palettePrefabs;

    public Dictionary<LevelCell.CellType, List<GameObject>> m_paletteMapping = new Dictionary<LevelCell.CellType, List<GameObject>>();

    public void Init()
    {
        for(int i = 0, n = m_palettePrefabs.Length; i < n; ++i)
        {
            GameObject prefab = m_palettePrefabs[i];
            LevelCell cell = prefab.GetComponent<LevelCell>();
            List<GameObject> prefabList = null;
            if(!m_paletteMapping.TryGetValue(cell.m_type, out prefabList) && prefabList == null)
            {
                prefabList = new List<GameObject>();
                m_paletteMapping.Add(cell.m_type, prefabList);
            }

            prefabList.Add(prefab);
        }
    }
    
    public GameObject GetLevelCellPrefab(LevelCell.CellType type)
    {
        List<GameObject> prefabList;
        if(m_paletteMapping.TryGetValue(type, out prefabList) && prefabList != null)
        {
            return prefabList[Random.Range(0, prefabList.Count)];
        }

        return null;
    }
}