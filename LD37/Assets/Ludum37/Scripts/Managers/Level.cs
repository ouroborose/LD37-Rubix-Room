using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level {
    public LevelData m_data;
    public LevelPalette m_palette;

    public Dictionary<int, LevelCell> m_levelCells;

    public void Init(LevelData data, LevelPalette palette)
    {
        m_data = data;
        m_palette = palette;
        m_levelCells = new Dictionary<int, LevelCell>();
    }

    public void GenerateMissingCells()
    {
        for(int i = 0, n = m_data.m_occupiedCells.Count; i < n; ++i)
        {
            LevelData.LevelCellData cellData = m_data.m_occupiedCells[i];

            LevelCell existingCell = GetCell(cellData.m_x, cellData.m_y, cellData.m_z);
            if (existingCell != null)
            {
                continue;
            }

            GameObject prefab = m_palette.GetLevelCellPrefab(cellData.m_type);
            GameObject cellObj = GameObject.Instantiate(prefab, GetCellPosition(cellData.m_x, cellData.m_y, cellData.m_z), GetCellRotation(cellData.m_rotationId), LevelManager.Instance.transform);
            cellObj.transform.localScale = Vector3.one * m_palette.m_scaleSize;

            LevelCell cell = cellObj.GetComponent<LevelCell>();
            cell.Init(cellData.m_x, cellData.m_y, cellData.m_z);
            cell.Show(LevelCell.kTransitionTime * 0.5f * cellData.m_y);
            SetCell(cellData.m_x, cellData.m_y, cellData.m_z, cell);
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
        LevelCell cell;
        if (m_levelCells.TryGetValue(m_data.GetCellIndex(x, y, z), out cell))
        {
            return cell;
        }

        return null;
    }

    public Vector3 GetCellPosition(int x, int y, int z)
    {
        return new Vector3(x * m_palette.m_spacingSize, y * m_palette.m_spacingSize, z * m_palette.m_spacingSize);
    }

    public Quaternion GetCellRotation(LevelCell.RotationId rotationId)
    {
        return Quaternion.identity;
    }

    public void DestroyAllCells()
    {
        foreach(var pair in m_levelCells)
        {
            GameObject.Destroy(pair.Value.gameObject);
        }
    }

    public void MapCells(Dictionary<int,LevelCell> cells)
    {
        foreach(var pair in cells)
        {
            LevelCell cell = pair.Value;
            if (cell != null && !cell.m_isMarkedForDeletion)
            {
                SetCell(cell.m_x, cell.m_y, cell.m_z, cell);
            }
        }
    }
}
