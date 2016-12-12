using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spin : MonoBehaviour {
    public float m_rotationRate = 90.0f;
    public Space m_space = Space.Self;
    protected void Update()
    {
        transform.Rotate(Vector3.up, m_rotationRate * Time.deltaTime, m_space);
    }
}
