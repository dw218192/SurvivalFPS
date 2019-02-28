using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

namespace SurvivalFPS.Core.UI
{
    [CustomEditor(typeof(GameMenu), true)]
    public class MenuEditor : Editor
    {
        private bool m_AddButton = false;
        private bool m_UpdateUI = false;
        //a list of serialized button object that the menu references
        private List<SerializedObject> m_SerializedSelectables = new List<SerializedObject>();

        //serialized list of buttons in the menu object
        private SerializedProperty m_ButtonList;

        private GameMenu m_GameMenu;

        private void OnEnable()
        {
            m_GameMenu = (GameMenu)target;

            serializedObject.Update();
            m_ButtonList = serializedObject.FindProperty("m_Buttons");

            if (m_ButtonList.arraySize > 0)
            {
                for (int i = 0; i < m_ButtonList.arraySize; i++)
                {
                    Button button = (Button)m_ButtonList.GetArrayElementAtIndex(i).objectReferenceValue;
                    m_SerializedSelectables.Add(button == null ? null : new SerializedObject(button));
                }
            }

            ScanExistingUIElements();
            serializedObject.ApplyModifiedProperties();
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            serializedObject.Update();

            m_UpdateUI = GUILayout.Button("Update UI");
            m_AddButton = GUILayout.Button("Add UI Button");

            EditorGUILayout.LabelField("Current UI Items: ");
            EditorGUI.indentLevel++;
            EditorGUILayout.LabelField("Buttons: ");
            EditorGUI.indentLevel++;

            //check the current status of UI selectables
            if (m_UpdateUI)
            {
                ScanExistingUIElements();

                Debug.Log("array size: " + m_ButtonList.arraySize + ", serialize size:" + m_SerializedSelectables.Count);
            }

            //add a button
            if (m_AddButton)
            {
                AddButton();
            }

            int indexMarkedForDelete = -1;
            for (int i = 0; i < m_ButtonList.arraySize; i++)
            {
                Button button = (Button)m_ButtonList.GetArrayElementAtIndex(i).objectReferenceValue;

                if (button)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(button.gameObject.name);
                    if( GUILayout.Button("Delete Button") )
                    {
                        indexMarkedForDelete = i;
                    }
                    EditorGUILayout.EndHorizontal();

                    //naming field
                    string newName = EditorGUILayout.TextField("Button Name: ", button.gameObject.name);
                    if (newName != button.gameObject.name)
                    {
                        SetButtonName(button, newName);
                    }

                    //on click event field
                    m_SerializedSelectables[i].Update();
                    EditorGUILayout.PropertyField(m_SerializedSelectables[i].FindProperty("m_OnClick"));
                }
            }

            if(indexMarkedForDelete != -1)
            {
                RemoveButton(indexMarkedForDelete);
            }

            //apply changes
            for (int i = 0; i < m_SerializedSelectables.Count; i++)
            {
                if(m_SerializedSelectables[i] != null)
                {
                    m_SerializedSelectables[i].ApplyModifiedProperties();
                }
            }
            serializedObject.ApplyModifiedProperties();
        }

        private void ScanExistingUIElements()
        {
            Selectable[] selectables = m_GameMenu.transform.GetComponentsInChildren<Selectable>();
          
            if(selectables != null)
            {
                //truncate the list if something is deleted
                if (selectables.Length < m_ButtonList.arraySize)
                {
                    int deletion = m_ButtonList.arraySize - selectables.Length;

                    for (int i = 0; i < deletion; i++)
                    {
                        RemoveArrayElement(m_ButtonList, m_ButtonList.arraySize - 1);

                        m_SerializedSelectables.RemoveAt(m_SerializedSelectables.Count - 1);
                    }
                }

                //modify the two lists to match the actual hierarchy
                for (int i = 0; i < selectables.Length; i++)
                {
                    //push back if needed
                    if (i > m_ButtonList.arraySize - 1)
                    {
                        AddButtonHelper((Button)selectables[i], m_ButtonList.arraySize, true);
                        continue;
                    }

                    //an item in the list has been modified
                    Object referenceVal = m_ButtonList.GetArrayElementAtIndex(i).objectReferenceValue;
                    if (selectables[i] != referenceVal)
                    {
                        AddButtonHelper((Button)selectables[i], i, false);
                    }
                }
            }
        }

        private void AddButton()
        {
            string defaultName;

            //create a button object and parent it to the menu
            GameObject buttonHolder = new GameObject();
            Button newButton = buttonHolder.AddComponent<Button>();
            Image buttonImage = buttonHolder.AddComponent<Image>();
            newButton.targetGraphic = buttonImage;
            buttonHolder.transform.SetParent(m_GameMenu.transform);

            int indexAvailable = FindAvailableIndex();

            //modify the inspected game object's reference list
            //push back if the array is filled

            if (indexAvailable == -1)
            {
                //modify the menu's button list
                AddButtonHelper(newButton, m_ButtonList.arraySize, true);

                defaultName = "Button " + (m_ButtonList.arraySize);
            }
            else
            {
                AddButtonHelper(newButton, indexAvailable, false);

                defaultName = "Button " + (indexAvailable + 1);
            }

            //set its default name
            SetButtonName(newButton, defaultName);
        }

