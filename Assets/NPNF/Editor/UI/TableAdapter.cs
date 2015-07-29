using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using NPNF.Core.CollectionModule;
using NPNF.Core;

namespace NPNF.UI
{
    public class TableAdapter
    {
        public enum Status
        {
            UNMODIFIED,
            NEW,
            DELETED,
            MODIFIED
        }

        private enum ButtonState
        {
            DELETE,
            RESTORE
        }

        private static readonly string NEWENTRY_CONTROL_NAME = "NewEntryValue";
        private static readonly string NEWFIELD_CONTROL_NAME = "NewFieldValue";
        private static readonly Vector2 NULL_VECTOR = new Vector2(-1, -1);
        private ITableDataSource mDataSource;
        private EditorWindow mWindow;
        private string mNewFieldName = "";
        private const string NewFieldPlaceHolderText = "New field name";
        private string[] mNewEntryValues;
        private Vector2 mLastFocusedCell = NULL_VECTOR;
        private Color colorUnmodified = Color.white;
        private Color colorModified = Color.cyan;
        private Color colorNew = Color.green;
        private Color colorDeleted = Color.red;

        // For context menu (remove custom field)
        private int curFieldClicked = -1;
        private GenericMenu contextMenu;

        public int NumColumns
        {
            get
            {
                return mDataSource.NumFields + 1; // last column for new field
            }
        }

        public int NumRows
        {
            get
            {
                return mDataSource.NumEntries + 1; // last row for new entry
            }
        }
        
        public int VerticalDividerPosition { get; set; }

        public GUIStyle LockedHeaderStyle { get; set; }

        public GUIStyle LockedCellStyle { get; set; }

        public GUIStyle ScrollableHeaderStyle { get; set; }

        public GUIStyle ScrollableCellStyle { get; set; }

        public GUIStyle NewDataCellStyle { get; set; }

        public GUIStyle PlaceHolderTextStyle { get; set; }

        public GUIStyle ButtonStyle { get; set; }

        public TableAdapter(ITableDataSource dataSource, EditorWindow window, int verticalDividerPos = 0)
        {
            mDataSource = dataSource;
            mWindow = window;
            VerticalDividerPosition = verticalDividerPos;

            RectOffset margin = new RectOffset(4, 4, 4, 4);
            RectOffset padding = new RectOffset(3, 3, 3, 3);

            GUIStyle gs = null;
            LockedHeaderStyle = new GUIStyle(EditorStyles.label);
            LockedCellStyle = new GUIStyle(EditorStyles.textField);
            ScrollableHeaderStyle = new GUIStyle(EditorStyles.label);
            ScrollableCellStyle = new GUIStyle(EditorStyles.textField);
            NewDataCellStyle = new GUIStyle(EditorStyles.textField);
            PlaceHolderTextStyle = new GUIStyle(EditorStyles.textField);
            ButtonStyle = new GUIStyle("button");
            
            // NAME, DESCRIPTION, etc.
            gs = LockedHeaderStyle;
            gs.fontStyle = FontStyle.Bold;
            gs.alignment = TextAnchor.MiddleCenter;
            gs.margin = margin;
            gs.padding = padding;
            
            gs = LockedCellStyle;
            gs.margin = margin;
            gs.padding = padding;
            gs.alignment = TextAnchor.MiddleLeft;
            
            // All custom fields
            gs = ScrollableHeaderStyle;
            gs.fontStyle = FontStyle.Bold;
            gs.alignment = TextAnchor.MiddleCenter;
            gs.margin = margin;
            gs.padding = padding;
            
            gs = ScrollableCellStyle;
            gs.margin = margin;
            gs.padding = padding;
            gs.alignment = TextAnchor.MiddleRight;

            // New entry fields
            gs = NewDataCellStyle;
            gs.margin = margin;
            gs.padding = padding;
            gs.alignment = TextAnchor.MiddleRight;

            gs = PlaceHolderTextStyle;
            gs.margin = margin;
            gs.padding = padding;
            gs.alignment = TextAnchor.MiddleRight;
            gs.fontStyle = FontStyle.Italic;

            gs = ButtonStyle;
            gs.padding = padding;
            gs.margin = margin;

            // Init context menu
            contextMenu = new GenericMenu ();
            contextMenu.AddItem(new GUIContent("Remove Field"), false, ContextMenuRemoveField);
        }

