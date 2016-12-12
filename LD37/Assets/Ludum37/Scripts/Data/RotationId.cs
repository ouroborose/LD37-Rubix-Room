using UnityEngine;

public enum RotationId : int
{
    Up = 0,
    Up90,
    Up180,
    Up270,

    Down,
    Down90,
    Down180,
    Down270,

    Forward,
    Forward90,
    Forward180,
    Forward270,

    Back,
    Back90,
    Back180,
    Back270,

    Right,
    Right90,
    Right180,
    Right270,

    Left,
    Left90,
    Left180,
    Left270,
    NumIds,
}

public static class RotationUtil
{
    public readonly static Quaternion[] kRotations =
    {
        Quaternion.LookRotation(Vector3.forward, Vector3.up),
        Quaternion.LookRotation(Vector3.right, Vector3.up),
        Quaternion.LookRotation(Vector3.back, Vector3.up),
        Quaternion.LookRotation(Vector3.left, Vector3.up),

        Quaternion.LookRotation(Vector3.forward, Vector3.down),
        Quaternion.LookRotation(Vector3.right, Vector3.down),
        Quaternion.LookRotation(Vector3.back, Vector3.down),
        Quaternion.LookRotation(Vector3.left, Vector3.down),

        Quaternion.LookRotation(Vector3.up, Vector3.forward),
        Quaternion.LookRotation(Vector3.right, Vector3.forward),
        Quaternion.LookRotation(Vector3.down, Vector3.forward),
        Quaternion.LookRotation(Vector3.left, Vector3.forward),

        Quaternion.LookRotation(Vector3.up, Vector3.back),
        Quaternion.LookRotation(Vector3.right, Vector3.back),
        Quaternion.LookRotation(Vector3.down, Vector3.back),
        Quaternion.LookRotation(Vector3.left, Vector3.back),

        Quaternion.LookRotation(Vector3.up, Vector3.right),
        Quaternion.LookRotation(Vector3.forward, Vector3.right),
        Quaternion.LookRotation(Vector3.down, Vector3.right),
        Quaternion.LookRotation(Vector3.back, Vector3.right),

        Quaternion.LookRotation(Vector3.up, Vector3.left),
        Quaternion.LookRotation(Vector3.forward, Vector3.left),
        Quaternion.LookRotation(Vector3.down, Vector3.left),
        Quaternion.LookRotation(Vector3.left, Vector3.left),
    };

    public static Quaternion GetRotation(RotationId id)
    {
        return kRotations[(int)id];
    }

    public static RotationId GetClosestId(Quaternion rotation)
    {
        float bestAngle = float.MaxValue;
        int bestId = 0;
        for (int i = 0, n = kRotations.Length ; i < n; ++i)
        {
            float angle = Quaternion.Angle(rotation, kRotations[i]);
            if(angle < bestAngle)
            {
                bestAngle = angle;
                bestId = i;
            }
        }

        return (RotationId)bestId;
    }
}