        //adds the button to the inspected menu's button array
        //and creates a serialized object
        private void AddButtonHelper(Button newButton, int index, bool insert)
        {
            SerializedObject serializedButton = new SerializedObject(newButton);

            //insert
            if(insert)
            {
                InsertArrayElement(m_ButtonList, index, newButton);

                if(index == m_SerializedSelectables.Count)
                {
                    m_SerializedSelectables.Add(serializedButton);
                }
                else
                {
                    m_SerializedSelectables.Insert(index, serializedButton);
                }
            }
            //replace
            else
            {
                m_ButtonList.GetArrayElementAtIndex(index).objectReferenceValue = newButton;
                m_SerializedSelectables[index] = serializedButton;
            }
        }

        private void RemoveButton(int index)
        {
            Button button = (Button) m_ButtonList.GetArrayElementAtIndex(index).objectReferenceValue;

            RemoveArrayElement(m_ButtonList, index);
            m_SerializedSelectables.RemoveAt(index);

            //destroy game object
            DestroyImmediate(button.gameObject);
        }

        private void RemoveArrayElement(SerializedProperty array, int index)
        {
            if (index > array.arraySize - 1 || index < 0)
            {
                Debug.LogWarning("Menu Editor: RemoveArrayElement- index out of bounds");
                return;
            }

            //if it's not simply pop back
            if (index != array.arraySize - 1)
            {
                //shift the list elements to the specified index
                for (int i = index + 1; i <= array.arraySize - 1; i++)
                {
                    array.GetArrayElementAtIndex(i - 1).objectReferenceValue = array.GetArrayElementAtIndex(i).objectReferenceValue;
                }
            }

            array.arraySize--;
        }

        private void InsertArrayElement(SerializedProperty array, int index, Object value)
        {
            if (index > array.arraySize || index < 0)
            {
                Debug.LogWarning("Menu Editor: InsertArrayElement- index out of bounds");
                return;
            }

            array.arraySize++;

            //if it's not simply push back
            if (index != array.arraySize - 1)
            {
                //shift the list elements after the specified index
                for (int i = index + 1; i <= array.arraySize - 1; i++)
                {
                    array.GetArrayElementAtIndex(i).objectReferenceValue = array.GetArrayElementAtIndex(i - 1).objectReferenceValue;
                }
            }

            array.GetArrayElementAtIndex(index).objectReferenceValue = value;
        }

        private int FindAvailableIndex()
        {
            //find an availbale spot in the array
            int indexAvailable = -1;
            for (int i = 0; i < m_ButtonList.arraySize; i++)
            {
                if (!m_ButtonList.GetArrayElementAtIndex(i).objectReferenceValue)
                {
                    indexAvailable = i;
                    break;
                }
            }

            return indexAvailable;
        }

        private void SetButtonName(Button button, string newName)
        {
            //add text object
            Text text = button.GetComponentInChildren<Text>();
            //if this button does not have a text component yet
            if(!text)
            {
                GameObject textHolder = new GameObject(newName + "Text");
                textHolder.transform.SetParent(button.gameObject.transform);
                text = textHolder.AddComponent<Text>();
            }
            //change the text
            text.text = newName;

            //set button game object name
            button.gameObject.name = newName;
        }


        /*
        private class EditInfo
        {
            public enum EditType { None, Remove, Replace, Insert }

            public int EditCost = -1;
            public EditInfo Predecessor = null; //used in backtrace
            public EditType Operation = EditType.None;
        }
        // define table[i,j] to be the min cost to transform 
        // x[i],x[i+1],..,x[n] to y[j],y[j+1],...,y[m]
        private EditInfo[,] m_Table;
        /// <summary>
        /// Finds the minimum edit distance. Returns true if the two lists are the same.
        /// </summary>
        private bool FindMinEditDistance(List<Selectable> x, List<Selectable> y)
        {
            if (x == null && y == null) return true;
            if (x == null || y == null) return false;

            int n = x.Count - 1;
            int m = y.Count - 1;
            m_Table = new EditInfo[x.Count + 1, y.Count + 1];

            //initialize base cases
            for (int i = 0; i <= n + 1; i++)
            {
                m_Table[i, m + 1].EditCost = n - i + 1;
            }
            for (int j = 0; j <= m + 1; j++)
            {
                m_Table[n + 1, j].EditCost = m - j + 1;
            }

            for (int i = n; i >= 0; i--)
            {
                //calculate table[i, j]
                for (int j = m; j >= 0; j--)
                {
                    if (x[i] == y[j])
                    {
                        m_Table[i, j].Predecessor = m_Table[i + 1, j + 1];
                        m_Table[i, j].EditCost = m_Table[i + 1, j + 1].EditCost;
                    }
                    else
                    {
                        EditInfo[] temp = { m_Table[i + 1, j], m_Table[i, j + 1], m_Table[i + 1, j + 1] };
                        int minIndex = 0;
                        for (int k = 1; k < 3; k ++)
                        {
                            if(temp[i].EditCost < temp[minIndex].EditCost)
                            {
                                minIndex = k;
                            }
                        }

                        m_Table[i, j].Predecessor = temp[minIndex];
                        m_Table[i, j].EditCost = 1 + temp[minIndex].EditCost;
                    }
                }
            }

            return m_Table[0, 0].EditCost == 0;
        }

        private void TransformList(List<Selectable> x, List<Selectable> y)
        {
            EditInfo info = m_Table[0, 0];
            while (info != null)
            {
                if(info.Operation != EditInfo.EditType.None)
                {
                    switch(info.Operation)
                    {
                        case EditInfo.EditType.Insert:
                            {
                                break;
                            }
                        case EditInfo.EditType.Remove:
                            break;
                        case EditInfo.EditType.Replace:
                            break;
                    }
                }
                info = info.Predecessor;
            }
        }
        */
    }
}
