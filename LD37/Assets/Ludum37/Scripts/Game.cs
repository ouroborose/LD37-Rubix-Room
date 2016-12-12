using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game : MonoBehaviour
{
    protected static Game s_instance;
    public static Game Instance { get { return s_instance; } }

    public readonly static Quaternion[] kValidRotations =
    {
        //Quaternion.identity,
        Quaternion.Euler(90,0,0),
        Quaternion.Euler(180,0,0),
        Quaternion.Euler(270,0,0),
        Quaternion.Euler(0,90,0),
        Quaternion.Euler(0,180,0),
        Quaternion.Euler(0,270,0),
        Quaternion.Euler(0,0,90),
        Quaternion.Euler(0,0,180),
        Quaternion.Euler(0,0,270),
    };

    public readonly static Vector3[] kRotationAxes =
    {
        Vector3.up,
        Vector3.right,
        Vector3.forward,
    };

    public Player m_player;

    public float m_cameraMoveSpeed = 1.0f;

    public float m_xCameraLookSensitivity = 10.0f;
    public float m_yCameraLookSensitivity = 10.0f;

    public float m_autoCompleteSpeed = 0.25f;
    public LeanTweenType m_autoCompleteEase = LeanTweenType.easeOutSine;
    
    protected LevelCell m_selectedCell;
    protected Level.CellGroup m_selectedXLayer = new Level.CellGroup();
    protected Level.CellGroup m_selectedYLayer = new Level.CellGroup();
    protected Level.CellGroup m_selectedZLayer = new Level.CellGroup();

    protected Vector3 m_cameraEulers = Vector3.zero;

    protected GameObject m_rotater;

    protected Vector3 m_rotationMouseStart;
    protected Vector3 m_rotationHitNormal;

    protected bool m_rotationStarted = false;
    protected Level.CellGroup m_rotationGroup;
    protected Vector3 m_rotationAxis = Vector3.zero;
    protected bool m_useMouseY = false;

    protected bool m_isAutoCompleting = false;

    protected Vector3 m_lastMousePos;

    protected void Awake()
    {
        s_instance = this;
    }

    protected void Start()
    {
        //Camera.main.transform.position = LevelManager.Instance.m_activeLevel.GetCenterWorldPosition();
        m_cameraEulers = Camera.main.transform.eulerAngles;
        m_lastMousePos = Input.mousePosition;
    }

    protected void Update()
    {
        if(LevelManager.Instance == null)
        {
            return;
        }

        if (!m_isAutoCompleting)
        {
            if(Input.GetKeyDown(KeyCode.F1) && !m_rotationStarted && LevelEditor.Instance != null)
            {
                // editor mode
                LevelEditor.Instance.enabled = !LevelEditor.Instance.enabled;
            }
            else if(LevelEditor.Instance == null || !LevelEditor.Instance.enabled)
            {
                HandlePlayerControls();
            }
        }
    }

    public void LateUpdate()
    {
        HandleCameraControls();

        m_lastMousePos = Input.mousePosition;
    }

    public void HandleCameraControls()
    {
        Vector3 moveDir = new Vector3(Input.GetAxis("Horizontal"), 0.0f, Input.GetAxis("Vertical"));
        if(Input.GetKey(KeyCode.Space))
        {
            moveDir.y += 1;
        }
        if (Input.GetKey(KeyCode.LeftControl))
        {
            moveDir.y -= 1;
        }

        Vector3 newPos = Camera.main.transform.position + Camera.main.transform.TransformDirection(moveDir) * m_cameraMoveSpeed * Time.deltaTime;
        Bounds bounds = LevelManager.Instance.m_activeLevel.m_worldBounds;
        newPos.x = Mathf.Clamp(newPos.x, bounds.min.x, bounds.max.x);
        newPos.y = Mathf.Clamp(newPos.y, bounds.min.y, bounds.max.y);
        newPos.z = Mathf.Clamp(newPos.z, bounds.min.z, bounds.max.z);
        Camera.main.transform.position = newPos;

        if (Input.GetMouseButton(1))
        {
            Vector3 delta = Input.mousePosition - m_lastMousePos;
            
            m_cameraEulers.y += -delta.x * m_xCameraLookSensitivity * Time.deltaTime;
            m_cameraEulers.x += delta.y * m_yCameraLookSensitivity * Time.deltaTime;
            m_cameraEulers.x = Mathf.Clamp(m_cameraEulers.x, -89, 89);
            Camera.main.transform.eulerAngles = m_cameraEulers;
        }
    }

    public void HandlePlayerControls()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, float.MaxValue, LayerUtils.kDefaultCameraLayerMask))
            {
                LevelCell cell = hit.collider.GetComponent<LevelCell>();
                if(Input.GetKey(KeyCode.LeftShift))
                {
                    if (cell != m_selectedCell)
                    {
                        Select(cell);

                        m_rotationHitNormal = hit.normal;
                        m_rotationMouseStart = Input.mousePosition;
                        m_rotationStarted = false;
                    }
                }
                else
                {
                    Vector3 destPos = hit.point;
                    if (cell.m_data.m_type == LevelCellType.Ramp)
                    {
                        destPos -= hit.normal * LevelManager.Instance.m_activeLevel.m_palette.m_spacingSize * 0.5f;
                    }
                    else
                    {
                        destPos += hit.normal * LevelManager.Instance.m_activeLevel.m_palette.m_spacingSize * 0.5f;
                    }
                    m_player.PathTo(destPos);
                }
            }
        }
        else if (Input.GetMouseButton(0))
        {
            if (m_selectedCell != null)
            {
                if (!m_rotationStarted)
                {
                    Vector3 deltaMouse = Input.mousePosition - m_rotationMouseStart;
                    if (deltaMouse.sqrMagnitude > 50)
                    {
                        m_rotater = new GameObject("Rotater");
                        m_rotater.transform.SetParent(transform);
                        m_rotater.transform.position = LevelManager.Instance.m_activeLevel.GetCenterWorldPosition();
                        m_rotater.transform.rotation = Quaternion.identity;

                        Ray startRay = Camera.main.ScreenPointToRay(m_rotationMouseStart);
                        Ray currRay = Camera.main.ScreenPointToRay(Input.mousePosition);

                        Vector3 deltaDir = currRay.GetPoint(10.0f) - startRay.GetPoint(10.0f);
                        deltaDir.Normalize();
                        m_rotationAxis = GetRotationAxis(Vector3.Cross(Camera.main.transform.forward, deltaDir));

                        if (m_rotationAxis == Vector3.up)
                        {
                            m_rotationGroup = m_selectedYLayer;
                            m_rotationGroup.SetColor(Color.green);
                        }
                        else if (m_rotationAxis == Vector3.right)
                        {
                            m_rotationGroup = m_selectedXLayer;
                            m_rotationGroup.SetColor(Color.blue);
                        }
                        else
                        {
                            m_rotationGroup = m_selectedZLayer;
                            m_rotationGroup.SetColor(Color.red);
                        }

                        m_rotationGroup.SetParent(m_rotater.transform);
                        if(m_player.m_isMoving)
                        {
                            m_player.Stop();
                        }

                        m_rotationStarted = true;
                    }
                }

                if (m_rotationStarted)
                {
                    Ray lastRay = Camera.main.ScreenPointToRay(m_lastMousePos);
                    Ray currRay = Camera.main.ScreenPointToRay(Input.mousePosition);

                    Vector3 lastProjected = Vector3.ProjectOnPlane(lastRay.direction, m_rotationAxis);
                    Vector3 currProjected = Vector3.ProjectOnPlane(currRay.direction, m_rotationAxis);

                    Vector3 lastRight = Vector3.Cross(m_rotationAxis, lastProjected);

                    Vector3 eulers = m_rotater.transform.eulerAngles;
                    eulers += m_rotationAxis * Mathf.Sign(Vector3.Dot(lastRight, currProjected)) * Vector3.Angle(lastProjected, currProjected);
                    m_rotater.transform.eulerAngles = eulers;
                }
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            if (m_selectedCell != null && m_rotationStarted)
            {
                GameObject rotater = m_rotater;
                Level.CellGroup rotatedCells = new Level.CellGroup();
                rotatedCells.AddRange(m_rotationGroup);
                m_isAutoCompleting = true;
                LeanTween.rotate(rotater, GetClosestValidRotation(rotater.transform.rotation).eulerAngles, m_autoCompleteSpeed).setEase(m_autoCompleteEase).setOnComplete(() =>
                {
                    rotatedCells.SetParent(LevelManager.Instance.transform);
                    rotatedCells.ResetColor();
                    LevelManager.Instance.m_activeLevel.UpdateCells(rotatedCells);
                    Destroy(m_rotater);
                    m_player.m_gravity = -m_player.transform.up;
                    m_isAutoCompleting = false;
                });
                ClearSelection();
                m_rotationStarted = false;
            }
        }
    }

    public Vector3 GetRotationAxis(Vector3 matchVector, Vector3 exclusionVector = default(Vector3))
    {
        float bestDot = -1;
        Vector3 bestAxis = Vector3.zero;
        for(int i = 0, n = kRotationAxes.Length; i < n; ++i)
        {
            if(kRotationAxes[i] == exclusionVector)
            {
                continue;
            }

            float dot = Mathf.Abs(Vector3.Dot(matchVector, kRotationAxes[i]));
            if(dot > bestDot)
            {
                bestDot = dot;
                bestAxis = kRotationAxes[i];
            }
        }
        return bestAxis;
    }

    public Quaternion GetClosestValidRotation(Quaternion rotation)
    {
        float bestAngle = float.MaxValue;
        Quaternion bestRotation = Quaternion.identity;
        for(int i = 0, n = kValidRotations.Length; i < n; ++i)
        {
            Quaternion validRotation = kValidRotations[i];
            float angle = Quaternion.Angle(validRotation, rotation);
            if(angle < bestAngle)
            {
                bestAngle = angle;
                bestRotation = validRotation;
            }
        }

        return bestRotation;
    }

    public void ClearSelection()
    {
        /*
        m_selectedXLayer.ResetColor();
        m_selectedYLayer.ResetColor();
        m_selectedZLayer.ResetColor();
        if (m_selectedCell != null)
        {
            m_selectedCell.ResetColor();
        }
        */

        m_selectedXLayer.Clear();
        m_selectedYLayer.Clear();
        m_selectedZLayer.Clear();

        
        m_selectedCell = null;
    }

    public void Select(LevelCell cell)
    {
        ClearSelection();

        m_selectedCell = cell;

        m_selectedXLayer.AddRange(LevelManager.Instance.m_activeLevel.m_xLayers[cell.m_data.m_x]);
        m_selectedYLayer.AddRange(LevelManager.Instance.m_activeLevel.m_yLayers[cell.m_data.m_y]);
        m_selectedZLayer.AddRange(LevelManager.Instance.m_activeLevel.m_zLayers[cell.m_data.m_z]);

        /*
        m_selectedXLayer.SetColor(Color.blue);
        m_selectedYLayer.SetColor(Color.red);
        m_selectedZLayer.SetColor(Color.green);
        cell.SetColor(Color.yellow);
        */
    }
}
