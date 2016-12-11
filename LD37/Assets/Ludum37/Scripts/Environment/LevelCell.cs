using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelCell : MonoBehaviour {
    public const float kTransitionTime = 0.25f;
    public const LeanTweenType kTransitionInEaseType = LeanTweenType.easeSpring;
    public const LeanTweenType kTransitionOutEaseType = LeanTweenType.easeInOutSine;

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

    public bool m_isMarkedForDeletion = false;

    protected Vector3 m_originalScale;

    public void Init(int x, int y, int z)
    {
        m_x = x;
        m_y = y;
        m_z = z;
        m_originalScale = transform.localScale;
        OnTransitionUpdate(0.0f);
        gameObject.SetActive(false);
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
        return string.Format("{0},{1},{2} - {3}", m_x, m_y, m_z, m_type);
    }
}
