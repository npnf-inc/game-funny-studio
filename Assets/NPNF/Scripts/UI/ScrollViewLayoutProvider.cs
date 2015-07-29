using UnityEngine;
using System;

namespace NPNF.UI
{
	public abstract class ScrollViewLayoutProvider : MonoBehaviour
	{
        public abstract GameObject GetPrefab(int cellIndex);
	}
}
