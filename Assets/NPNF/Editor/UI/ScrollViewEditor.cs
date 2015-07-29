using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using NPNF.UI;
public class ScrollViewEditor 
{
    [MenuItem("GameObject/UI/NPNF/Scroll View")]
    static void AddScrollView()
    {
        Canvas canvas = null;

        if (Selection.activeGameObject != null)
        {
            Transform activeGameObjectRoot = Selection.activeGameObject.transform;
            while(true)
            {
                if (activeGameObjectRoot.transform.parent == null)
                {
                    canvas = activeGameObjectRoot.gameObject.GetComponent<Canvas>();
                    break;
                }
                activeGameObjectRoot = activeGameObjectRoot.transform.parent;
            }
        }

        if (canvas == null)
        {
            canvas = GameObject.FindObjectOfType<Canvas>();
        }

        if(canvas == null)
        {
            EditorApplication.ExecuteMenuItem("GameObject/UI/Canvas");
            canvas = GameObject.FindObjectOfType<Canvas>();
        }

        if (canvas != null)
        {
            int parentLayer = canvas.transform.gameObject.layer;
            GameObject scrollViewObj = new GameObject("ScrollView", typeof(RectTransform), typeof(ScrollPanel));
            scrollViewObj.transform.SetParent(canvas.transform);
            scrollViewObj.layer = parentLayer;
            ResetPosition(scrollViewObj);
            RectTransform scrollViewRect = scrollViewObj.GetComponent<RectTransform>();
            scrollViewRect.sizeDelta = new Vector2(400, 250);

            GameObject scrollPanelObj = new GameObject("ScrollPanel", typeof(RectTransform), typeof(Image), typeof(ScrollRect), typeof(Mask));
            scrollPanelObj.transform.SetParent(scrollViewObj.transform);
            scrollPanelObj.layer = parentLayer;
            ResetPosition(scrollPanelObj);
            RectTransform scrollPanelObjTransform = scrollPanelObj.GetComponent<RectTransform>();
            scrollPanelObjTransform.anchorMin = Vector2.zero;
            scrollPanelObjTransform.anchorMax = Vector2.one;

            Image image = scrollPanelObj.GetComponent<Image>();
            image.color = new Color(1, 1, 1, 0.5f);

            GameObject contentPanel = new GameObject("ContentPanel", typeof(RectTransform), typeof(GridLayoutGroup), typeof(HorizontalLayoutGroup), typeof(VerticalLayoutGroup));
            contentPanel.transform.SetParent(scrollPanelObj.transform);
            contentPanel.layer = parentLayer;
            ResetPosition(contentPanel);
            RectTransform contentPanelRect = contentPanel.GetComponent<RectTransform>();
            contentPanelRect.pivot = new Vector2(0.5f, 1f);
            contentPanelRect.anchorMin = Vector2.zero;
            contentPanelRect.anchorMax = Vector2.one;

            ScrollRect scrollRectScript = scrollPanelObj.GetComponent<ScrollRect>();
            scrollRectScript.horizontal = false;
            scrollRectScript.content = contentPanelRect;

            GridLayoutGroup contentGridLayout = contentPanel.GetComponent<GridLayoutGroup>();
            contentGridLayout.cellSize = new Vector2(400, 1000);
            contentGridLayout.spacing = Vector2.zero;
            contentGridLayout.startCorner = GridLayoutGroup.Corner.UpperLeft;
            contentGridLayout.startAxis = GridLayoutGroup.Axis.Vertical;
            contentGridLayout.childAlignment = TextAnchor.UpperCenter;
            contentGridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            contentGridLayout.constraintCount = 1;

            HorizontalLayoutGroup contentHorizontalLayout = contentPanel.GetComponent<HorizontalLayoutGroup>();
            contentHorizontalLayout.childForceExpandHeight = false;
            contentHorizontalLayout.childForceExpandWidth = false;
            contentHorizontalLayout.enabled = false;

            VerticalLayoutGroup contentVerticallLayout = contentPanel.GetComponent<VerticalLayoutGroup>();
            contentVerticallLayout.childForceExpandHeight = false;
            contentVerticallLayout.childForceExpandWidth = false;
            contentVerticallLayout.enabled = false;
        }
    }

    private static void ResetPosition(GameObject uiGameObj)
    {
        RectTransform transform = (RectTransform) uiGameObj.transform;
        transform.anchoredPosition = Vector3.zero;
        transform.anchoredPosition3D = Vector3.zero;
        transform.localScale = Vector3.one;
        transform.localPosition = Vector3.zero;
        transform.offsetMin = Vector2.zero;
        transform.offsetMax = Vector2.zero;
    }
}
