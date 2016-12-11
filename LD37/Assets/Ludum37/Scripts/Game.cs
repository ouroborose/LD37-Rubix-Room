using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game : MonoBehaviour
{
    public readonly Quaternion[] kValidRotations =
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

    public readonly Vector3[] kRotationAxes =
    {
        Vector3.up,
        Vector3.right,
        Vector3.forward,
    };

    public float m_moveSpeed = 1.0f;

    public float m_xLookSensitivity = 10.0f;
    public float m_yLookSensitivity = 10.0f;

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

    protected void Start()
    {
        Camera.main.transform.position = LevelManager.Instance.m_activeLevel.GetCenterWorldPosition();
        m_lastMousePos = Input.mousePosition;
    }

    protected void Update()
    {
        if(!m_isAutoCompleting)
        {
            HandleRotationControls();
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
        Camera.main.transform.position += Camera.main.transform.TransformDirection(moveDir) * m_moveSpeed * Time.deltaTime;

        if (Input.GetMouseButton(1))
        {
            Vector3 delta = Input.mousePosition - m_lastMousePos;
            
            m_cameraEulers.y += -delta.x * m_xLookSensitivity * Time.deltaTime;
            m_cameraEulers.x += delta.y * m_yLookSensitivity * Time.deltaTime;
            m_cameraEulers.x = Mathf.Clamp(m_cameraEulers.x, -89, 89);
            Camera.main.transform.eulerAngles = m_cameraEulers;
        }
    }

    public void HandleRotationControls()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                LevelCell cell = hit.collider.GetComponent<LevelCell>();
                if (cell != m_selectedCell)
                {
                    //Debug.Log(cell.ToString());
                    Select(cell);

                    m_rotationHitNormal = hit.normal;
                    m_rotationMouseStart = Input.mousePosition;
                    m_rotationStarted = false;
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
                    if (deltaMouse.sqrMagnitude > 25)
                    {

                        m_rotater = new GameObject("Rotater");
                        m_rotater.transform.SetParent(transform);
                        m_rotater.transform.position = LevelManager.Instance.m_activeLevel.GetCenterWorldPosition();
                        m_rotater.transform.rotation = Quaternion.identity;

                        Ray startRay = Camera.main.ScreenPointToRay(m_rotationMouseStart);
                        Ray currRay = Camera.main.ScreenPointToRay(Input.mousePosition);
                        RaycastHit startHit, currHit;
                        if (Physics.Raycast(startRay, out startHit) && Physics.Raycast(currRay, out currHit))
                        {
                            Vector3 deltaDir = currHit.point - startHit.point;
                            deltaDir.Normalize();
                            m_rotationAxis = GetRotationAxis(Vector3.Cross(startHit.normal, deltaDir));
                        }

                        /*
                        if (Mathf.Abs(deltaMouse.x) > Mathf.Abs(deltaMouse.y))
                        {
                            m_useMouseY = false;
                            m_rotationAxis = GetRotationAxis(Camera.main.transform.up, GetRotationAxis(m_rotationHitNormal));
                        }
                        else
                        {
                            m_useMouseY = true;
                            m_rotationAxis = GetRotationAxis(Camera.main.transform.right, GetRotationAxis(m_rotationHitNormal));
                        }
                        */
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

                        m_rotationGroup.SetColor(Color.green);
                        m_rotationGroup.SetParent(m_rotater.transform);

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
                    m_isAutoCompleting = false;
                });
                ClearSelection();
            }
            m_rotationStarted = false;
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

        m_selectedXLayer.AddRange(LevelManager.Instance.m_activeLevel.m_xLayers[cell.m_x]);
        m_selectedYLayer.AddRange(LevelManager.Instance.m_activeLevel.m_yLayers[cell.m_y]);
        m_selectedZLayer.AddRange(LevelManager.Instance.m_activeLevel.m_zLayers[cell.m_z]);

        /*
        m_selectedXLayer.SetColor(Color.blue);
        m_selectedYLayer.SetColor(Color.red);
        m_selectedZLayer.SetColor(Color.green);
        cell.SetColor(Color.yellow);
        */
    }
}
