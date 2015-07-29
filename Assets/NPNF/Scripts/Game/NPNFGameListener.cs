using UnityEngine;
using NPNF.Core;

namespace NPNF.Game
{
    public abstract class NPNFGameListener : MonoBehaviour
    {
        protected virtual void Start()
        {
            if (Platform.IsInitialized)
            {
                InitializeHandler();
            }
        }

        protected abstract void InitializeHandler();
        protected abstract void InitializeErrorHandler(NPNFError error);
    }
}
