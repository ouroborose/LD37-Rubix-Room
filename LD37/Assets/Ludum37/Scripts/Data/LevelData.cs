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
        public int m_x = 0;
        public int m_y = 0;
        public int m_z = 0;
        public LevelCell.CellType m_type = LevelCell.CellType.Empty;
        public LevelCell.RotationId m_rotationId = LevelCell.RotationId.Default;

        public LevelCellData(int x, int y, int z)
        {
            m_x = x;
            m_y = y;
            m_z = z;
            m_type = LevelCell.CellType.Empty;
            m_rotationId = LevelCell.RotationId.Default;
        }

        public LevelCellData(int x, int y, int z, LevelCell.CellType type, LevelCell.RotationId rotationId)
        {
            m_x = x;
            m_y = y;
            m_z = z;
            m_type = type;
            m_rotationId = rotationId;
        }
    }
    
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

    public void SetCellData(int x, int y, int z, LevelCell.CellType type, LevelCell.RotationId rotationId = LevelCell.RotationId.Default)
    {
        if (IsInBounds(x, y, z))
        {
            LevelCellData data = new LevelCellData(x,y,z, type, rotationId);
            m_cellDatas[GetCellIndex(x, y, z)] = data;
            if(data.m_type != LevelCell.CellType.Empty)
            {
                m_occupiedCells.Add(data);
            }
        }
    }

    public LevelCellData GetCellData(int x, int y, int z)
    {
        if (IsInBounds(x,y,z))
        {
            return m_cellDatas[GetCellIndex(x, y, z)];
        }

        return new LevelCellData(x,y,z);
    }

    public bool IsInBounds(int x, int y, int z)
    {
        return x >= 0 && x < m_width &&
            y >= 0 && y < m_height &&
            z >= 0 && z < m_depth;
    }
}
