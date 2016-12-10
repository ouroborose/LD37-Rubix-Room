using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level {
    public LevelData m_data;
    public LevelPalette m_palette;

    public LevelCell[] m_levelCells;

    public void Init(LevelData data, LevelPalette palette)
    {
        m_data = data;
        m_palette = palette;
        m_levelCells = new LevelCell[m_data.m_cellDatas.Length];
    }

    public void GenerateMissingCells()
    {
        for (int z = 0; z < m_data.m_depth; ++z)
        {
            for (int y = 0; y < m_data.m_height; ++y)
            {
                for (int x = 0; x < m_data.m_width; ++x)
                {
                    LevelCell existingCell = GetCell(x, y, z);
                    if(existingCell != null)
                    {
                        continue;
                    }

                    LevelData.LevelCellData cellData = m_data.GetCellData(x, y, z);
                    if(cellData.m_type == LevelCell.CellType.Empty)
                    {
                        continue;
                    }

                    GameObject prefab = m_palette.GetLevelCellPrefab(cellData.m_type);
                    GameObject cellObj = GameObject.Instantiate(prefab, GetCellPosition(x, y, z), GetRotation(cellData.m_rotationId),LevelManager.Instance.transform);
                    LevelCell cell = cellObj.GetComponent<LevelCell>();
                    cell.Init(x, y, z);
                    cell.Show(LevelCell.kTransitionTime * 0.5f * y);
                    SetCell(x, y, z, cell);
                }
            }
        }
    }

    public void SetCell(int x, int y, int z, LevelCell cell)
    {
        if (m_data.IsInBounds(x,y,z))
        {
            m_levelCells[m_data.GetCellIndex(x, y, z)] = cell;
        }
    }

    public LevelCell GetCell(int x, int y, int z)
    {
        if (m_data.IsInBounds(x, y, z))
        {
            return m_levelCells[m_data.GetCellIndex(x, y, z)];
        }

        return null;
    }

    public Vector3 GetCellPosition(int x, int y, int z)
    {
        return new Vector3(x * m_palette.m_spacingSize, y * m_palette.m_spacingSize, z * m_palette.m_spacingSize);
    }

    public Quaternion GetRotation(LevelCell.RotationId rotationId)
    {
        return Quaternion.identity;
    }

    public void DestroyAllCells()
    {
        for(int i = 0, n = m_levelCells.Length; i < n; ++i)
        {
            LevelCell cell = m_levelCells[i];
            if(cell != null)
            {
                GameObject.Destroy(cell.gameObject);
            }
        }
    }

    public void MapCells(LevelCell[] cells)
    {
        for(int i = 0, n = cells.Length; i < n; ++i)
        {
            LevelCell cell = cells[i];
            if(cell != null && !cell.m_isMarkedForDeletion)
            {
                SetCell(cell.m_x, cell.m_y, cell.m_z, cell);
            }
        }
    }
}
