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

    public const float kTransitionThreshold = 25.0f;

    public Player m_player;
    public Color m_rotationHighlightColor = Color.green;

    public bool m_isVR = true;
    public SteamVR_PlayArea m_vrPlayArea;
    public Transform m_vrEyesTransform;

    public float m_cameraMoveSpeed = 1.0f;

    public float m_xCameraLookSensitivity = 10.0f;
    public float m_yCameraLookSensitivity = 10.0f;

    public float m_autoCompleteSpeed = 0.25f;
    public LeanTweenType m_autoCompleteEase = LeanTweenType.easeOutSine;
    
    protected LevelCell m_selectedCell;
    public LevelCell SelectedCell { get { return m_selectedCell; } }
    protected Level.CellGroup m_selectedXLayer = new Level.CellGroup();
    protected Level.CellGroup m_selectedYLayer = new Level.CellGroup();
    protected Level.CellGroup m_selectedZLayer = new Level.CellGroup();

    protected Vector3 m_cameraEulers = Vector3.zero;

    protected GameObject m_rotater;

    protected Level.CellGroup m_rotationGroup;
    protected Vector3 m_rotationAxis = Vector3.zero;
    protected bool m_useMouseY = false;

    protected bool m_isAutoCompleting = false;

    protected Vector3 m_rotationMouseStart;
    protected Vector3 m_lastMousePos;

    protected Vector3 m_rotationStartDirProjected;
    protected float m_interactionDistance = 0.0f;


    protected void Awake()
    {
        s_instance = this;
    }

    protected void Start()
    {
        m_cameraEulers = Camera.main.transform.eulerAngles;
        m_lastMousePos = Input.mousePosition;
        
        Vector3 center = LevelManager.Instance.m_activeLevel.GetCenterWorldPosition();
        if(m_vrPlayArea != null)
        {
            center.y = 0;
            m_vrPlayArea.transform.position = center;
        }
        else
        {
            Camera.main.transform.position = LevelManager.Instance.m_activeLevel.GetCenterWorldPosition();
        }
    }

    protected void Update()
    {
        if(LevelManager.Instance == null)
        {
            return;
        }

        if (!m_isAutoCompleting && !LevelManager.Instance.m_isTransitioning)
        {
            if (Application.isEditor && Input.GetKeyDown(KeyCode.F1) && m_selectedCell == null && LevelEditor.Instance != null)
            {
                // editor mode
                LevelEditor.Instance.enabled = !LevelEditor.Instance.enabled;
            }
            else if (LevelEditor.Instance == null || !LevelEditor.Instance.enabled)
            {
                if (!m_isVR)
                {
                    HandlePlayerControls();
                }
            }
        }
        
    }

    protected void LateUpdate()
    {
        if (m_isVR)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                RecenterVRPlaySpace();
            }
        }
        else
        {
            HandleCameraControls();

            m_lastMousePos = Input.mousePosition;
        }
    }

    public void RecenterVRPlaySpace()
    {
        if(m_vrPlayArea == null)
        {
            return;
        }

        m_vrPlayArea.transform.position += LevelManager.Instance.m_activeLevel.GetCenterWorldPosition() - m_vrEyesTransform.position;
    }

    protected void HandleCameraControls()
    {
        if(Input.GetKeyDown(KeyCode.O))
        {
            m_xCameraLookSensitivity *= -1;
        }
        if(Input.GetKeyDown(KeyCode.P))
        {
            m_yCameraLookSensitivity *= -1;
        }

        Vector3 forward = Vector3.ProjectOnPlane(Camera.main.transform.forward, Vector3.up);
        forward.Normalize();
        Vector3 moveDir = forward * Input.GetAxis("Vertical") + Camera.main.transform.right*Input.GetAxis("Horizontal");
        if (Input.GetKey(KeyCode.Space))
        {
            moveDir.y += 1;
        }
        if (Input.GetKey(KeyCode.LeftControl))
        {
            moveDir.y -= 1;
        }

        Vector3 newPos = Camera.main.transform.position + moveDir * m_cameraMoveSpeed * Time.deltaTime;
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

    protected void HandlePlayerControls()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, float.MaxValue, LayerUtils.kDefaultCameraLayerMask))
            {
                StartInteraction(hit);
            }
            m_rotationMouseStart = Input.mousePosition;
        }
        else if (Input.GetMouseButton(0))
        {
            ContinueInteraction(Camera.main.ScreenPointToRay(Input.mousePosition));
        }
        else if (Input.GetMouseButtonUp(0))
        {
            EndInteraction();
        }
    }

    public void StartInteraction(RaycastHit hit)
    {
        if(m_isAutoCompleting || LevelManager.Instance.m_isTransitioning)
        {
            return;
        }

        LevelCell cell = hit.collider.GetComponentInParent<LevelCell>();
        if(cell == null)
        {
            return;
        }

        if (cell.m_data.m_type == LevelCellType.Handle && Vector3.Dot(hit.normal, cell.transform.up) > 0.5f)
        {
            Select(cell);
            
            m_rotationAxis = GetRotationAxis(m_selectedCell.transform.forward);
            if (m_rotationAxis == Vector3.up)
            {
                m_rotationGroup = m_selectedYLayer;
            }
            else if (m_rotationAxis == Vector3.right)
            {
                m_rotationGroup = m_selectedXLayer;
            }
            else
            {
                m_rotationGroup = m_selectedZLayer;
            }

            m_rotationGroup.SetColor(m_rotationHighlightColor);

            m_rotater = new GameObject("Rotater");
            m_rotater.transform.SetParent(transform);
            m_rotater.transform.position = LevelManager.Instance.m_activeLevel.GetCenterWorldPosition();
            m_rotater.transform.rotation = Quaternion.identity;

            m_interactionDistance = hit.distance;
            
            Vector3 center = Vector3.ProjectOnPlane(LevelManager.Instance.m_activeLevel.GetCenterWorldPosition(), m_rotationAxis);
            m_rotationStartDirProjected = Vector3.ProjectOnPlane(hit.point, m_rotationAxis) - center;
            m_rotationStartDirProjected.Normalize();

            m_rotationGroup.SetParent(m_rotater.transform);

            if (m_player.m_isMoving)
            {
                m_player.Stop();
            }
        }
        else
        {
            Vector3 destPos = hit.point;
            switch (cell.m_data.m_type)
            {
                case LevelCellType.Goal:
                case LevelCellType.Ramp:
                    destPos = cell.transform.position;
                    break;
                default:
                    destPos += hit.normal * LevelManager.Instance.m_activeLevel.m_palette.m_spacingSize * 0.5f;
                    break;
            }
            m_player.PathTo(destPos);
        }
    }

    public void ContinueInteraction(Ray ray)
    {
        if (m_selectedCell != null)
        {
            Vector3 center = Vector3.ProjectOnPlane(LevelManager.Instance.m_activeLevel.GetCenterWorldPosition(), m_rotationAxis);
            float dist = Mathf.Max(Vector3.Distance(ray.origin, m_selectedCell.transform.position) - m_selectedCell.transform.localScale.x * 0.5f, m_interactionDistance);
            Vector3 currProjected = Vector3.ProjectOnPlane(ray.GetPoint(dist), m_rotationAxis);
            currProjected -= center;
            currProjected.Normalize();
            m_rotater.transform.rotation = Quaternion.FromToRotation(m_rotationStartDirProjected, currProjected);
        }
    }

    public void ContinueInteraction(Ray lastRay, Ray currRay)
    {
        if (m_selectedCell != null)
        {
            RaycastHit lastHit, currHit;
            if(Physics.Raycast(lastRay, out lastHit, Mathf.Infinity, LayerUtils.kDefaultCameraLayerMask) && Physics.Raycast(currRay, out currHit, Mathf.Infinity, LayerUtils.kDefaultCameraLayerMask))
            {
                Vector3 center = Vector3.ProjectOnPlane(LevelManager.Instance.m_activeLevel.GetCenterWorldPosition(), m_rotationAxis);
                
                Vector3 lastProjected = Vector3.ProjectOnPlane(lastHit.point, m_rotationAxis) - center;
                Vector3 currProjected = Vector3.ProjectOnPlane(currHit.point, m_rotationAxis) - center;
                lastProjected.Normalize();
                currProjected.Normalize();

                Vector3 lastRight = Vector3.Cross(m_rotationAxis, lastProjected);

                Vector3 eulers = m_rotater.transform.eulerAngles;
                eulers += m_rotationAxis * Mathf.Sign(Vector3.Dot(lastRight, currProjected)) * Vector3.Angle(lastProjected, currProjected);
                m_rotater.transform.eulerAngles = eulers;
            }
        }
    }

    public void EndInteraction()
    {
        if (m_selectedCell != null)
        {
            Level.CellGroup rotatedCells = new Level.CellGroup();
            rotatedCells.AddRange(m_rotationGroup);
            GameObject rotater = m_rotater;
            Quaternion startRotation = rotater.transform.rotation;
            Quaternion finalRotation = Quaternion.identity;
            if (Quaternion.Angle(startRotation, Quaternion.identity) > kTransitionThreshold)
            {
                finalRotation = GetClosestValidRotation(startRotation);
            }

            m_isAutoCompleting = true;

            LeanTween.value(rotater.gameObject, 0.0f, 1.0f, m_autoCompleteSpeed).setEase(m_autoCompleteEase).setOnUpdate((float t) =>
            {
                rotater.transform.rotation = Quaternion.Slerp(startRotation, finalRotation, t);
                rotatedCells.SetColor(Color.Lerp(m_rotationHighlightColor, Color.white, t));
            }).setOnComplete(() =>
            {
                rotatedCells.SetParent(LevelManager.Instance.transform);
                rotatedCells.ResetColor();
                LevelManager.Instance.m_activeLevel.UpdateCells(rotatedCells);
                Destroy(m_rotater);

                m_player.m_desiredPosition = m_player.transform.position;
                m_player.m_desiredRotation = m_player.transform.rotation;
                m_player.m_gravity = -m_player.transform.up;

                m_isAutoCompleting = false;
            });
            
            ClearSelection();
        }
    }

    protected Vector3 GetRotationAxis(Vector3 matchVector, Vector3 exclusionVector = default(Vector3))
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

    protected Quaternion GetClosestValidRotation(Quaternion rotation)
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

    protected void ClearSelection()
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

    protected void Select(LevelCell cell)
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
