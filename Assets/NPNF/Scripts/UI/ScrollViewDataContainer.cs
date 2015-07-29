using UnityEngine;
using System.Collections.Generic;
using UnityEngine.EventSystems;

namespace NPNF.UI
{
    public class SelectableEventHelper : MonoBehaviour, IPointerClickHandler
    {
        public event System.Action OnClicked;

        public void OnPointerClick(PointerEventData eventData)
        {
            if (OnClicked != null)
            {
                OnClicked();
            }
        }
    }

    public class ScrollViewDataContainer : MonoBehaviour
    {
        public Dictionary<string, object> Data { get; set; }
        public int CellIndex { get; set; }
        public ScrollViewDataProvider DataProvider { get; set; }

        void Start()
        {
            SelectableEventHelper helper = gameObject.AddComponent<SelectableEventHelper>();
            helper.name = CellIndex.ToString();
            helper.OnClicked += HandlePointerClick;
        }

        private void HandlePointerClick()
        {
            DataProvider.OnClick(CellIndex, gameObject);
        }
    }
}
