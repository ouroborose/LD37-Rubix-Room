using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level {
    public LevelData m_data;
    public LevelPalette m_palette;

    public Dictionary<int, LevelCell> m_levelCells;

    public Bounds m_worldBounds;

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

        Vector3 size = new Vector3(m_data.m_width - 3, m_data.m_height - 3, m_data.m_depth - 3);
        size *= m_palette.m_spacingSize;
        m_worldBounds = new Bounds(GetCenterWorldPosition(), size);
    }

    public void GenerateMissingCells()
    {
        for(int i = 0, n = m_data.m_occupiedCells.Count; i < n; ++i)
        {
            LevelCellData cellData = m_data.m_occupiedCells[i];
            LevelCell existingCell = GetCell(cellData.m_x, cellData.m_y, cellData.m_z);
            if (existingCell != null)
            {
                continue;
            }

            CreateCell(cellData).Show(LevelCell.kTransitionTime * 0.5f * cellData.m_y);
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
            cell.UpdatePoseData(m_palette.m_spacingSize);
            AddCell(cell);
        }
    }

    public void RemoveCell(LevelCell cell)
    {
        m_xLayers[cell.m_data.m_x].Remove(cell);
        m_yLayers[cell.m_data.m_y].Remove(cell);
        m_zLayers[cell.m_data.m_z].Remove(cell);
        m_levelCells.Remove(GetCellIndex(cell));
    }

    public int GetCellIndex(LevelCell cell)
    {
        return m_data.GetCellIndex(cell.m_data.m_x, cell.m_data.m_y, cell.m_data.m_z);
    }

    public LevelCellData CreateCellData(Vector3 worldPos, LevelCellType type, RotationId rotationId = RotationId.Up)
    {
        int x = Mathf.RoundToInt(worldPos.x / m_palette.m_spacingSize);
        int y = Mathf.RoundToInt(worldPos.y / m_palette.m_spacingSize);
        int z = Mathf.RoundToInt(worldPos.z / m_palette.m_spacingSize);
        return new LevelCellData(x, y, z, type, rotationId);
    }

    public LevelCell CreateCell(Vector3 worldPos, LevelCellType type, RotationId rotationId = RotationId.Up)
    {
        return CreateCell(CreateCellData(worldPos, type, rotationId));
    }

    public LevelCell CreateCell(LevelCellData data)
    {
        return CreateCell(data.m_x, data.m_y, data.m_z, data.m_type, data.m_rotationId);
    }

    public LevelCell CreateCell(int x, int y, int z, LevelCellType type, RotationId rotationId = RotationId.Up)
    {
        GameObject prefab = m_palette.GetLevelCellPrefab(type);
        GameObject cellObj = GameObject.Instantiate(prefab, GetCellWorldPosition(x, y, z), RotationUtil.GetRotation(rotationId), LevelManager.Instance.transform);
        cellObj.transform.localScale = Vector3.one * m_palette.m_scaleSize;

        LevelCell cell = cellObj.GetComponent<LevelCell>();
        cell.Init(x, y, z);
        AddCell(cell);
        return cell;
    }

    public void AddCell(LevelCell cell)
    {
        if (m_data.IsInBounds(cell.m_data.m_x, cell.m_data.m_y, cell.m_data.m_z))
        {
            m_levelCells[GetCellIndex(cell)] = cell;

            m_xLayers[cell.m_data.m_x].Add(cell);
            m_yLayers[cell.m_data.m_y].Add(cell);
            m_zLayers[cell.m_data.m_z].Add(cell);
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

    public Vector3 GetCellWorldPosition(Vector3 worldPos)
    {
        int x = Mathf.RoundToInt(worldPos.x / m_palette.m_spacingSize);
        int y = Mathf.RoundToInt(worldPos.y / m_palette.m_spacingSize);
        int z = Mathf.RoundToInt(worldPos.z / m_palette.m_spacingSize);
        return new Vector3(x, y, z) * m_palette.m_spacingSize;
    }

    public Vector3 GetCellWorldPosition(int x, int y, int z)
    {
        return new Vector3(x,y,z) * m_palette.m_spacingSize;
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
    
    public LevelData CreateLevelData()
    {
        LevelData data = new LevelData();
        data.Init(m_data.m_width, m_data.m_height, m_data.m_depth);
        foreach(var pair in m_levelCells)
        {
            data.AddCellData(pair.Value.m_data);
        }
        return data;
    }
}