        public void OnDrawCellGUI(int columnIndex, int rowIndex, Rect position)
        {
            if (rowIndex == 0) // Header
                HeaderCellGUI(columnIndex, position);
            else if (columnIndex < mDataSource.NumFields)
                BodyCellGUI(rowIndex - 1, columnIndex, position);
            else
            {
                Rect newPosition = new Rect(position.x, position.y + position.height * 0.1f, position.width / 2, position.height * 0.8f);
                ControlCellGUI(rowIndex - 1, newPosition);
            }
        }

        private void ContextMenuRemoveField()
        {
            mDataSource.RemoveField (curFieldClicked);
        }

        private void HeaderCellGUI(int fieldIndex, Rect position)
        {
            if (fieldIndex < mDataSource.NumFields)
            {
                GUIStyle style = (fieldIndex < VerticalDividerPosition ? LockedHeaderStyle : ScrollableHeaderStyle);
                EditorGUI.LabelField(position, mDataSource.FieldNames [fieldIndex], style);
                Event curEvent = Event.current;
                if(mDataSource.IsFieldRemovable(fieldIndex) && curEvent.type == EventType.ContextClick && position.Contains(curEvent.mousePosition))
                {
                    curFieldClicked = fieldIndex;
                    contextMenu.ShowAsContext();
                }
            } else // New field control
            {
                GUI.Box(position, new GUIContent("", "Enter new field name"), NewDataCellStyle);
                string newFieldName;
                GUIStyle newFieldStyle;
                if (string.IsNullOrEmpty(mNewFieldName))
                {
                    newFieldName = NewFieldPlaceHolderText;
                    newFieldStyle = PlaceHolderTextStyle;
                }
                else
                {
                    newFieldName = mNewFieldName;
                    newFieldStyle = NewDataCellStyle;
                    if (Event.current.type == EventType.KeyDown && (Event.current.keyCode == KeyCode.Return))
                    {
                        AddNewField();
                    }
                }
                GUI.SetNextControlName(NEWFIELD_CONTROL_NAME);
                newFieldName = EditorGUI.TextField(position, newFieldName, newFieldStyle);
                if (newFieldName != NewFieldPlaceHolderText)
                {
                    mNewFieldName = newFieldName;
                }
            }
        }
        
        private void BodyCellGUI(int entryIndex, int fieldIndex, Rect position)
        {
            Status status = mDataSource.GetEntryStatus(entryIndex);
            Color originalColor = GUI.color;
            Color launchColor = originalColor;
            string entryTooltip = mDataSource.GetEntryTooltip(entryIndex, fieldIndex);
            switch (status)
            {
                case Status.UNMODIFIED:
                    launchColor = colorUnmodified;
                    break;
                case Status.NEW:
                    launchColor = colorNew;
                    break;
                case Status.DELETED:
                    launchColor = colorDeleted;
                    break;
                case Status.MODIFIED:
                    launchColor = colorModified;
                    break;
            }
            GUI.color = launchColor;
            if (entryIndex < mDataSource.NumEntries)
            {
                GUIStyle style = (fieldIndex < VerticalDividerPosition ? LockedCellStyle : ScrollableCellStyle);
                object entryValueObj = mDataSource.GetEntryValue(entryIndex, fieldIndex);
                string entryValue = Util.Print(entryValueObj);
                if (entryValueObj != null)
                {
                    if (entryValueObj.GetType() == typeof(Dictionary<string, object>))
                    {
                        entryValue = entryValue.Replace("\n", "");
                        entryValue = entryValue.Replace("\t", "");
                    }
                }
                if (status != Status.DELETED)
                {
                    GUI.Box(position, new GUIContent("", entryTooltip), style);
                    string newEntryValue = EditorGUI.TextField(position, entryValue, style);
                    if (newEntryValue != null && !newEntryValue.Equals(entryValue))
                    {
                        mDataSource.UpdateEntryValue(entryIndex, fieldIndex, newEntryValue);
                    }
                } else
                {
                    EditorGUI.LabelField(position, entryValue, style);
                }
            } else // New entry controls
            {
                if (mNewEntryValues == null)
                {
                    mNewEntryValues = new string[mDataSource.NumFields];
                }
                string controlName = string.Format("{0}:{1}:{2}", NEWENTRY_CONTROL_NAME, entryIndex, fieldIndex);
                GUI.SetNextControlName(controlName);
                GUI.Box(position, new GUIContent("", entryTooltip), NewDataCellStyle);
                mNewEntryValues [fieldIndex] = EditorGUI.TextField(position, mNewEntryValues [fieldIndex], NewDataCellStyle);
            }
            GUI.color = originalColor;
        }

