using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelCell : MonoBehaviour {
    public enum CellType: int
    {
        Empty = 0,
        Solid
    }

    public enum RotationId: int
    {
        Default = 0,
    }

    public CellType m_type = CellType.Empty;
    public int m_x = 0;
    public int m_y = 0;
    public int m_z = 0;

    public void Init(int x, int y, int z)
    {
        m_x = x;
        m_y = y;
        m_z = z;
    }

    public void Show(bool instant = false)
    {

    }

    public void Hide(bool instant = false)
    {

    }
}
