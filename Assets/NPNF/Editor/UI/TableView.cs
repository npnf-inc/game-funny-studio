using UnityEngine;
using System.Collections;

namespace NPNF.UI
{
	internal class TableView
	{
		private const int DEFAULT_CELL_WIDTH = 150;
		private const int DEFAULT_CELL_HEIGHT = 20;

		private Vector2 mOrigin;
		private float mCellWidth;
		private float mCellHeight;
		private Vector2 mTRScrollPosition;
		private Vector2 mBLScrollPosition;
		private Vector2 mBRScrollPosition;
		private bool mLastScrolledYBR = false;
		private int mLastNumRows = 0;
		private int mLastNumCols = 0;

		private TableAdapter mAdapter = null;
		private Rect mRect;

		public Vector2 MaxSize { get; set; }
		public Rect PositionRect { get; set; }

		public TableView(TableAdapter adapter)
		{
			mAdapter = adapter;
		}

        public void UpdateLocation(Vector2 position, float cellWidth, float cellHeight)
        {
            mCellWidth = cellWidth;
            mCellHeight = cellHeight;
            mOrigin = position;
            mRect = new Rect(position.x, position.y, cellWidth, cellHeight);
            PositionRect = new Rect(mRect);
        }

        public void SetAdapter(TableAdapter adapter)
        {
            mAdapter = adapter;
        }

		public void OnGUI()
		{
			HeadingLeftGUI(mOrigin);
			HeadingRightGUI(mOrigin);

			Vector2 origin = new Vector2(mOrigin.x, mOrigin.y + mCellHeight);
			BodyLeftGUI(origin);
			BodyRightGUI(origin);

			PositionRect = new Rect(mOrigin.x,
			                        mOrigin.y, 
			                        Mathf.Min(mCellWidth * mAdapter.NumColumns, MaxSize.x) + GUI.skin.verticalScrollbar.fixedWidth,
			                        Mathf.Min(mCellHeight * (mAdapter.NumRows + 1), MaxSize.y) + GUI.skin.horizontalScrollbar.fixedHeight);

			if (mAdapter.NumColumns != mLastNumCols)
			{
				mBRScrollPosition.x = mCellWidth * mAdapter.NumColumns;
				mLastNumCols = mAdapter.NumColumns;
			}
			if (mAdapter.NumRows != mLastNumRows)
			{
				mBRScrollPosition.y = mCellHeight * mAdapter.NumRows;
				mLastScrolledYBR = true;
				mLastNumRows = mAdapter.NumRows;
			}
			if (mLastScrolledYBR)
			{
				mBLScrollPosition.y = mBRScrollPosition.y;
				mLastScrolledYBR = false;
			}
			else
			{
				mBRScrollPosition.y = mBLScrollPosition.y;
			}
			mTRScrollPosition.x = mBRScrollPosition.x;
			mAdapter.GUIUpdate();
		}

		private void HeadingLeftGUI (Vector2 origin)
		{
			int columnCount = mAdapter.NumColumns;
			int verticalDividerPos = mAdapter.VerticalDividerPosition;
			mRect = new Rect(origin.x, origin.y, mCellWidth, mCellHeight);
			for (int i = 0; i < verticalDividerPos && i < columnCount; ++i)
			{
				mRect.x = origin.x + i * mCellWidth;
				mAdapter.OnDrawCellGUI(i, 0, mRect);
			}
		}

		private void HeadingRightGUI (Vector2 origin)
		{
			int numColumns = mAdapter.NumColumns;
			int verticalDividerPos = mAdapter.VerticalDividerPosition;
			float x = origin.x + verticalDividerPos * mCellWidth;
			float y = origin.y;
			Rect position = new Rect(x, y, MaxSize.x - (mCellWidth * verticalDividerPos), mCellHeight);
			Rect viewRect = new Rect(0, 0, (numColumns - verticalDividerPos) * mCellWidth, mCellHeight);

			mTRScrollPosition = GUI.BeginScrollView(position, mTRScrollPosition, viewRect, GUIStyle.none, GUIStyle.none);

			mRect = new Rect(0, 0, mCellWidth, mCellHeight);
			for (int i = verticalDividerPos; i < numColumns; ++i)
			{
				mRect.x = (i - verticalDividerPos) * mCellWidth;
				mAdapter.OnDrawCellGUI(i, 0, mRect);
			}

			GUI.EndScrollView();
		}

		void BodyLeftGUI (Vector2 origin)
		{
			int verticalDividerPos = mAdapter.VerticalDividerPosition;
			Rect position = new Rect(origin.x, origin.y, mCellWidth * verticalDividerPos, MaxSize.y - mCellHeight);
			Rect viewRect = new Rect(0, 0, mCellWidth * verticalDividerPos, mCellHeight * mAdapter.NumRows);

			mBLScrollPosition = GUI.BeginScrollView(position, mBLScrollPosition, viewRect, GUIStyle.none, GUIStyle.none);

			mRect = new Rect(0, 0, mCellWidth, mCellHeight);
			for (int j = 0; j < mAdapter.NumRows; ++j)
			{
				for (int i = 0; i < verticalDividerPos; ++i)
				{
					mAdapter.OnDrawCellGUI(i, j+1, mRect);
					mRect.x += mCellWidth;
				}
				mRect.x = 0;
				mRect.y += mCellHeight;
			}

			GUI.EndScrollView();
		}

		void BodyRightGUI (Vector2 origin)
		{
			int verticalDividerPos = mAdapter.VerticalDividerPosition;
			float x = origin.x + mCellWidth * verticalDividerPos;
			float y = origin.y;
			Rect viewRect = new Rect(0, 0, mCellWidth * (mAdapter.NumColumns - verticalDividerPos), mCellHeight * mAdapter.NumRows);
			Rect position = new Rect(x, y,
			                         Mathf.Min(MaxSize.x - (mCellWidth * verticalDividerPos), viewRect.width),
			                         Mathf.Min(MaxSize.y - mCellHeight, viewRect.height));
			if (viewRect.width > position.width)
			{
				position.height += GUI.skin.horizontalScrollbar.fixedHeight;
			}

			if (viewRect.height > position.height)
			{
				position.width += GUI.skin.verticalScrollbar.fixedWidth;
			}
			Vector2 newScrollPosition = GUI.BeginScrollView(position, mBRScrollPosition, viewRect);
			if (newScrollPosition.y != mBRScrollPosition.y)
			{
				mLastScrolledYBR = true;
			}
			mBRScrollPosition = newScrollPosition;

			mRect = new Rect(0, 0, mCellWidth, mCellHeight);
			for (int j = 0; j < mAdapter.NumRows; ++j)
			{
				for (int i = verticalDividerPos; i < mAdapter.NumColumns; ++i)
				{
					mAdapter.OnDrawCellGUI(i, j+1, mRect);
					mRect.x += mCellWidth;
				}  
				mRect.x = 0;
				mRect.y += mCellHeight;
			}
			GUI.EndScrollView();
		}
	}
}