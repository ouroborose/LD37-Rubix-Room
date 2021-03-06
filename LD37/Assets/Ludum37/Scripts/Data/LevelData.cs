﻿using System.Collections.Generic;

[System.Serializable]
public class LevelData {
    public int m_paletteIndex = 0;

    public int m_width = 0;
    public int m_height = 0; 
    public int m_depth = 0;

    public LevelCellData[] m_cellDatas;
    public List<LevelCellData> m_occupiedCells;
    

    public void Init(int width, int height, int depth)
    {
        m_width = width;
        m_height = height;
        m_depth = depth;
        m_cellDatas = new LevelCellData[width*height*depth];
        m_occupiedCells = new List<LevelCellData>();
    }

    public int GetCellIndex(int x, int y, int z)
    {
        return x + m_height * y + z * m_height * m_width;
    }

    public void SetCellData(int x, int y, int z, LevelCellType type, RotationId rotationId = RotationId.Up)
    {
        if (IsInBounds(x, y, z))
        {
            LevelCellData data = new LevelCellData(x,y,z, type, rotationId);
            m_cellDatas[GetCellIndex(x, y, z)] = data;
            if(data.m_type != LevelCellType.Empty)
            {
                m_occupiedCells.Add(data);
            }
        }
    }

    public void AddCellData(LevelCellData data)
    {
        SetCellData(data.m_x, data.m_y, data.m_z, data.m_type, data.m_rotationId);
    }

    public LevelCellData GetCellData(int x, int y, int z)
    {
        LevelCellData data = null;
        if (IsInBounds(x,y,z))
        {
            data = m_cellDatas[GetCellIndex(x, y, z)];
        }
        
        if (data == null)
        {
            data = new LevelCellData(x, y, z);
        }
        return data;
    }

    public bool IsInBounds(int x, int y, int z)
    {
        return x >= 0 && x < m_width &&
            y >= 0 && y < m_height &&
            z >= 0 && z < m_depth;
    }
}
