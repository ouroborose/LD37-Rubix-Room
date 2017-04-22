using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
#endif

public class LevelEditor : MonoBehaviour {
    private static LevelEditor s_instance;
    public static LevelEditor Instance { get { return s_instance; } }

    public Transform m_previewer;
    public VRController m_paletteController;
    public VRController m_interactionController;

    protected LevelCellType m_currentType = LevelCellType.Solid;

    protected List<Rect> m_buttonRects;

    protected Stack<BaseCommand> m_undoStack = new Stack<BaseCommand>();
    protected Stack<BaseCommand> m_redoStack = new Stack<BaseCommand>();

    protected void Awake()
    {
        s_instance = this;

        m_previewer.gameObject.SetActive(false);

        m_buttonRects = new List<Rect>();
        for (int i = 0, n = (int)LevelCellType.NumTypes; i < n; ++i)
        {
            m_buttonRects.Add(new Rect(10, 10 + i * 60, 75, 50));
        }
    }

    public void OnEnable()
    {
        Camera.main.cullingMask = LayerUtils.kLevelEditorCameraLayerMask;
        m_previewer.gameObject.SetActive(true);
    }

    public void OnDisable()
    {
        Camera.main.cullingMask = LayerUtils.kDefaultCameraLayerMask;
        m_previewer.gameObject.SetActive(false);
        ClearCommandStacks();
    }
    
    protected void Update()
    {
        if(Input.GetKeyDown(KeyCode.Z))
        {
            UndoCommand();
        }
        else if(Input.GetKeyDown(KeyCode.X))
        {
            RedoCommand();
        }

        if (Input.mouseScrollDelta.y != 0)
        {
            int newType = ((int)m_currentType - (int)Input.mouseScrollDelta.y) % ((int)LevelCellType.NumTypes);
            if (newType < 0)
            {
                newType += (int)LevelCellType.NumTypes;
            }
            m_currentType = (LevelCellType)newType;
        }

        Vector3 guiCheckMousePos = Input.mousePosition;
        guiCheckMousePos.y = Screen.height - guiCheckMousePos.y;
        for(int i = 0, n = m_buttonRects.Count; i < n; ++i)
        {
            if(m_buttonRects[i].Contains(guiCheckMousePos))
            {
                // early out if button is over GUI
                return;
            }
        }

        m_previewer.localScale = Vector3.one * LevelManager.Instance.m_activeLevel.m_palette.m_scaleSize;

        if (Game.Instance.m_isVR)
        {
            PositionPreviewer(m_interactionController.TargetingHit);
        }
        else
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                PositionPreviewer(hit);

                if (Input.GetMouseButtonDown(0))
                {
                    if (m_currentType == LevelCellType.Empty)
                    {
                        // delete cell
                        LevelCell cell = hit.collider.GetComponentInParent<LevelCell>();
                        ExecuteCommand(new DeleteCommand(cell.m_data));
                    }
                    else if (Input.GetKey(KeyCode.LeftShift))
                    {
                        // rotate
                        LevelCell cell = hit.collider.GetComponentInParent<LevelCell>();
                        if (cell.m_data.m_type != LevelCellType.Solid)
                        {
                            ExecuteCommand(new RotateCommand(cell.m_data));
                        }
                    }
                    else
                    {
                        // add cell
                        int rotationId = (int)RotationUtil.GetClosestId(Quaternion.LookRotation(Vector3.Cross(Camera.main.transform.forward, hit.normal), hit.normal));
                        rotationId /= 4;
                        rotationId *= 4;
                        ExecuteCommand(new CreateCommand(m_previewer.position, m_currentType, (RotationId)rotationId));
                    }
                }
                else if (Input.GetMouseButtonDown(2))
                {
                    // delete cell
                    LevelCell cell = hit.collider.GetComponentInParent<LevelCell>();
                    ExecuteCommand(new DeleteCommand(cell.m_data));
                }

            }
        }
    }

    protected void PositionPreviewer(RaycastHit hit)
    {
        Vector3 pos = hit.point;
        if (m_currentType == LevelCellType.Empty || Input.GetKey(KeyCode.LeftShift))
        {
            pos -= hit.normal * LevelManager.Instance.m_activeLevel.m_palette.m_spacingSize * 0.5f;
        }
        else
        {
            pos += hit.normal * LevelManager.Instance.m_activeLevel.m_palette.m_spacingSize * 0.5f;
        }
        m_previewer.position = LevelManager.Instance.m_activeLevel.GetCellWorldPosition(pos);
    }

    public void ExecuteCommand(BaseCommand command)
    {
        command.Execute();
        m_undoStack.Push(command);
        m_redoStack.Clear();
    }

    public void UndoCommand()
    {
        if(m_undoStack.Count > 0)
        {
            BaseCommand command = m_undoStack.Pop();
            command.Undo();
            m_redoStack.Push(command);
        }
    }

    public void RedoCommand()
    {
        if(m_redoStack.Count > 0)
        {
            BaseCommand command = m_redoStack.Pop();
            command.Execute();
            m_undoStack.Push(command);
        }
    }

    public void ClearCommandStacks()
    {
        m_undoStack.Clear();
        m_redoStack.Clear();
    }

