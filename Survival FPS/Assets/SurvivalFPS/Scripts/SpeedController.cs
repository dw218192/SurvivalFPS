using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeedController : MonoBehaviour
{
    public float Speed = 0.0f;
    private Animator m_Controller = null;

	// Use this for initialization
	void Start () {
        m_Controller.SetFloat("Speed", Speed);
	}
	
	// Update is called once per frame
	void Update () {
        m_Controller.SetFloat("Speed", Speed);
    }
}
