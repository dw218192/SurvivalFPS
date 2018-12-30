using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace SurvivalFPS.Core
{
    [Serializable]
    public class HeadBob
    {
        public enum HeadBobCallBackType { Horizontal, Vertical }

        [Serializable]
        public class HeadBobEvent
        {
            public float Time = 0.0f;
            public Action HeadBobCallBack = null;
            public HeadBobCallBackType Type = HeadBobCallBackType.Vertical;
        }

        [SerializeField] private AnimationCurve m_BobCurve = new AnimationCurve( new Keyframe(0.0f, 0.0f), new Keyframe(0.5f, 1.0f),
                                                                         new Keyframe(1.0f, 0.0f), new Keyframe(1.5f, -1.0f),
                                                                         new Keyframe(2.0f, 0.0f));

        //how big the bob is going to be horizontally
        [SerializeField] private float m_HorizontalMultiplier = 0.01f;
        //how big the bob is going to be vertically
        [SerializeField] private float m_VerticalMultiplier = 0.02f;
        //how many times faster the Y play head's speed will be moving than the X play head
        [SerializeField] private float m_VerticalHorizontalSpeedRatio = 2.0f;
        //how frequent the head bob is going to be
        [SerializeField] private float m_BaseInterval = 0.1f;
        //how much the bobing should be slowed down if the player's running
        [SerializeField] private float m_RunStepMultiplier = 0.75f;
        //how much the bobing should be slowed down if the player's crouch-walking
        [SerializeField] private float m_CrouchStepMultiplier = 0.5f;


        //internal variables
        private float m_PrevXPlayHead;
        private float m_PrevYPlayHead;
        private float m_XPlayHead;
        private float m_YPlayHead;
        private float m_CurveEndTime;
        private FirstPersonController m_Player;
        private List<HeadBobEvent> m_Events = new List<HeadBobEvent>();

        public void Init(FirstPersonController player)
        {
            m_Player = player;
            m_CurveEndTime = m_BobCurve[m_BobCurve.length - 1].time;

            m_PrevXPlayHead = 0.0f;
            m_PrevYPlayHead = 0.0f;

            m_XPlayHead = 0.0f;
            m_YPlayHead = 0.0f;
        }

        public void RegisterEvent(float curveTime, Action function, HeadBobCallBackType type)
        {
            HeadBobEvent headbobEvent = new HeadBobEvent();
            headbobEvent.Time = curveTime;
            headbobEvent.HeadBobCallBack = function;
            headbobEvent.Type = type;

            m_Events.Add(headbobEvent);
            m_Events.Sort((HeadBobEvent event1, HeadBobEvent event2) => { return event1.Time.CompareTo(event2.Time); });
        }

        public Vector3 GetLocalSpaceVectorOffset(float speed)
        {

            if(m_BaseInterval < float.Epsilon)
            {
                Debug.LogWarning("the headbob interval cannot be zero");
                return Vector3.zero;
            }

            if (m_Player)
            {
                if (m_Player.Running)
                {
                    m_XPlayHead += (speed * m_RunStepMultiplier * Time.deltaTime) / m_BaseInterval;
                    m_YPlayHead += ((speed * m_RunStepMultiplier * Time.deltaTime) / m_BaseInterval) * m_VerticalHorizontalSpeedRatio;
                }
                else if (m_Player.Crouching)
                {
                    m_XPlayHead += (speed * m_CrouchStepMultiplier * Time.deltaTime) / m_BaseInterval;
                    m_YPlayHead += ((speed * m_CrouchStepMultiplier * Time.deltaTime) / m_BaseInterval) * m_VerticalHorizontalSpeedRatio;
                }
                else
                {
                    m_XPlayHead += (speed * Time.deltaTime) / m_BaseInterval;
                    m_YPlayHead += ((speed * Time.deltaTime) / m_BaseInterval) * m_VerticalHorizontalSpeedRatio;
                }
            }

            //wrap back
            if (m_XPlayHead > m_CurveEndTime)
            {
                m_XPlayHead -= m_CurveEndTime;
            }

            if (m_YPlayHead > m_CurveEndTime)
            {
                m_YPlayHead -= m_CurveEndTime;
            }

            //event calls
            for(int i = 0; i < m_Events.Count; i++)
            {
                HeadBobEvent headBobEvent = m_Events[i];
                if(headBobEvent != null)
                {
                    if(headBobEvent.Type == HeadBobCallBackType.Vertical)
                    {
                        //if the event time is between the previous frame and the current frame
                        //or in case the time line has been wrapped around and the event time is still between the previous frame and the current frame
                        if ((m_PrevYPlayHead < headBobEvent.Time && m_YPlayHead > headBobEvent.Time)
                            || (m_PrevYPlayHead > m_YPlayHead && (headBobEvent.Time > m_PrevYPlayHead || headBobEvent.Time < m_YPlayHead))
                            )
                        {
                            headBobEvent.HeadBobCallBack();
                        }
                    }
                    else
                    {
                        //if the event time is between the previous frame and the current frame
                        //or in case the time line has been wrapped around and the event time is still between the previous frame and the current frame
                        if ((m_PrevXPlayHead < headBobEvent.Time && m_XPlayHead > headBobEvent.Time)
                            || (m_PrevXPlayHead > m_XPlayHead && (headBobEvent.Time > m_PrevXPlayHead || headBobEvent.Time < m_XPlayHead))
                            )
                        {
                            headBobEvent.HeadBobCallBack();
                        }
                    }
                }
            }


            float xPos = m_BobCurve.Evaluate(m_XPlayHead) * m_HorizontalMultiplier;
            float yPos = m_BobCurve.Evaluate(m_YPlayHead) * m_VerticalMultiplier;

            return new Vector3(xPos, yPos, 0.0f);
        }
    }
}