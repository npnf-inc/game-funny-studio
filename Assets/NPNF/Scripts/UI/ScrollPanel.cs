using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine.UI;
using System;

namespace NPNF.UI
{
    [Serializable]
    public class ScrollPanel : MonoBehaviour
    {
        [Serializable]
        public class GridLayoutParams : LayoutGroupParams
        {
            public Vector2 cellSize;
            public Vector2 spacing;
            public GridLayoutGroup.Corner startCorner;
            public GridLayoutGroup.Constraint constraint;
            public int constraintCount;
        }

        [Serializable]
        public class HorizontalLayoutParams : LayoutGroupParams
        {
            public float spacing = 0;
            public bool forceExpandWidth = false;
            public bool forceExpandHeight = false;
        }

        [Serializable]
        public class VerticalLayoutParams : LayoutGroupParams
        {
            public float spacing = 0;
            public bool forceExpandWidth = false;
            public bool forceExpandHeight = false;
        }

        [Serializable]
        public enum ViewType
        {
            Horizontal,
            Vertical,
            VerticalGrid
        }

        /* *** Inspector Window Parameters *** */

        public ViewType m_ViewType = ViewType.Vertical;

        // Only one of the below will be used to generate the scrollview, determined by the scroll orientation
        public GridLayoutParams m_GridLayoutParams;
        public HorizontalLayoutParams m_HorizontalLayoutParams;
        public VerticalLayoutParams m_VerticalLayoutParams;

        public GameObject m_DefaultCellPrefab;
        public int m_GroupItemsCount = 10;

        public bool m_ShowOnStart = false;

        //how close group border to the view the next group should be created
        //the larger the TRESHOLD, the earlier the group will be created
        public int m_GroupCreateThreshold = 2;
        
        //When new group is created, the groups standing by distance out of this threshold will be destroyed
        //this value should be equal to or greater than the m_GroupCreateThreshold
        public int m_GroupDestroyThreshold = 3;

        public ScrollViewDataProvider m_DataProvider;
        public ScrollViewLayoutProvider m_LayoutProvider;

        /* *** Private Data *** */

        // Existing cell group index and its data
        private SortedList<int, GameObject> _groups = new SortedList<int, GameObject>();

        // Components in children which need to be updated based on current settings
        private ScrollRect _scrollRect;
        private LayoutGroup _contentLayoutGroup;
        private LayoutGroupParams _contentLayoutGroupParams;
        private int _maxNumCells;
        
        //view position parameters
        private RectTransform _scrollRectTransform;
        private RectTransform _contentTransform;

        private int MinShownGroupIndex { get { return _groups.Count > 0 ? _groups.Keys[0] : -1; } }
        private int MaxShownGroupIndex { get { return _groups.Count > 0 ? _groups.Keys[_groups.Count - 1] : -1; } }
        
        private bool isInitialized = false;
        private bool populateWhenInitialized = false;

        /* *** Properties *** */

		private float ContentHeight
		{
            get
            {
                float height = 0f;
                if (MinShownGroupIndex <= MaxShownGroupIndex && _groups.Count > 0)
                {
                    RectTransform maxGridTransform = _groups[MaxShownGroupIndex].gameObject.transform as RectTransform;
                    height += -maxGridTransform.localPosition.y;
                    if (maxGridTransform.childCount > 0)
                    {
                        RectTransform lastChild = maxGridTransform.GetChild(maxGridTransform.childCount - 1) as RectTransform;
                        height += -lastChild.localPosition.y + lastChild.rect.height;
                    }
                }

                if (height < _scrollRectTransform.rect.height)
                    height = _scrollRectTransform.rect.height;
                return height;
			}
		}

        private float ContentWidth
        {
            get
            {
                float width = 0f;
                if (MinShownGroupIndex <= MaxShownGroupIndex && _groups.Count > 0)
                {
                    if (_groups[MaxShownGroupIndex].gameObject != null)
                    {
                        RectTransform maxGridTransform = _groups[MaxShownGroupIndex].gameObject.transform as RectTransform;
                        width += maxGridTransform.localPosition.x;
                        if (maxGridTransform.childCount > 0)
                        {
                            RectTransform lastChild = maxGridTransform.GetChild(maxGridTransform.childCount - 1) as RectTransform;
                            width += lastChild.localPosition.x + lastChild.rect.width;
                        }
                    }
                }
                return width;
            }
        }

        private int CurrentGroupIndex { get; set; }

