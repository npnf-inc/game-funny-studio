using UnityEngine;
using System.Collections;
using UnityEditor;
using NPNF.UI;
using UnityEngine.UI;

[CustomEditor(typeof(ScrollPanel))]
public class ScrollPanelEditor : Editor
{
    private ScrollViewDataProvider mDataProvider;
    private ScrollViewLayoutProvider mLayoutProvider;
    private ScrollRect mScrollRect;

    public void OnEnable()
    {
        ScrollPanel scrollPanel = (ScrollPanel)target;
        mDataProvider = scrollPanel.gameObject.GetComponent<ScrollViewDataProvider>();
        mLayoutProvider = scrollPanel.gameObject.GetComponent<ScrollViewLayoutProvider>();
        mScrollRect = scrollPanel.GetComponentInChildren<ScrollRect>();
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        ScrollPanel scrollPanel = (ScrollPanel)target;
        
        // View Type
        scrollPanel.m_ViewType = (ScrollPanel.ViewType) EditorGUILayout.EnumPopup("View Type", scrollPanel.m_ViewType);

        // Layout Parameters
        SerializedProperty prop = null;
        bool scrollRectVerticalFlag = false;
        bool scrollRectHorizontalFlag = false;
        if (scrollPanel.m_ViewType == ScrollPanel.ViewType.Horizontal)
        {
            prop = serializedObject.FindProperty("m_HorizontalLayoutParams");
            scrollRectHorizontalFlag = true;
        } else if (scrollPanel.m_ViewType == ScrollPanel.ViewType.Vertical)
        {
            prop = serializedObject.FindProperty("m_VerticalLayoutParams");
            scrollRectVerticalFlag = true;
        } else if (scrollPanel.m_ViewType == ScrollPanel.ViewType.VerticalGrid)
        {
            prop = serializedObject.FindProperty("m_GridLayoutParams");
            scrollRectVerticalFlag = true;
        }
        if (prop != null)
        {
            EditorGUILayout.PropertyField(prop, true);
            mScrollRect.vertical = scrollRectVerticalFlag;
            mScrollRect.horizontal = scrollRectHorizontalFlag;
        }

        // Show OnStart
        scrollPanel.m_ShowOnStart = EditorGUILayout.Toggle("Show On Start", scrollPanel.m_ShowOnStart);

        // Default Cell Prefab
        scrollPanel.m_DefaultCellPrefab = (GameObject) EditorGUILayout.ObjectField("Default Cell Prefab", scrollPanel.m_DefaultCellPrefab, typeof(GameObject), true);

        // Group Items Count
        scrollPanel.m_GroupItemsCount = EditorGUILayout.IntField("Group Items Count", scrollPanel.m_GroupItemsCount);

        // Group Create Threshold
        scrollPanel.m_GroupCreateThreshold = EditorGUILayout.IntField("Group Create Threshold", scrollPanel.m_GroupCreateThreshold);

        // Group Destroy Threshold
        scrollPanel.m_GroupDestroyThreshold = EditorGUILayout.IntField("Group Destroy Threshold", scrollPanel.m_GroupDestroyThreshold);

        // Data Provider
        scrollPanel.m_DataProvider = (ScrollViewDataProvider) EditorGUILayout.ObjectField("Data Provider", scrollPanel.m_DataProvider, typeof(ScrollViewDataProvider), true);
        if (scrollPanel.m_DataProvider == null)
        {
            prop = serializedObject.FindProperty("m_DataProvider");
            prop.objectReferenceValue = mDataProvider;
        }

        // Layout Provider
        scrollPanel.m_LayoutProvider = (ScrollViewLayoutProvider) EditorGUILayout.ObjectField("Layout Provider", scrollPanel.m_LayoutProvider, typeof(ScrollViewLayoutProvider), true);
        if (scrollPanel.m_LayoutProvider == null)
        {
            prop = serializedObject.FindProperty("m_LayoutProvider");
            prop.objectReferenceValue = mLayoutProvider;
        }

        serializedObject.ApplyModifiedProperties();
    }
}
