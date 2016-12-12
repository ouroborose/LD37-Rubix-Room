using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseActor : MonoBehaviour {
    public readonly static Vector3[] kUpAxisMovement =
    {
        Vector3.forward,
        Vector3.right,
        Vector3.back,
        Vector3.left,
    };

    public readonly static Vector3[] kRightAxisMovement =
    {
        Vector3.up,
        Vector3.forward,
        Vector3.down,
        Vector3.back,
    };

    public readonly static Vector3[] kForwardAxisMovement =
    {
        Vector3.up,
        Vector3.right,
        Vector3.down,
        Vector3.left,
    };

    public float m_movementTime = 0.5f;
    public float m_turnSpeed = 180.0f;
    public LevelCell m_groundCell;

    protected Coroutine m_pathFindingCoroutine;
    protected Stack<int> m_path = new Stack<int>();
    
    protected float m_moveTimer = 0.0f;
    protected Vector3 m_startPosition;
    protected Vector3 m_desiredPosition;
    protected Quaternion m_desiredRotation;

    protected virtual void Update()
    {
        if(m_moveTimer <= 0)
        {
            if (m_path.Count > 0)
            {
                m_startPosition = transform.position;
                m_desiredPosition = GetCellWorldPosition(m_path.Pop());
                m_desiredRotation = Quaternion.LookRotation((m_desiredPosition - m_startPosition).normalized, Vector3.up);
                m_moveTimer += m_movementTime;
            }
        }
        else
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, m_desiredRotation, Time.deltaTime * m_turnSpeed);
            m_moveTimer -= Time.deltaTime;
            float t = 1.0f - (m_moveTimer / m_movementTime);
            transform.position = Vector3.Lerp(m_startPosition, m_desiredPosition, t);
        }
        
    }

    public void UpdateGroundCell()
    {
        m_groundCell = GetCell(transform.position - transform.up);
        if (m_groundCell != null)
        {
            transform.parent = m_groundCell.transform;
        }
    }

    public LevelCell GetCell(Vector3 worldPos)
    {
        return LevelManager.Instance.m_activeLevel.GetCell(worldPos);
    }

    public LevelCell GetCell(int index)
    {
        return LevelManager.Instance.m_activeLevel.GetCell(index);
    }

    public Vector3 GetCellWorldPosition(int index)
    {
        return LevelManager.Instance.m_activeLevel.GetCellWorldPosition(index);
    }

    public int GetCellIndex(Vector3 worldPos)
    {
        return LevelManager.Instance.m_activeLevel.GetCellIndex(worldPos);
    }

    public int GetCellIndex(LevelCell cell)
    {
        return LevelManager.Instance.m_activeLevel.GetCellIndex(cell);
    }

    public void PathTo(Vector3 destWorldPos)
    {
        if(m_pathFindingCoroutine != null)
        {
            StopCoroutine(m_pathFindingCoroutine);
        }
        m_pathFindingCoroutine = StartCoroutine(CalculatePath(destWorldPos));
    }

    public void ClearPath()
    {
        m_path.Clear();
    }

    public IEnumerator CalculatePath(Vector3 destWorldPos)
    {
        ClearPath();

        int startIndex = GetCellIndex(transform.position);
        int destIndex = GetCellIndex(destWorldPos);
        if (startIndex == destIndex)
        {
            yield break;
        }

        //Debug.LogFormat("Pathing to {0}", destWorldPos);
        HashSet<int> closedSet = new HashSet<int>();
        HashSet<int> openSet = new HashSet<int>();
        Dictionary<int, int> cameFrom = new Dictionary<int, int>();
        Dictionary<int, float> gScores = new Dictionary<int, float>();
        Dictionary<int, float> fScores = new Dictionary<int, float>();

        int currIndex = startIndex;
        gScores[currIndex] = 0;
        fScores[currIndex] = Vector3.SqrMagnitude(destWorldPos - transform.position);
        openSet.Add(currIndex);
        Vector3 currPos = transform.position;
        Vector3 currGravity = -transform.up;

        while (openSet.Count > 0)
        {
            if (Input.GetKey(KeyCode.Escape))
            {
                break;
            }

            float best = float.MaxValue;
            foreach (int index in openSet)
            {
                float fScore = fScores[index];
                if(fScore < best)
                {
                    best = fScore;
                    currIndex = index;
                }
            }

            openSet.Remove(currIndex);

            if(currIndex == destIndex)
            {
                // Reach dest! build the path
                while(currIndex != startIndex)
                {
                    m_path.Push(currIndex);
                    Debug.DrawLine(GetCellWorldPosition(currIndex), GetCellWorldPosition(cameFrom[currIndex]), Color.green, 2.0f);
                    currIndex = cameFrom[currIndex];
                }
                
                yield break;
            }

            currPos = GetCellWorldPosition(currIndex);
            closedSet.Add(currIndex);

            List<int> possibleMoves = GetPossibleMoves(currPos, currGravity, closedSet);
            for(int i = 0, n = possibleMoves.Count; i < n; ++i)
            {
                int moveIndex = possibleMoves[i];
                float moveGScore = gScores[currIndex] + 1;
                if(!openSet.Contains(moveIndex))
                {
                    openSet.Add(moveIndex);
                }
                else if(moveGScore >= gScores[moveIndex])
                {
                    continue;
                }

                cameFrom[moveIndex] = currIndex;
                gScores[moveIndex] = moveGScore;
                fScores[moveIndex] = moveGScore + Vector3.SqrMagnitude(destWorldPos - GetCellWorldPosition(moveIndex));
            }

            yield return new WaitForEndOfFrame();
        }
    }

    public int GetBestMove(List<int> possibleMoves, Vector3 dest)
    {
        float best = float.MaxValue;
        int bestMove = -1;
        for(int i = 0, n = possibleMoves.Count; i < n; ++i)
        {
            int index = possibleMoves[i];
            LevelCell cell = GetCell(index);
            if(cell == null || cell.gameObject.layer != 0)
            {
                Vector3 pos = GetCellWorldPosition(possibleMoves[i]);
                float dist = Vector3.SqrMagnitude(dest - pos);
                if (dist < best)
                {
                    best = dist;
                    bestMove = index;
                }
            }
            else
            {
                // handle ramp case
            }
        }
        return bestMove;
    }
    
    public List<int> GetPossibleMoves(Vector3 worldPos, Vector3 gravityDir, HashSet<int> visited)
    {
        Vector3[] dirs;
        if(gravityDir.x != 0)
        {
            dirs = kRightAxisMovement;
        }
        else if (gravityDir.y != 0)
        {
            dirs = kUpAxisMovement;
        }
        else
        {
            dirs = kForwardAxisMovement;
        }

        List<int> possibleMoves = new List<int>();
        for(int i = 0; i < 4; ++i)
        {
            Vector3 dir = dirs[i];
            Vector3 checkPos = worldPos + dir;
            int index = GetCellIndex(checkPos);
            if(visited.Contains(index))
            {
                continue;
            }

            LevelCell cell = GetCell(checkPos);
            if (cell != null)
            {
                switch (cell.m_data.m_type)
                {
                    case LevelCellType.Solid:
                        continue;
                    case LevelCellType.Ramp:
                        // determine if we can move up on the ramp from here
                        break;
                }
            }

            LevelCell below = GetCell(checkPos + gravityDir);
            if(below != null)
            {
                if (below.m_data.m_type == LevelCellType.Solid)
                {
                    possibleMoves.Add(index);
                }
                else if (below.m_data.m_type == LevelCellType.Ramp)
                {
                    // determine if we can move down on the ramp
                }
            }
        }

        return possibleMoves;
    }
}
