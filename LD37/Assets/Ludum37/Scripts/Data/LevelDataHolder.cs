using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelDataHolder", menuName = "Custom/LevelDataHolder")]
public class LevelDataHolder : ScriptableObject {
    [Multiline]
    public string[] m_generationStrings;
    public int m_generationWidth;
    public int m_generationHeight;
    public int m_generationDepth;

    public LevelData m_data;

    [ContextMenu("Generate From Strings")]
    public void GenerateFromStrings()
    {
        m_data = new LevelData();
        m_data.Init(m_generationWidth, m_generationHeight, m_generationDepth);
        for (int z = 0; z < m_generationDepth; ++z)
        {
            // iterate backwards since its easier to think about with first string as being the back most
            string[] rows = m_generationStrings[m_generationDepth-1-z].Split('\n');
            for (int y = 0; y < m_generationHeight; ++y)
            {
                // iterate backwards since its easier to think about with first line being the top row
                string[] cells = rows[m_generationHeight-1-y].Split(',');
                for(int x = 0; x < m_generationWidth; ++x)
                {
                    m_data.SetCellData(x, y, z, (LevelCell.CellType)int.Parse(cells[x]));
                }
            }
        }
    }

    [ContextMenu("Generate Box")]
    public void GenerateBox()
    {
        m_data = new LevelData();
        m_data.Init(m_generationWidth, m_generationHeight, m_generationDepth);

        int xEnd = m_generationWidth - 1;
        int yEnd = m_generationHeight - 1;
        int zEnd = m_generationDepth - 1;

        for(int x = 0; x < m_generationWidth; ++x)
        {
            for (int y = 0; y < m_generationHeight; ++y)
            {
                m_data.SetCellData(x, y, 0, LevelCell.CellType.Solid);
                m_data.SetCellData(x, y, zEnd, LevelCell.CellType.Solid);
            }
        }

        for (int z = 0; z < m_generationWidth; ++z)
        {
            for (int y = 0; y < m_generationHeight; ++y)
            {
                m_data.SetCellData(0, y, z, LevelCell.CellType.Solid);
                m_data.SetCellData(xEnd, y, z, LevelCell.CellType.Solid);
            }
        }

        for (int x = 0; x < m_generationWidth; ++x)
        {
            for (int z = 0; z < m_generationDepth; ++z)
            {
                m_data.SetCellData(x, 0, z, LevelCell.CellType.Solid);
                m_data.SetCellData(x, yEnd, z, LevelCell.CellType.Solid);
            }
        }
    }
}