        private void UpdateCurrentGroupIndex()
        {
            float top, bottom;
            if (Horizontal)
            {
                bottom = -_contentTransform.offsetMin.x;
                top = bottom + _scrollRectTransform.rect.width;
            } else
            {
                top = -_contentTransform.offsetMax.y;
                bottom = top - _scrollRectTransform.rect.height;
            }
            int index = 0;
            foreach (var pair in _groups)
            {
                Vector3 localPosition = pair.Value.transform.localPosition;
                float width = ((RectTransform)pair.Value.transform).rect.width;
                float groupPosition = (Horizontal ? localPosition.x : localPosition.y);
                if ((!Horizontal && groupPosition < bottom) || (Horizontal && (groupPosition < top && groupPosition + width > bottom)))
                {
                    break;
                } else
                {
                    index = pair.Key;
                }
            }
            CurrentGroupIndex = index;
        }

        private bool Horizontal { get { return m_ViewType == ViewType.Horizontal; } }

        void Awake()
        {
            _scrollRect = gameObject.GetComponentInChildren<ScrollRect>();
            _scrollRectTransform = _scrollRect.gameObject.GetComponent<RectTransform>();

            foreach(LayoutGroup layoutGroup in _scrollRect.content.GetComponents<LayoutGroup>())
            {
                layoutGroup.enabled = false;
            }
            if (m_ViewType == ViewType.Vertical)
            {
                _contentLayoutGroup = _scrollRect.content.GetComponent<VerticalLayoutGroup>();
                _contentLayoutGroupParams = m_VerticalLayoutParams;
            } else if (m_ViewType == ViewType.Horizontal)
            {
                _contentLayoutGroup = _scrollRect.content.GetComponent<HorizontalLayoutGroup>();
                _contentLayoutGroupParams = m_HorizontalLayoutParams;
            } else
            {
                _contentLayoutGroup = _scrollRect.content.GetComponent<GridLayoutGroup>();
                _contentLayoutGroupParams = m_GridLayoutParams;
            }
            _contentLayoutGroup.enabled = true;
            _contentTransform = _contentLayoutGroup.GetComponent<RectTransform>();
        }

        void OnEnable()
        {
            if (m_DataProvider != null)
            {
                m_DataProvider.OnDataChanged += Show;
            }
        }

        void OnDisable()
        {
            if (m_DataProvider != null)
            {
                m_DataProvider.OnDataChanged -= Show;
            }
        }

        void Start()
        {

            // Update common group layout parameters
            _contentLayoutGroup.childAlignment = _contentLayoutGroupParams.childAlignment;
            _contentLayoutGroup.padding = _contentLayoutGroupParams.padding;

            // Update grid-specific Group Layout Parameters
            if (m_ViewType == ViewType.VerticalGrid)
            {
                float _verticalDivider = 1.0f;

                GridLayoutGroup contentGridLayoutGroup = (GridLayoutGroup) _contentLayoutGroup;
                GridLayoutParams groupParams = (GridLayoutParams) _contentLayoutGroupParams;

                // Set up grid dividers
                if (groupParams.constraint == GridLayoutGroup.Constraint.FixedColumnCount)
                    _verticalDivider = groupParams.constraintCount;

                float cellHeight = (groupParams.cellSize.y + groupParams.spacing.y) * m_GroupItemsCount / _verticalDivider;
                float cellWidth = (groupParams.cellSize.x + groupParams.spacing.x) * _verticalDivider;
                contentGridLayoutGroup.cellSize = new Vector2(cellWidth, cellHeight);
                contentGridLayoutGroup.spacing = Vector2.zero;
                contentGridLayoutGroup.startCorner = groupParams.startCorner;


            } else if (m_ViewType == ViewType.Vertical)
            {
                VerticalLayoutGroup contentVerticalLayoutGroup = (VerticalLayoutGroup) _contentLayoutGroup;
                VerticalLayoutParams groupParams = (VerticalLayoutParams) _contentLayoutGroupParams;
                contentVerticalLayoutGroup.childForceExpandWidth = groupParams.forceExpandWidth;
                contentVerticalLayoutGroup.childForceExpandHeight = groupParams.forceExpandHeight;
                contentVerticalLayoutGroup.spacing = groupParams.spacing;

            } else if (m_ViewType == ViewType.Horizontal)
            {
                HorizontalLayoutGroup contentHorizontalLayoutGroup = (HorizontalLayoutGroup) _contentLayoutGroup;
                HorizontalLayoutParams groupParams = (HorizontalLayoutParams) _contentLayoutGroupParams;
                contentHorizontalLayoutGroup.childForceExpandWidth = groupParams.forceExpandWidth;
                contentHorizontalLayoutGroup.childForceExpandHeight = groupParams.forceExpandHeight;
                contentHorizontalLayoutGroup.spacing = groupParams.spacing;
            }

            isInitialized = true;
            if (populateWhenInitialized)
                PopulateScrollView();

            if (m_ShowOnStart)
                Show();
        }

