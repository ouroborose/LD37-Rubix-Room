using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelCell : MonoBehaviour {
    public const float kTransitionTime = 0.25f;
    public const LeanTweenType kTransitionInEaseType = LeanTweenType.easeSpring;
    public const LeanTweenType kTransitionOutEaseType = LeanTweenType.easeInOutSine;

    public LevelCellData m_data;

    [HideInInspector]
    public bool m_isMarkedForDeletion = false;

    protected Vector3 m_originalScale;
    protected Renderer[] m_renderers;
    protected Color[] m_originalColors;

    protected void Awake()
    {
        m_renderers = GetComponentsInChildren<Renderer>();
        m_originalColors = new Color[m_renderers.Length];

        for(int i = 0, n = m_renderers.Length; i < n; ++i)
        {
            m_originalColors[i] = m_renderers[i].material.color;
        }
    }

    protected void OnDestroy()
    {
        LeanTween.cancel(gameObject, false);
    }

    public void SetColor(Color color)
    {
        for (int i = 0, n = m_renderers.Length; i < n; ++i)
        {
            m_renderers[i].material.color = color;
        }
    }

    public void ResetColor()
    {
        for (int i = 0, n = m_renderers.Length; i < n; ++i)
        {
            m_renderers[i].material.color = m_originalColors[i];
        }
    }

    public void Init(int x, int y, int z)
    {
        m_data.m_x = x;
        m_data.m_y = y;
        m_data.m_z = z;
        m_originalScale = transform.localScale;
        OnTransitionUpdate(0.0f);
        gameObject.SetActive(false);
    }

    public void UpdatePoseData(float spacing)
    {
        m_data.m_x = Mathf.RoundToInt(transform.position.x / spacing);
        m_data.m_y = Mathf.RoundToInt(transform.position.y / spacing);
        m_data.m_z = Mathf.RoundToInt(transform.position.z / spacing);
        m_data.m_rotationId = RotationUtil.GetClosestId(transform.rotation);
    }

    public void Show(float delay = 0.0f)
    {
        gameObject.SetActive(true);
        LeanTween.value(0.0f, 1.0f, kTransitionTime).setDelay(delay).setEase(kTransitionInEaseType).setOnUpdate(OnTransitionUpdate);
    }

    public void Hide(float delay = 0.0f)
    {
        LeanTween.value(1.0f, 0.0f, kTransitionTime).setDelay(delay).setEase(kTransitionOutEaseType).setOnUpdate(OnTransitionUpdate).setOnComplete(OnHideComplete);
    }

    public void OnTransitionUpdate(float t)
    {
        transform.localScale = m_originalScale * t;
    }

    public void OnHideComplete()
    {
        if(m_isMarkedForDeletion)
        {
            Destroy(gameObject);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    public override string ToString()
    {
        return string.Format("{0},{1},{2} - {3} - {4}", m_data.m_x, m_data.m_y, m_data.m_z, m_data.m_type, m_data.m_rotationId);
    }
}
