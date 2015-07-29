using UnityEngine;
using System.Collections.Generic;
using System;
using NPNF.Core;
using NPNF.UI;
using System.ComponentModel;
using System.Globalization;

namespace NPNF.Game
{
    public abstract class NPNFGameGroup<TContainer, TModel> : ScrollViewDataProvider
        where TContainer : UnityEngine.Component
        where TModel : NPNFModel
    {
        [SerializeField] private GameObject template;
        [SerializeField] private GameObject parent;
        [SerializeField] private bool setActiveImmediately;
        [SerializeField] private bool showEnabledOnly;
        [SerializeField] private string sortKey;
        [SerializeField] private ListSortDirection sortOrder;

        private bool mPlatformInitialized;
        private ScrollPanel mScrollPanel;

        #region Properties
        public bool IsInitialized { get; private set; }
        public List<TModel> ModelObjects { get; protected set; }

        private Predicate<TModel> mFilter = null;

        /**
         * Set a Filter to filter data saved to ModelObjects
         * Return true to keep the data, and false to remove
         */
        public Predicate<TModel> Filter
        {
            get
            {
                return mFilter;
            }
            set
            {
                if (mFilter != value)
                {
                    mFilter = value;
                    if (IsInitialized)
                    {
                        Refresh();
                    }
                }
            }
        }
        #endregion

        #pragma warning disable 67
        /**
         * Triggers when initialization is completed for this Group
         */
        public event System.Action OnInitialized;

        /**
         * Triggers when there is an error occurring during initialization
         */
        public event Action<NPNFError> OnInitializeError;
        #pragma warning restore 67
        
        private class ModelComparer : IComparer<TModel>
        {
            private string mSortKey;
            private ListSortDirection mSortOrder;
            
            public ModelComparer(string sortKey, ListSortDirection sortOrder)
            {
                mSortKey = sortKey;
                mSortOrder = sortOrder;
            }
            
            #region IComparer implementation
            
            public int Compare(TModel x, TModel y)
            {
                object xVal, yVal;
                if (mSortOrder == ListSortDirection.Ascending)
                {
                    xVal = x.GetCustom(mSortKey);
                    yVal = y.GetCustom(mSortKey);
                } else
                {
                    yVal = x.GetCustom(mSortKey);
                    xVal = y.GetCustom(mSortKey);
                }
                
                if (xVal == null && yVal == null)
                    return 0;
                else if (xVal == null)
                    return -1;
                else if (yVal == null)
                    return 1;
                else if (IsNumeric(xVal) && IsNumeric(yVal))
                    return CompareNumeric(Convert.ToDouble(xVal), Convert.ToDouble(yVal));
                else if (xVal.GetType() != yVal.GetType())
                    return 0;
                else if (xVal.GetType() == typeof(string))
                    return CompareStrings(xVal as string, yVal as string);
                else
                    return 0;
            }
            
            #endregion
            
            private int CompareNumeric(double xVal, double yVal)
            {
                return xVal.CompareTo(yVal);
            }
            
            private int CompareStrings(string xVal, string yVal)
            {
                return xVal.CompareTo(yVal);
            }
            
            public static bool IsNumeric(object expression)
            {
                if (expression == null)
                    return false;
                
                double number;
                return Double.TryParse(Convert.ToString(expression, CultureInfo.InvariantCulture), System.Globalization.NumberStyles.Any, NumberFormatInfo.InvariantInfo, out number);
            }
        }
        
        #region Lifecycle
        protected virtual void OnEnable()
        {
            NPNFMain.OnInitComplete += InitCompleteHandler;
            if (IsInitialized)
            {
                Refresh();
            }
        }
        
        protected virtual void OnDisable()
        {
            NPNFMain.OnInitComplete -= InitCompleteHandler;
        }
        
        protected virtual void Start()
        {
            if (!mPlatformInitialized && Platform.IsInitialized)
            {
                OnPlatformInit();
                mPlatformInitialized = true;
            }

            // Automatically set properties if a Scroll Panel exists on the same game object
            mScrollPanel = gameObject.GetComponent<ScrollPanel>();
            if (mScrollPanel != null)
            {
                mScrollPanel.m_DataProvider = this;
                template = mScrollPanel.m_DefaultCellPrefab;
                parent = null;
            }

            if (IsInitialized)
            {
                TriggerInitialized(ModelObjects);
            }
        }
        #endregion

        #region Handlers
        private void InitCompleteHandler(NPNFError error)
        {
            if (!mPlatformInitialized)
            {
                if (error == null)
                {
                    OnPlatformInit();
                    mPlatformInitialized = true;
                } else if (OnInitializeError != null)
                {
                    OnInitializeError(error);
                }
            }
        }
        #endregion

        #region implemented abstract members of ScrollViewDataProvider

        public override Dictionary<string, object> GetData(int cellIndex, GameObject cell)
        {
            AddComponent(cell, ModelObjects[cellIndex]);
            return null;
        }

        public override int NumItems
        {
            get
            {
                return ModelObjects.Count;
            }
        }

        public override void OnClick(int cellIndex, GameObject cell)
        {
        }
        #endregion

        public List<GameObject> GenerateAll()
        {
            if (!IsInitialized)
            {
                Debug.LogError("Attempted to generate GameObjects before completing initialization of Group component");
                return null;
            }

            List<GameObject> gameObjs = new List<GameObject>();
            if (ModelObjects != null && ModelObjects.Count > 0)
            {
                for (int i = 0; i < ModelObjects.Count; ++i)
                {
                    gameObjs.Add(Generate(ModelObjects[i], i+1, parent));
                }
            }
            return gameObjs;
        }

        private GameObject Generate(TModel model, int index, GameObject parent)
        {
            GameObject newGameObject = Instantiate(template, template.transform.position, template.transform.rotation) as GameObject;
            if (parent != null)
            {
                newGameObject.transform.SetParent(parent.transform);
            }
            newGameObject.SetActive(setActiveImmediately);
            newGameObject.name = template.name + index;
            AddComponent(newGameObject, model);
            return newGameObject;
        }

        private TContainer AddComponent(GameObject gameObj, TModel model)
        {
            TContainer newComponent = gameObj.AddComponent<TContainer>();
            ConfigureComponent(gameObj, newComponent, model);
            return newComponent;
        }

        protected void TriggerInitialized(List<TModel> models)
        {
            ModelObjects = models;
            if (showEnabledOnly)
            {   
                ModelObjects.RemoveAll((TModel model) => {
                    return (!model.Enabled);
                });
            }
            if (!string.IsNullOrEmpty(sortKey))
            {
                ModelObjects.Sort(new ModelComparer(sortKey, sortOrder));
            }
            if (Filter != null)
            {
                ModelObjects.RemoveAll(delegate(TModel obj) {
                    return !Filter(obj);
                });
            }
            if (mScrollPanel != null)
            {
                mScrollPanel.Show();
            }
            IsInitialized = true;
            if (OnInitialized != null)
                OnInitialized();
        }

        protected void TriggerInitializeError(NPNFError error)
        {
            IsInitialized = false;
            if (OnInitializeError != null)
                OnInitializeError(error);
        }

        protected virtual void OnPlatformInit()
        {
        }

        protected virtual void Refresh()
        {
        }

        protected abstract void ConfigureComponent(GameObject gameObject, TContainer component, TModel model);
    }
}