        void Update()
        {
            if (!Application.isPlaying) return;

            if (_groups.Count > 0)
            {
                // Resize and reposition the Content transform
                if (Horizontal)
                {
                    Vector2 offsetMin = _contentTransform.offsetMin;
                    _contentTransform.sizeDelta = new Vector2(ContentWidth - _scrollRectTransform.rect.width, 0f);
                    _contentTransform.offsetMin = offsetMin;
                } else
                {
                    Vector2 offsetMax = _contentTransform.offsetMax;
                    _contentTransform.sizeDelta = new Vector2(0f, ContentHeight - _scrollRectTransform.rect.height);
                    _contentTransform.offsetMax = offsetMax;
                }
                UpdateCurrentGroupIndex();

                int currentGroupIndex = CurrentGroupIndex;
                // Weird positions are registered when removing/adding grids, account for this
                if (currentGroupIndex >= MinShownGroupIndex &&
                    currentGroupIndex <= MaxShownGroupIndex)
                    //                    currentGroupIndex != _lastGroupIndex)
                {
                    CreateThresholdGroups(currentGroupIndex, m_GroupCreateThreshold);
                    DestroyThresholdGroups(currentGroupIndex, m_GroupDestroyThreshold);
                    //                    _lastGroupIndex = currentGroupIndex;
                }
            }
        }


        private void CreateThresholdGroups(int currentGridIndex, int threshold)
        {
            for (int i = currentGridIndex - threshold; i <= currentGridIndex + threshold && i * m_GroupItemsCount < _maxNumCells; ++i)
            {
                if (i < 0) continue;
                CreateCellGroup(i);
            }
        }

        private void CreateCellGroup(int groupIndex)
        {
            if (groupIndex >= 0 && !_groups.ContainsKey(groupIndex))
            {
                _groups.Add(groupIndex, null);
                int childIndex = _groups.IndexOfKey(groupIndex);
                _groups[groupIndex] = CreateCellGroupGameObject(groupIndex, childIndex);

                if (groupIndex < CurrentGroupIndex)
                {
                    RectTransform transform = (RectTransform)_groups[groupIndex].transform;
                    float gridSize = (Horizontal ? transform.rect.width : transform.rect.height);
                    float newOffSetX =  (Horizontal ? _contentTransform.offsetMin.x - gridSize : _contentTransform.offsetMax.x);
                    float newOffSetY =  (Horizontal ? _contentTransform.offsetMin.y : _contentTransform.offsetMax.y - gridSize);
                    if (Horizontal)
                    {
                        _contentTransform.offsetMin = new Vector2(newOffSetX, newOffSetY);
                    } else
                    {
                        _contentTransform.offsetMax = new Vector2(newOffSetX, newOffSetY);
                    }
                }
            }
        }

        private void DestroyThresholdGroups(int currentGridIndex, int thresholdSize)
        {
            int lowerIndex = currentGridIndex - thresholdSize;
            int upperIndex = currentGridIndex + thresholdSize;
            foreach(var pair in _groups)
            {
                int gridIndex = pair.Key;
                if (gridIndex < lowerIndex || gridIndex > upperIndex)
                {
                    DestroyGrid(gridIndex);
                }
            }
        }

        private void DestroyGrid(int groupIndex)
        {
            if(_groups.ContainsKey(groupIndex))
            {
                bool removingGridFromTop = false;
                RectTransform transform = (RectTransform)_groups[groupIndex].transform;
                float gridSize = (Horizontal ? transform.rect.width : transform.rect.height);
                if (groupIndex < CurrentGroupIndex)
                {
                    removingGridFromTop = true;
                }
                Destroy(_groups[groupIndex]);
                _groups.Remove(groupIndex);

                if (removingGridFromTop)
                {
                    float newOffSetX =  (Horizontal ? _contentTransform.offsetMin.x + gridSize : _contentTransform.offsetMax.x);
                    float newOffSetY =  (Horizontal ? _contentTransform.offsetMin.y : _contentTransform.offsetMax.y - gridSize);
                    if (Horizontal)
                    {
                        _contentTransform.offsetMin = new Vector2(newOffSetX, newOffSetY);
                    } else
                    {
                        _contentTransform.offsetMax = new Vector2(newOffSetX, newOffSetY);
                    }
                }
            }
        }