        private void ControlCellGUI(int entryIndex, Rect position)
        {
            if (entryIndex < mDataSource.NumEntries)
            {
                Status status = mDataSource.GetEntryStatus(entryIndex);
                ButtonState buttonState = ButtonState.DELETE;
                string buttonText = null;
                string buttonTooltip = null;
                switch (status)
                {
                    case Status.UNMODIFIED:
                    case Status.NEW:
                        buttonState = ButtonState.DELETE;
                        buttonText = "DELETE";
                        buttonTooltip = "Delete the entry";
                        break;
                    case Status.DELETED:
                    case Status.MODIFIED:
                        buttonState = ButtonState.RESTORE;
                        buttonText = "RESTORE";
                        buttonTooltip = "Restore the entry to orginal value";
                        break;
                }

                if (GUI.Button(position, new GUIContent(buttonText, buttonTooltip), ButtonStyle))
                {
                    if (buttonState == ButtonState.RESTORE)
                        mDataSource.RestoreEntry(entryIndex);
                    else // deleting
                        mDataSource.DeleteEntry(entryIndex);
                }
            }
        }

        public void GUIUpdate()
        {
            string focusedControl = GUI.GetNameOfFocusedControl();

            if (Event.current.type == EventType.Repaint)
            {
                Vector2 coordinate = GetEntryCoordinate(focusedControl);
                if (mLastFocusedCell != NULL_VECTOR)
                {
                    // Check if new entry should be created
                    if (coordinate != mLastFocusedCell && 
                        !string.IsNullOrEmpty(mNewEntryValues [(int)(mLastFocusedCell.x)]))
                    {
                        int entryIndex = (int)mLastFocusedCell.y;
                        if (mDataSource.AddNewEntry(entryIndex))
                        {
                            for (int i=0; i < mNewEntryValues.Length; ++i)
                            {
                                mDataSource.UpdateEntryValue(entryIndex, i, mNewEntryValues [i]);
                            }
                        }
                        mNewEntryValues = null;
                    }
                }
                mLastFocusedCell = coordinate;

                // Check if new field should be created
                if (focusedControl != NEWFIELD_CONTROL_NAME && !string.IsNullOrEmpty(mNewFieldName))
                {
                    AddNewField();
                }
            }
        }

        private void AddNewField()
        {
            mDataSource.AddNewField(mNewFieldName);
            mNewFieldName = "";
            string[] newEntryValues = new string[mDataSource.NumFields];
            mNewEntryValues.CopyTo(newEntryValues, 0);
            mNewEntryValues = newEntryValues;
            mWindow.Repaint();
        }

        public void Refresh()
        {
            mNewFieldName = "";
            mNewEntryValues = null;
            mLastFocusedCell = NULL_VECTOR;
        }
        
        private Vector2 GetEntryCoordinate(string controlName)
        {
            Vector2 coordinate = NULL_VECTOR;
            if (!string.IsNullOrEmpty(controlName))
            {
                string[] parts = controlName.Split(':');
                if (parts != null && parts.Length == 3 && parts [0].Equals(NEWENTRY_CONTROL_NAME))
                {
                    coordinate = new Vector2(Convert.ToInt32(parts [2]), Convert.ToInt32(parts [1]));
                }
            }
            return coordinate;
        }
    }
}
