using UnityEngine;
using System;
using NPNF.Core;

namespace NPNF.Game
{
    [Serializable]
    public abstract class NPNFGameTrigger<TTrigger, TResource> : MonoBehaviour
    {
        public event Action<TTrigger> OnTrigger;
        public event Action<TResource, TTrigger, NPNFError> OnTriggerActionDone;

        public Camera sceneCamera;
        public LayerMask triggerLayerMask = ~0;            // Layer to accept input against (default Everything).

        protected bool DetectHit(Vector3 point)
        {
            RaycastHit hit = new RaycastHit();
            Ray ray = sceneCamera.ScreenPointToRay(point);
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, triggerLayerMask))
            {
                if (hit.collider.gameObject == gameObject)
                {
                    Activate();
                    return true;
                } else
                {
                    return false;
                }
            } else
            {
                return false;
            }
        }
        
        protected virtual void Update()
        {
            if (sceneCamera != null && OnTrigger != null)
            {
                #if UNITY_EDITOR || (!UNITY_IPHONE && !UNITY_ANDROID)
                // Mouse input.
                if (Input.GetMouseButtonDown(0))
                {
                    DetectHit(Input.mousePosition);
                }
                #else
                // Touch input.
                for(int i = Input.touchCount - 1; i >= 0; --i) {
                    Touch touch = Input.GetTouch(i);
                    if (touch.phase == TouchPhase.Ended && DetectHit(touch.position)) {
                        break;
                    }
                }
                #endif
            }
        }

        protected abstract void Activate();

        protected virtual void StartTriggerAction(TTrigger trigger)
        {
            if (OnTrigger != null)
            {
                OnTrigger(trigger);
            }
        }

        internal virtual void FinishTriggeredAction(TResource module, TTrigger trigger, NPNFError error)
        {
            if (OnTriggerActionDone != null)
            {
                OnTriggerActionDone(module, trigger, error);
            }
        }
    }
}