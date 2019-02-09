using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using SurvivalFPS.Utility;

namespace SurvivalFPS.Core.UI
{
    [ExecuteInEditMode]
    [DisallowMultipleComponent]
    public class MenuConfigurator : MonoBehaviour
    {
        [Serializable]
        private class MenuItem
        {
            public string Name;
            public UnityEvent ItemEvent;
        }

        private List<Selectable> m_Selectables = new List<Selectable>();
        private int m_PrevChildCnt;

        private void Update()
        {
            /*
            if(Application.isEditor)
            {
                if(transform.childCount != m_PrevChildCnt)
                {
                    Selectable[] arr = GetComponentsInChildren<Selectable>();
                    List<Selectable> selectables = new List<Selectable>(arr);
                }

                m_PrevChildCnt = transform.childCount;
            }
            */
        }

        private int MinEditDistance(List<char> x, List<char> y)
        {
            // define table[i,j] to be the min cost to ransform 
            // x[i],x[i+1],..,x[n] to y[j],y[j+1],...,y[m]
            // allocate a table
            int n = x.Count - 1;
            int m = y.Count - 1;
            int[,] table = new int[x.Count+1, y.Count+1];

            //initialize base cases
            for (int i = 0; i <= n + 1; i ++)
            {
                table[i, m + 1] = n - i + 1;
            }
            for (int j = 0; j <= m + 1; j ++)
            {
                table[n + 1, j] = m - j + 1;
            }

            for (int i = n; i >= 0; i --)
            {
                //calculate table[i, j]
                for (int j = m; j >= 0; j --)
                {
                    if (x[i] == y[j])
                    {
                        table[i, j] = table[i + 1, j + 1];
                    }
                    else
                    {
                        table[i, j] = 1 + Mathf.Min(table[i + 1, j], table[i, j + 1], table[i + 1, j + 1]);
                    }
                }
            }

            return table[0, 0];
        }
    }
}