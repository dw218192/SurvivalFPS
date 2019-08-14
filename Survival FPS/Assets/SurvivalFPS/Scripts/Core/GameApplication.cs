using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;

using UnityEngine;
using SurvivalFPS.Utility;

namespace SurvivalFPS
{
    public class GameApplication : SingletonBehaviour<GameApplication>
    {
        protected override void Awake()
        {
            base.Awake();
            //load assets
        }

        public class LayerData : IEquatable<LayerData>
        {
            private const uint NUM_LAYERS = 32u;
            private static BitVector32 m_IncludedLayers = new BitVector32(0);

            static LayerData()
            {
                for(int i=0; i<NUM_LAYERS; i++)
                {
                    string name = LayerMask.LayerToName(i);
                    if(name != "" && !m_IncludedLayers[1 << i])
                    {
                        Debug.LogWarningFormat("layer {0} is defined in this project but not defined in GameApplication::LayerData!", name);
                    }
                }
            }

            private int intVal = -1;

            public static readonly LayerData invalidLayer = new LayerData();
            public static readonly LayerData defaultLayer = GetLayer("Default");
            public static readonly LayerData transparentFXLayer = GetLayer("TransparentFX");
			public static readonly LayerData ignoreRaycastLayer = GetLayer("Ignore Raycast");
			public static readonly LayerData waterLayer = GetLayer("Water");
			public static readonly LayerData uILayer = GetLayer("UI");
			public static readonly LayerData postProcessingLayer = GetLayer("PostProcessing");
			public static readonly LayerData aIEntityLayer = GetLayer("AI Entity");
			public static readonly LayerData aIEntityTriggerLayer = GetLayer("AI Entity Trigger");
			public static readonly LayerData aITriggerLayer = GetLayer("AI Trigger");
			public static readonly LayerData playerLayer = GetLayer("Player");
			public static readonly LayerData visualAggravatorLayer = GetLayer("Visual Aggravator");
			public static readonly LayerData audioAggravatorLayer = GetLayer("Audio Aggravator");
			public static readonly LayerData aIBodyPartLayer = GetLayer("AI Body Part");
			public static readonly LayerData obstacleLayer = GetLayer("Obstacle");
			public static readonly LayerData interactiveLayer = GetLayer("Interactive Items");

            private static LayerData GetLayer(string layerName)
            {
                LayerData ret = new LayerData { intVal = LayerMask.NameToLayer(layerName) };
                if(ret == LayerData.invalidLayer)
                {
                    Debug.LogWarningFormat("layer with name {0} is not defined in this project!", layerName);
                }
                else
                {
                    if(m_IncludedLayers[1 << ret.intVal])
                    {
                        Debug.LogWarningFormat("layer with name {0} and index {1} is defined in GameApplication::LayerData twice!", layerName, ret.intVal);
                    }
                    else
                    {
                        m_IncludedLayers[1 << ret.intVal] = true;
                    }
                }

                return ret;
            }

            public static LayerMask[] GetCollisionMatrix()
            {
                LayerMask[] ret = new LayerMask[NUM_LAYERS];

                for(int i=0; i<NUM_LAYERS; i++)
                {
                    LayerMask layerMask = default(LayerMask);
                    for (int j = 0; j < NUM_LAYERS; j++)
                    {
                        if(!Physics.GetIgnoreLayerCollision(i,j))
                        {
                            layerMask |= 1 << j;
                        }
                        ret[i] = layerMask;
                    }
                }

                return ret;
            }
            
            public bool Equals(LayerData other)
            {
                return intVal == other.intVal;
            }

            public override bool Equals(object obj)
            {
                if (obj == null) return false;
                if (obj.GetType() != GetType()) return false;
                return Equals(obj as LayerData);
            }

            public override int GetHashCode()
            {
                return base.GetHashCode();
            }

            public override string ToString()
            {
                return string.Format("[{0},{1}]", intVal == -1 ? "Invalid" : LayerMask.LayerToName(intVal), intVal);
            }

            public static bool operator!=(LayerData ld1, LayerData ld2)
            {
                return !ld1.Equals(ld2);
            }

            public static bool operator ==(LayerData ld1, LayerData ld2)
            {
                return ld1.Equals(ld2);
            }

            public static implicit operator int(LayerData layerData)
            {
                return layerData.intVal;
            }
        }
    }
}
