using System;
using UnityEngine;

namespace NPNF.UI
{
    [Serializable]
    public abstract class LayoutGroupParams
    {
        public RectOffset padding;
        public TextAnchor childAlignment = TextAnchor.UpperCenter;
    }
}
