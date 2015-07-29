using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine.UI;
using System;

namespace NPNF.UI
{
    public abstract class ScrollViewDataProvider : MonoBehaviour
    {
        public event System.Action OnDataChanged;
        
        public abstract int NumItems { get; }
        
        public abstract Dictionary<string, object> GetData(int cellIndex, GameObject cell);
        
        public virtual void OnClick(int cellIndex, GameObject cell)
        {
        }
        
        protected void TriggerDataChanged()
        {
            if (OnDataChanged != null)
            {
                OnDataChanged();
            }
        }
    }
}