#if UNITY_EDITOR
    [MenuItem("LevelEditor/Save LevelData...")]
    public static void SaveToFile()
    {
        if(!Application.isPlaying)
        {
            Debug.LogWarning("Cannot save when game is not running");
            return;
        }

        Debug.Log("Saving");
        string path = EditorUtility.SaveFilePanelInProject("Saving level", "NewLevelData", "asset", "Please enter a name", "Assets/Ludum37/ReferencedResources/Data/LevelData");

        if (path.Length <= 0)
        {
            Debug.Log("Save canceled");
            return;
        }

        Debug.Log(path);
        LevelDataHolder levelData = AssetDatabase.LoadAssetAtPath<LevelDataHolder>(path);
        if(levelData == null)
        {
            levelData = Editor.CreateInstance<LevelDataHolder>();
            levelData.m_data = LevelManager.Instance.m_activeLevel.CreateLevelData();
            AssetDatabase.CreateAsset(levelData, path);
        }
        else
        {
            levelData.m_data = LevelManager.Instance.m_activeLevel.CreateLevelData();
            EditorUtility.SetDirty(levelData);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = levelData;
    }

    protected void OnGUI()
    {
        for(int i = 0, n = (int)LevelCellType.NumTypes; i < n; ++i)
        {
            LevelCellType type = (LevelCellType)i;

            if(m_currentType == type)
            {
                GUI.color = Color.green;
            }
            else
            {
                GUI.color = Color.white;
            }

            if (GUI.Button(m_buttonRects[i], type.ToString()))
            {
                m_currentType = type;
            }
        }
    }
#endif

    public class DeleteCommand : BaseCommand
    {
        public LevelCellData m_data;

        public DeleteCommand(LevelCellData data)
        {
            m_data = new LevelCellData(data);
        }

        public override void Execute()
        {
            base.Execute();
            LevelCell cell = LevelManager.Instance.m_activeLevel.GetCell(m_data.m_x, m_data.m_y, m_data.m_z);
            LevelManager.Instance.m_activeLevel.RemoveCell(cell);
            Destroy(cell.gameObject);
        }

        public override void Undo()
        {
            base.Undo();
            LevelManager.Instance.m_activeLevel.CreateCell(m_data).Show();
        }
    }

    public class CreateCommand : BaseCommand
    {
        public LevelCellData m_data;

        public CreateCommand(Vector3 worldPos, LevelCellType type, RotationId rotationId)
        {
            m_data = LevelManager.Instance.m_activeLevel.CreateCellData(worldPos, type, rotationId);
        }

        public override void Execute()
        {
            base.Execute();
            LevelManager.Instance.m_activeLevel.CreateCell(m_data).Show();
        }

        public override void Undo()
        {
            base.Undo();
            LevelCell cell = LevelManager.Instance.m_activeLevel.GetCell(m_data.m_x, m_data.m_y, m_data.m_z);
            LevelManager.Instance.m_activeLevel.RemoveCell(cell);
            Destroy(cell.gameObject);
        }
    }

    public class RotateCommand : BaseCommand
    {
        public LevelCellData m_data;
        public RotationId m_originalRotationId;
        public RotationId m_newRotationId;

        public RotateCommand(LevelCellData data)
        {
            m_data = new LevelCellData(data);
            m_originalRotationId = m_data.m_rotationId;
            m_newRotationId = (RotationId)((((int)m_originalRotationId) + 1) % ((int)RotationId.NumIds));
        }

        public override void Execute()
        {
            base.Execute();
            SetRotation(m_newRotationId);
        }

        public override void Undo()
        {
            base.Undo();
            SetRotation(m_originalRotationId);
        }

        public void SetRotation(RotationId id)
        {
            LevelCell cell = LevelManager.Instance.m_activeLevel.GetCell(m_data.m_x, m_data.m_y, m_data.m_z);
            cell.m_data.m_rotationId = id;
            cell.transform.rotation = RotationUtil.GetRotation(cell.m_data.m_rotationId);
        }
    }
}


