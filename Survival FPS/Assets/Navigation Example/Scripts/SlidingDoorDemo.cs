using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum DoorState { Open, Animating, Closed };

public class SlidingDoorDemo : MonoBehaviour
{
    public float SlidingDistance = 4.0f;
    public float Duration = 1.5f;
    public AnimationCurve OpeningCurve = new AnimationCurve();

    private Transform m_Transform = null;
    private Vector3 m_OpenPos = Vector3.zero;
    private Vector3 m_ClosedPos = Vector3.zero;
    private DoorState m_DoorState = DoorState.Closed;


	// Use this for initialization
	void Start ()
    {
        m_Transform = transform;
        m_ClosedPos = m_Transform.position;
        m_OpenPos = m_Transform.position + (m_Transform.forward * SlidingDistance);
	}
	
	// Update is called once per frame
	void Update ()
    {
		if(Input.GetKeyDown(KeyCode.Space) && m_DoorState != DoorState.Animating)
        {
            StartCoroutine(AnimateDoor(m_DoorState == DoorState.Open ? DoorState.Closed : DoorState.Open));
        }
	}

    IEnumerator AnimateDoor(DoorState newState)
    {
        if (m_DoorState == DoorState.Animating) yield break;
        m_DoorState = DoorState.Animating;

        float time = 0.0f;
        Vector3 startPos = newState == DoorState.Open ? m_ClosedPos : m_OpenPos;
        Vector3 endPos = newState == DoorState.Open ? m_OpenPos : m_ClosedPos; 

        while(time <= Duration)
        {
            float t = time / Duration;
            m_Transform.position = Vector3.Lerp(startPos, endPos, OpeningCurve.Evaluate(t));
            time += Time.deltaTime;
            yield return null;
        }

        m_Transform.position = endPos;
        m_DoorState = newState;
    }
}
