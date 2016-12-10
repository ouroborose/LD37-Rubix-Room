using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LevelData {
    public int m_paletteIndex = 0;

    public int m_width = 0;
    public int m_height = 0; 
    public int m_depth = 0;

    [System.Serializable]
    public class LevelCellData
    {
        public LevelCell.CellType m_type = LevelCell.CellType.Empty;
        public LevelCell.RotationId m_rotationId = LevelCell.RotationId.Default;
    }
    
    public LevelCellData[] m_cellDatas;

    public void Init(int width, int height, int depth)
    {
        m_width = width;
        m_height = height;
        m_depth = depth;
        m_cellDatas = new LevelCellData[width*height*depth];
    }

    public int GetCellIndex(int x, int y, int z)
    {
        return x + m_height * y + z * m_height * m_width;
    }

    public void SetCellData(int x, int y, int z, LevelCell.CellType type, LevelCell.RotationId rotationId = LevelCell.RotationId.Default)
    {
        if (IsInBounds(x, y, z))
        {
            LevelCellData data = new LevelCellData();
            data.m_type = type;
            data.m_rotationId = rotationId;
            m_cellDatas[GetCellIndex(x, y, z)] = data;
        }
    }

    public LevelCellData GetCellData(int x, int y, int z)
    {
        if (IsInBounds(x,y,z))
        {
            return m_cellDatas[GetCellIndex(x, y, z)];
        }

        return new LevelCellData();
    }

    public bool IsInBounds(int x, int y, int z)
    {
        return x >= 0 && x < m_width &&
            y >= 0 && y < m_height &&
            z >= 0 && z < m_depth;
    }
}