        private void Show(int itemsCount)
        {
            _maxNumCells = itemsCount;
            if (isInitialized)
                PopulateScrollView();
            else
                populateWhenInitialized = true;
        }

        public void Show()
        {
            if (m_DataProvider == null)
            {
                throw new Exception("Expecting to use a data provider but none was specified!");
            }
            _maxNumCells = m_DataProvider.NumItems;
            Show(_maxNumCells);
        }

        public void ScrollTo(int cellIndex)
        {
            if (cellIndex < 0 || cellIndex >= _maxNumCells)
            {
                Debug.Log("Cannot scroll to cell with index " + cellIndex + ": out of bounds (max=" + _maxNumCells +")");
            }
            if (cellIndex < 0)
                cellIndex = 0;

            float normalizedPosition = -1;
            normalizedPosition = GetCellNormalizedPosition(cellIndex, m_ViewType);
            SetScrollRectPosition(normalizedPosition, m_ViewType);
        }

        private float GetCellNormalizedPosition(int cellIndex, ViewType viewType)
        {
            float normalizedPosition = -1f;
            float localPosition = 0f;

            int groupIndex = cellIndex / m_GroupItemsCount;
            if (_groups.ContainsKey(groupIndex))
            {
                Transform childTransform = _groups[groupIndex].transform.GetChild(cellIndex % m_GroupItemsCount);
                float childPosition = childTransform != null ? GetPosition(childTransform.localPosition, viewType) : 0f;
                if (Horizontal)
                {
                    localPosition = ((RectTransform)_groups[groupIndex].transform).offsetMin.x - ((RectTransform)_groups[MinShownGroupIndex].transform).offsetMin.x;
                    localPosition += childPosition;
                    normalizedPosition = localPosition / _contentTransform.sizeDelta.x;
                } else
                {
                    localPosition = -(((RectTransform)_groups[groupIndex].transform).offsetMax.y - ((RectTransform)_groups[MinShownGroupIndex].transform).offsetMax.y);
                    localPosition -= childPosition;
                    normalizedPosition = 1f - localPosition / _contentTransform.sizeDelta.y;
                }
            } else if (cellIndex < MinShownGroupIndex)
            {
                normalizedPosition = 1f;
            } else
            {
                normalizedPosition = 0f;
            }
            return normalizedPosition;
        }

        private float GetPosition(Vector3 position, ViewType viewType)
        {
            if (viewType == ViewType.Horizontal)
            {
                return position.x;
            } else
            {
                return position.y;
            }
        }

        private void SetScrollRectPosition(float normalizedPosition, ViewType viewType)
        {
            if (viewType == ViewType.Horizontal)
            {
                _scrollRect.horizontalNormalizedPosition = normalizedPosition;
            } else
            {
                _scrollRect.verticalNormalizedPosition = normalizedPosition;
            }
        }

        private void PopulateScrollView()
        {
            if (_contentTransform != null)
            {
                _contentTransform.offsetMax.Set((Horizontal ? 0 : _contentTransform.offsetMax.x), (Horizontal ? _contentTransform.offsetMax.y : 0f));
            }   
            ClearGroups();
            CreateThresholdGroups(0, m_GroupCreateThreshold);
            _scrollRect.normalizedPosition = Horizontal ? Vector2.zero : Vector2.one;
        }

        private void ClearGroups()
        {
            foreach(GameObject group in _groups.Values)
            {
                group.SetActive(false);
                Destroy(group);
            }
            _groups.Clear();
        }

        private GameObject CreateCellGroupGameObject(int groupIndex, int childIndex)
        {
            //get the items range
            int startCellIndex = groupIndex * m_GroupItemsCount;
            int lastCellIndex = startCellIndex + m_GroupItemsCount - 1;

            GameObject nextGroup = new GameObject("CellGroup " + groupIndex.ToString());
            nextGroup.transform.SetParent(_scrollRect.content);
            nextGroup.transform.SetSiblingIndex(childIndex);

            AddLayoutGroupComponent(nextGroup);

            RectTransform nextGridObjTransform = nextGroup.GetComponent<RectTransform>();
            nextGridObjTransform.localPosition = Vector3.zero;
            nextGridObjTransform.anchoredPosition = Vector3.zero;
            nextGridObjTransform.offsetMin = Vector2.zero;
            nextGridObjTransform.offsetMax = Vector2.zero;
            nextGridObjTransform.anchorMin = Vector2.zero;
            nextGridObjTransform.anchorMax = Vector2.one;
            nextGridObjTransform.localScale = Vector3.one;
            nextGridObjTransform.pivot = new Vector2(0f, 1f);

            for(int i = startCellIndex; i <= lastCellIndex; i++)
            {
                if (i >= _maxNumCells) break;
                GameObject aCell = CreateCell(groupIndex, i, nextGroup);
                aCell.SetActive(true);
                SetUpCell(i, aCell);
            }

            return nextGroup;
        }
        
