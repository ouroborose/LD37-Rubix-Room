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
}
