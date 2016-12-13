using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Indicator : MonoBehaviour {

    public const float kTransitionTime = 0.25f;
    public const LeanTweenType kTransitionInEaseType = LeanTweenType.easeSpring;
    public const LeanTweenType kTransitionOutEaseType = LeanTweenType.easeInOutSine;

    public const float kPulseTime = 1.0f;
    public const LeanTweenType kPulseEaseType = LeanTweenType.easeInOutSine;

    public const float kColorTransitionTime = 0.5f;

    protected Vector3 m_originalScale;
    protected Renderer[] m_renderers;
    protected Color[] m_originalColors;

    protected Color m_fromColor;
    protected Color m_toColor;

    protected void Awake()
    {
        m_renderers = GetComponentsInChildren<Renderer>();
        m_originalColors = new Color[m_renderers.Length];

        for (int i = 0, n = m_renderers.Length; i < n; ++i)
        {
            m_originalColors[i] = m_renderers[i].material.color;
        }
        m_toColor = m_originalColors[0];
        m_originalScale = transform.localScale;
        gameObject.SetActive(false);
    }

    public void SetColor(Color color)
    {
        m_fromColor = m_toColor;
        m_toColor = color;
        LeanTween.value(0.0f, 1.0f, kColorTransitionTime).setEase(kTransitionInEaseType).setOnUpdate(OnSetColorUpdate);
    }

    public void OnSetColorUpdate(float t)
    {
        SetColorInstant(Color.Lerp(m_fromColor, m_toColor, t));
    }

    public void SetColorInstant(Color color)
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
        m_toColor = m_originalColors[0];
    }

    public void Show(float delay = 0.0f)
    {
        LeanTween.cancel(gameObject);
        gameObject.SetActive(true);
        ResetColor();
        transform.localScale = Vector3.zero;
        LeanTween.scale(gameObject, m_originalScale, kTransitionTime).setDelay(delay).setEase(kTransitionInEaseType).setOnComplete(Pulse);
    }

    public void Hide(float delay = 0.0f)
    {
        LeanTween.cancel(gameObject);
        LeanTween.scale(gameObject, Vector3.zero, kTransitionTime).setDelay(delay).setEase(kTransitionOutEaseType).setOnComplete(OnHideComplete);
    }

    public void Pulse()
    {
        gameObject.SetActive(true);
        LeanTween.scale(gameObject, m_originalScale * 1.33f, kPulseTime).setLoopPingPong(-1).setEase(kPulseEaseType);
    }

    public void OnHideComplete()
    {
        transform.localScale = Vector3.zero;
        gameObject.SetActive(false);
    }
}