        private void AddLayoutGroupComponent(GameObject cellGroup)
        {
            LayoutGroup layoutGroup = null;

            // Update grid-specific Group Layout Parameters
            if (m_ViewType == ViewType.VerticalGrid)
            {
                GridLayoutGroup gridLayoutGroup = cellGroup.AddComponent<GridLayoutGroup>();
                GridLayoutParams groupParams = (GridLayoutParams) _contentLayoutGroupParams;
                gridLayoutGroup.cellSize = groupParams.cellSize;
                gridLayoutGroup.spacing = groupParams.spacing;
                gridLayoutGroup.startCorner = groupParams.startCorner;
                gridLayoutGroup.startAxis = GridLayoutGroup.Axis.Horizontal;
                gridLayoutGroup.constraint = groupParams.constraint;
                gridLayoutGroup.constraintCount = m_GridLayoutParams.constraintCount;
                gridLayoutGroup.padding = new RectOffset(0, 0, 0, 0);
                layoutGroup = gridLayoutGroup;
            } else if (m_ViewType == ViewType.Vertical)
            {
                VerticalLayoutGroup verticalLayoutGroup = cellGroup.AddComponent<VerticalLayoutGroup>();
                VerticalLayoutParams groupParams = (VerticalLayoutParams) _contentLayoutGroupParams;
                verticalLayoutGroup.childForceExpandWidth = groupParams.forceExpandWidth;
                verticalLayoutGroup.childForceExpandHeight = groupParams.forceExpandHeight;
                verticalLayoutGroup.spacing = groupParams.spacing;
                verticalLayoutGroup.padding = groupParams.padding;
                layoutGroup = verticalLayoutGroup;
                
            } else if (m_ViewType == ViewType.Horizontal)
            {
                HorizontalLayoutGroup horizontalLayoutGroup = cellGroup.AddComponent<HorizontalLayoutGroup>();
                HorizontalLayoutParams groupParams = (HorizontalLayoutParams) _contentLayoutGroupParams;
                horizontalLayoutGroup.childForceExpandWidth = groupParams.forceExpandWidth;
                horizontalLayoutGroup.childForceExpandHeight = groupParams.forceExpandHeight;
                horizontalLayoutGroup.spacing = groupParams.spacing;
                horizontalLayoutGroup.padding = groupParams.padding;
                layoutGroup = horizontalLayoutGroup;
            }

            // Update common group layout parameters
            layoutGroup.childAlignment = _contentLayoutGroupParams.childAlignment;
        }

        public GameObject CreateCell(int gridIndex, int cellIndex, GameObject gridObject)
        {
            GameObject cellPrefab = null;

            if (m_LayoutProvider != null)
            {
                cellPrefab = m_LayoutProvider.GetPrefab(cellIndex);
            }
            if (cellPrefab == null)
            {
                cellPrefab = m_DefaultCellPrefab;
            }
            if (cellPrefab == null)
            {
                Debug.LogError(string.Format("No cell prefab provided for cell {0}", cellIndex));
                return null;
            } else
            {
                return AddChild(gridObject, cellPrefab);
            }
        }

        public virtual void SetUpCell(int cellIndex, GameObject cell) 
        {
            if (m_DataProvider != null)
            {
                Dictionary<string, object> data = m_DataProvider.GetData(cellIndex, cell);
                ScrollViewDataContainer container = cell.AddComponent<ScrollViewDataContainer>();
                container.Data = data;
                container.CellIndex = cellIndex;
                container.DataProvider = m_DataProvider;
            }
        }

        private static GameObject AddChild(GameObject parent, GameObject prefab)
        {
            GameObject child = Instantiate<GameObject>(prefab);
            child.transform.SetParent(parent.transform);
            RectTransform childTransform = child.GetComponent<RectTransform>();
            childTransform.localScale = Vector3.one;
            childTransform.pivot = new Vector2(0, 1);
            return child;
        }
    }
}
