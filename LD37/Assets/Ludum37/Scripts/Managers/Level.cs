using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level {
    public LevelData m_data;
    public LevelPalette m_palette;

    public Dictionary<int, LevelCell> m_levelCells;

    public class CellGroup : List<LevelCell>
    {
        public void SetColor(Color c)
        {
            for (int i = 0, n = Count; i < n; ++i)
            {
                this[i].SetColor(c);
            }
        }

        public void ResetColor()
        {
            for (int i = 0, n = Count; i < n; ++i)
            {
                this[i].ResetColor();
            }
        }

        public void SetParent(Transform parent)
        {
            for (int i = 0, n = Count; i < n; ++i)
            {
                this[i].transform.parent = parent;
            }
        }
    }

    public List<CellGroup> m_xLayers;
    public List<CellGroup> m_yLayers;
    public List<CellGroup> m_zLayers;

    public void Init(LevelData data, LevelPalette palette)
    {
        m_data = data;
        m_palette = palette;

        m_levelCells = new Dictionary<int, LevelCell>();

        m_xLayers = CreateNewLayers(m_data.m_width);
        m_yLayers = CreateNewLayers(m_data.m_height);
        m_zLayers = CreateNewLayers(m_data.m_depth);
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
            GameObject cellObj = GameObject.Instantiate(prefab, GetCellWorldPosition(cellData.m_x, cellData.m_y, cellData.m_z), GetCellRotation(cellData.m_rotationId), LevelManager.Instance.transform);
            cellObj.transform.localScale = Vector3.one * m_palette.m_scaleSize;

            LevelCell cell = cellObj.GetComponent<LevelCell>();
            cell.Init(cellData.m_x, cellData.m_y, cellData.m_z);
            cell.Show(LevelCell.kTransitionTime * 0.5f * cellData.m_y);
            AddCell(cell);
        }
    }

    protected List<CellGroup> CreateNewLayers(int size)
    {
        List<CellGroup> layers = new List<CellGroup>(size);
        for(int i = 0; i < size; ++i)
        {
            layers.Add(new CellGroup());
        }

        return layers;
    }

    public void UpdateCells(List<LevelCell> cells)
    {
        for(int i = 0, n = cells.Count; i < n; ++i)
        {
            RemoveCell(cells[i]);
        }

        for (int i = 0, n = cells.Count; i < n; ++i)
        {
            LevelCell cell = cells[i];
            cell.m_x = Mathf.RoundToInt(cell.transform.position.x / m_palette.m_spacingSize);
            cell.m_y = Mathf.RoundToInt(cell.transform.position.y / m_palette.m_spacingSize);
            cell.m_z = Mathf.RoundToInt(cell.transform.position.z / m_palette.m_spacingSize);
            AddCell(cell);
        }
    }

    public void RemoveCell(LevelCell cell)
    {
        m_xLayers[cell.m_x].Remove(cell);
        m_yLayers[cell.m_y].Remove(cell);
        m_zLayers[cell.m_z].Remove(cell);
        m_levelCells.Remove(GetCellIndex(cell));
    }

    public int GetCellIndex(LevelCell cell)
    {
        return m_data.GetCellIndex(cell.m_x, cell.m_y, cell.m_z);
    }

    public void AddCell(LevelCell cell)
    {
        if (m_data.IsInBounds(cell.m_x, cell.m_y, cell.m_z))
        {
            m_levelCells[GetCellIndex(cell)] = cell;

            m_xLayers[cell.m_x].Add(cell);
            m_yLayers[cell.m_y].Add(cell);
            m_zLayers[cell.m_z].Add(cell);
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

    public Vector3 GetCellWorldPosition(int x, int y, int z)
    {
        return new Vector3(x * m_palette.m_spacingSize, y * m_palette.m_spacingSize, z * m_palette.m_spacingSize);
    }

    public Quaternion GetCellRotation(LevelCell.RotationId rotationId)
    {
        return Quaternion.identity;
    }
    
    public Vector3 GetCenterWorldPosition()
    {
        return GetCellWorldPosition(m_data.m_width - 1, m_data.m_height - 1, m_data.m_depth - 1) * 0.5f;
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
                AddCell(cell);
            }
        }
    }
}
