[System.Serializable]
public class LevelCellData
{
    public int m_x = 0;
    public int m_y = 0;
    public int m_z = 0;
    public LevelCellType m_type = LevelCellType.Empty;
    public RotationId m_rotationId = RotationId.Up;

    public LevelCellData(LevelCellData other)
    {
        m_x = other.m_x;
        m_y = other.m_y;
        m_z = other.m_z;
        m_type = other.m_type;
        m_rotationId = other.m_rotationId;
    }

    public LevelCellData(int x, int y, int z)
    {
        m_x = x;
        m_y = y;
        m_z = z;
        m_type = LevelCellType.Empty;
        m_rotationId = RotationId.Up;
    }

    public LevelCellData(int x, int y, int z, LevelCellType type, RotationId rotationId)
    {
        m_x = x;
        m_y = y;
        m_z = z;
        m_type = type;
        m_rotationId = rotationId;
    }
}
