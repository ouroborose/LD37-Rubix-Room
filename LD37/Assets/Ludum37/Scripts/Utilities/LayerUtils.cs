using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LayerUtils
{
    public readonly static int kDefaultCameraLayerMask = ~LayerMask.GetMask("LevelEditor");
    public readonly static int kLevelEditorCameraLayerMask = LayerMask.GetMask("Default", "LevelEditor");
}
