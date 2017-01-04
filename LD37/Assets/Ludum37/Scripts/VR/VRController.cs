using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRController : MonoBehaviour {
    public Indicator m_indicator;
    public SteamVR_TrackedObject m_controllerTrackedObject;

    protected Ray m_targetingRay = new Ray();
    protected RaycastHit m_targetingHit;
    public RaycastHit TargetingHit { get { return m_targetingHit; } }

    public bool Exists { get { return m_controllerTrackedObject.index != SteamVR_TrackedObject.EIndex.None; } }
    public SteamVR_Controller.Device Input { get { return SteamVR_Controller.Input((int)m_controllerTrackedObject.index); } }

    protected Vector3 m_localGrabPoint;

    protected void Awake()
    {
        if(m_controllerTrackedObject == null)
        {
            m_controllerTrackedObject = GetComponent<SteamVR_TrackedObject>();
        }
    }

    protected void Start()
    {
        m_indicator.Show();
    }

    protected void Update()
    {
        if (!Exists)
        {
            return;
        }

        m_targetingRay.origin = transform.position;
        m_targetingRay.direction = transform.forward;
        
        if(Physics.Raycast(m_targetingRay, out m_targetingHit, Mathf.Infinity, LayerUtils.kDefaultCameraLayerMask))
        {
            m_indicator.transform.position = m_targetingHit.point;

            if (Input.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
            {
                Game.Instance.StartInteraction(m_targetingHit);
                if (Game.Instance.SelectedCell != null)
                {
                    m_localGrabPoint = Game.Instance.SelectedCell.transform.InverseTransformPoint(m_targetingHit.point);
                }
            }
            else if(Input.GetPress(SteamVR_Controller.ButtonMask.Trigger))
            {
                Game.Instance.ContinueInteraction(m_targetingRay);
                if(Game.Instance.SelectedCell != null)
                {
                    m_indicator.transform.position = Game.Instance.SelectedCell.transform.TransformPoint(m_localGrabPoint);
                }
            }
            else if (Input.GetPressUp(SteamVR_Controller.ButtonMask.Trigger))
            {
                Game.Instance.EndInteraction();
            }
        }

        if(Input.GetPressDown(SteamVR_Controller.ButtonMask.ApplicationMenu))
        {
            Game.Instance.RecenterVRPlaySpace();
        }
    }
}
