using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using NPNF.Core.CollectionModule;
using NPNF.UI;
using System;
using NPNF.Core;
using System.Reflection;

namespace NPNF.Editor
{
    public class NPNFAssetsEditor : EditorWindow
    {
        private const string DIALOG_CANCEL_BTN = "Cancel";
        private const string DIALOG_DELETE_BTN = "Delete";
        private const string DIALOG_RELOAD_BTN = "Reload";
        private const string DIALOG_OK_BTN = "OK";
        private const int CELL_WIDTH = 150;
        private const int CELL_HEIGHT = 20;
        private const int MARGIN = 20;
        private const int TASKBAR_HEIGHT = 60;
        private const float BUTTON_SPACING = CELL_WIDTH * 0.25f;
        private const string NEW_COLLECTION_CONTROL_NAME = "NewCollectionNameLabel";
        private const string NEW_COLLECTION_PLACEHOLDER_TEXT = "Enter a Collection Name";
        private bool mAssetsLoaded = false;
        private bool mTypesLoaded = false;
        private bool mWindowResizeSwitch = true;
        private AssetManager mAssetManager;
        private int mSelectedTypeIndex = -1;
        private Rect mRect;
        private TableView mAssetsTable = null;
        private TableAdapter mAdapter = null;
        private List<NPNF.Core.Configuration.Version> versions;
        private int _mSelectedVersionIndex = -1;
        private GUIContent[] assetTypesContent = null;
        private GUIContent[] versionsContent = null;
        private bool isEditCollectionName = false;
        private string savingStatus = "";
        private string forceFocusControl = null;
        private bool saveButtonPressed;
        private string currentSaveButtonText = "SAVE";
        private string saveButtonNotPressedText = "SAVE";
        private string saveButtonPressedText = "Saving...";

        private int mSelectedVersionIndex
        {
            get
            {
                return _mSelectedVersionIndex;
            }
            set
            {
                if (_mSelectedVersionIndex != value)
                {
                    if (PopDialogCommandsGUI("Switch Version", 
                                         "Are you sure you want to switch to another version? All changes that have not been saved will be removed permanently.",
                                         DIALOG_OK_BTN,
                                         DIALOG_CANCEL_BTN))
                    {
                        mAssetManager.SetVersion(versions [value].ClientVersion);
						_mSelectedVersionIndex = value;
						Reload(null);
                    }
                }
            }
        }
        
        // New Collection Type
        private string mNewCollectionName = "";
        private bool mClearNewCollectionName = false;
        private GUIStyle mCustomFieldTextStyle;
        private GUIStyle mSelectionLabelStyle;
        private GUIStyle mPlaceHolderTextStyle;
        private GUIStyle mCollectionDataTextStyle;
        private GUIStyle mCollectionNameButtonStyle;

        [MenuItem ("NPNF/Asset Manager")]
        public static void ShowWindow()
        {
            EditorWindow.GetWindow(typeof(NPNFAssetsEditor));
        }

        private void OnEnable()
        {
            this.Init();
        }

        private void Init()
        {
            RectOffset margin = new RectOffset(4, 4, 4, 4);
            RectOffset padding = new RectOffset(3, 3, 3, 3);
            mCustomFieldTextStyle = new GUIStyle(EditorStyles.textField);
            mCustomFieldTextStyle.margin = margin;
            mCustomFieldTextStyle.padding = padding;
            mCustomFieldTextStyle.alignment = TextAnchor.MiddleRight;

            mPlaceHolderTextStyle = new GUIStyle(EditorStyles.textField);
            mPlaceHolderTextStyle.alignment = TextAnchor.MiddleLeft;
            mPlaceHolderTextStyle.fontStyle = FontStyle.Italic;

            mCollectionDataTextStyle = new GUIStyle(EditorStyles.textField);
            mCollectionDataTextStyle.alignment = TextAnchor.MiddleLeft;

            mCollectionNameButtonStyle = new GUIStyle(EditorStyles.miniButtonMid);
            mCollectionNameButtonStyle.fontSize = 11;
            mCollectionNameButtonStyle.alignment = TextAnchor.MiddleLeft;

            mAssetManager = ScriptableObject.CreateInstance("AssetManager") as AssetManager;
            mSelectedTypeIndex = -1;
            mAdapter = new TableAdapter(mAssetManager, this, 2);
            RefreshVersions();
        }

        private void RefreshVersions()
        {
            mAssetManager.GetVersions((List<NPNF.Core.Configuration.Version> versions, NPNFError error) => {
                this.versions = versions;
                if (error != null)
                {
                    PopDialogCommandsGUI("Server unavailable or network not setup properly", 
                                         "Please try again later.", 
                                         DIALOG_OK_BTN);
                } else
                {
                    if (versions.Count > 0)
                    {
                        versionsContent = new GUIContent[versions.Count];
                        for (int i=0; i<versions.Count; i++) 
                            versionsContent [i] = new GUIContent(versions[i].ClientVersion, "Select App Version");
                        
                        _mSelectedVersionIndex = GetCurrentVersionIndex();
                        mAssetManager.SetVersion(versions [_mSelectedVersionIndex].ClientVersion);
                        Reload(null);
                    } else
                    {
                        PopDialogCommandsGUI("No Version found", 
                                             "Please create a verion in npnf developer portal.", 
                                             DIALOG_OK_BTN);
                    }
                }
            });
        }

        private int GetCurrentVersionIndex()
        {
            for (int i = 0; i < versions.Count; i++)
            {
                if (versions [i].ClientVersion.Equals(NPNFSettings.Instance.AppVersion))
                {
                    return i;
                }
            }
            return 0;
        }

        private void Reload(Action<NPNFError> callback)
        {
            mAssetsLoaded = false;
            mTypesLoaded = false;
            NPNFError loadError = null;

            mAssetManager.LoadAssets((NPNFError error) => {
                if (error != null)
                {
                    loadError = error;
                    PopDialogCommandsGUI("Assets Cannot be Loaded", 
                                         "Please try again later.", 
                                         DIALOG_OK_BTN);
                }
                mAssetsLoaded = true;
                mWindowResizeSwitch = true;
                if (callback != null && mTypesLoaded)
                {
                    callback(loadError);
                }
            });
            mAssetManager.LoadTypes((NPNFError error) => {
                if (error != null)
                {
                    loadError = error;
                    PopDialogCommandsGUI("Types Cannot be Loaded",
                                         "Please try again later.",
                                         DIALOG_OK_BTN);
                }
                mTypesLoaded = true;
                mWindowResizeSwitch = true;
                SetupTypes(mSelectedTypeIndex);
                if (callback != null && mAssetsLoaded)
                {
                    callback(loadError);
                }
            });
        }
        
        private void SetupTypes(int selectedTypeIndex)
        {
            mSelectedTypeIndex = selectedTypeIndex;
            if (mAssetManager.AssetTypes.Count > 0)
            {
				if (mSelectedTypeIndex == -1 || mSelectedTypeIndex >= mAssetManager.AssetTypes.Count)
				{
					mSelectedTypeIndex = 0;
				}
                mAssetManager.CurrentAssetType = mAssetManager.AssetTypes [mSelectedTypeIndex];
                assetTypesContent = new GUIContent[mAssetManager.AssetTypes.Count];
                for (int i=0; i<mAssetManager.AssetTypes.Count; i++)
                    assetTypesContent[i] = new GUIContent(mAssetManager.AssetTypes[i], mAssetManager.AssetTypes[i]);
                mWindowResizeSwitch = true;
            }
            else
			{
				mSelectedTypeIndex = -1;
				mAssetManager.CurrentAssetType = null;
			}
            mAdapter.Refresh();
            Repaint();
        }

        private void ResizeAndFitRectOnScreen(Rect rectCurrent)
        {
            BindingFlags fullBinding = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
            MethodInfo isDockedMethod = typeof(EditorWindow).GetProperty("docked", fullBinding).GetGetMethod(true);
            if (isDockedMethod == null || !(bool)isDockedMethod.Invoke(this, null))
            {
                // Resizing rect
                Rect rect = this.position;
                float defaultWidth = 5 * CELL_WIDTH + 2 * MARGIN;
                float defaultHeight = rectCurrent.y + MARGIN;
                if (this.position.width < defaultWidth)
                    rect.width = defaultWidth;
                if (this.position.height < defaultHeight)
                    rect.height = defaultHeight;
                if (rect.height > Screen.currentResolution.height)
                    rect.height = Screen.currentResolution.height - TASKBAR_HEIGHT;

                // Shifiting position
                float windowRight = rect.x + rect.width;
                float windowBottom = rect.y + rect.height;
                float screenWidth = Screen.currentResolution.width;
                float screenHeight = Screen.currentResolution.height;
                if (windowRight > screenWidth)
                    rect.x -= windowRight - screenWidth;
                if (windowBottom > screenHeight)
                    rect.y -= windowBottom - screenHeight;
                this.position = rect;
            }
        }

        private void OnGUI()
        {
            mRect = new Rect(MARGIN, MARGIN, CELL_WIDTH, CELL_HEIGHT);
            VersionsGUI();
            if (!mTypesLoaded && !mAssetsLoaded)
            {
                mRect.width = CELL_WIDTH * 3;
                EditorGUI.LabelField(mRect, isEditCollectionName ? "Collection saving..." : "Loading collections and assets...");
            } else if (!mTypesLoaded && mAssetsLoaded)
            {
                mRect.width = CELL_WIDTH * 3;
                EditorGUI.LabelField(mRect, isEditCollectionName ? "Collection saving..." : "Assets loaded. Waiting for collections...");
            } else if (mTypesLoaded)
            {
                if (isEditCollectionName)
                {
                    EditCollectionUI();
                } else
                {
                    CollectionsGUI();
                }

                if (!mAssetsLoaded)
                {
                    EditorGUI.LabelField(mRect, isEditCollectionName ? "Collection saving..." : "Loading assets...");
                } else
                {
                    if (mAssetManager.CurrentAssetType != null)
                    {
                        mRect.width = Screen.width;
                        EditorGUI.LabelField(mRect, "Assets for Collection " + mAssetManager.CurrentAssetType, mSelectionLabelStyle);
                        mRect.x = MARGIN;
                        mRect.y += mRect.height * 1.5f;
                        mRect.width = CELL_WIDTH;

                        Vector2 maxTableSize = new Vector2(Screen.width - mRect.x - MARGIN, Screen.height - mRect.y - MARGIN - CELL_HEIGHT * 5);

                        if (mAdapter == null)
                        {
                            mAdapter = new TableAdapter(mAssetManager, this, 2);
                        }
                        if (mAssetsTable == null)
                        {
                            mAssetsTable = new TableView(mAdapter);
                        } else
                        {
                            mAssetsTable.SetAdapter(mAdapter);
                        }
                        mAssetsTable.UpdateLocation(new Vector2(mRect.x, mRect.y), CELL_WIDTH, CELL_HEIGHT);

                        mAssetsTable.OnGUI();
                        mAssetsTable.MaxSize = maxTableSize;
                        mRect.y = mAssetsTable.PositionRect.y + mAssetsTable.PositionRect.height;
                        ManagerCommandsGUI();
                        mRect.y += (mAssetManager.NumEntries + 1) * CELL_HEIGHT;
                    }
                }
                Repaint();
            }
            mRect.y += mRect.height;

            if (mWindowResizeSwitch)
            {
                ResizeAndFitRectOnScreen(mRect);
                mWindowResizeSwitch = false;
            }

            if (Event.current.type == EventType.Repaint && forceFocusControl != null)
            {
                GUI.FocusControl(forceFocusControl);
                forceFocusControl = null;
            }
        }

        private void Update()
        {
            if (mAssetManager == null)
            {
                this.Init();
            }
            if (mClearNewCollectionName)
            {
                mNewCollectionName = "";
                mClearNewCollectionName = false;
            }
            if (mAssetManager != null)
            {
                mAssetManager.Update();
            }
        }

        private void VersionsGUI()
        {
            mRect.width = CELL_WIDTH * 2f;
            GUI.SetNextControlName("Version");
            this.mSelectedVersionIndex = EditorGUI.Popup(mRect, new GUIContent("Version"), this.mSelectedVersionIndex, versionsContent);

            mRect.x += mRect.width + 10;
            mRect.width = CELL_WIDTH;
            GUIContent reloadVersionsLabel = new GUIContent("Refresh", "Reload version");
            if (GUI.Button(mRect, reloadVersionsLabel))
            {
                isEditCollectionName = false;
                RefreshVersions();
            }

            mRect.x = MARGIN;
            mRect.y += mRect.height * 2;
        }
        
        private void CollectionsGUI()
        {
            mSelectionLabelStyle = new GUIStyle(EditorStyles.boldLabel);
            mSelectionLabelStyle.fontSize = 15;

            GUI.SetNextControlName("CollectionLabel");
            EditorGUI.LabelField(mRect, "Collections", mSelectionLabelStyle);
            mRect.y += mRect.height * 1.5f;
            GUI.SetNextControlName(NEW_COLLECTION_CONTROL_NAME);
            string newCollectionName;
            GUIStyle newFieldStyle;
            if (string.IsNullOrEmpty(mNewCollectionName))
            {
                newCollectionName = NEW_COLLECTION_PLACEHOLDER_TEXT;
                newFieldStyle = mPlaceHolderTextStyle;
            } else
            {
                newCollectionName = mNewCollectionName;
                newFieldStyle = mCollectionDataTextStyle;
                if (GUI.GetNameOfFocusedControl() == NEW_COLLECTION_CONTROL_NAME &&
                    Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Return)
                {
                    if (CreateNewCollection())
                    {
                        newCollectionName = "";
                        forceFocusControl = NEW_COLLECTION_CONTROL_NAME;
                    }
                }
            }
            newCollectionName = EditorGUI.TextField(mRect, newCollectionName, newFieldStyle);
            if (NEW_COLLECTION_CONTROL_NAME == GUI.GetNameOfFocusedControl() && newCollectionName != NEW_COLLECTION_PLACEHOLDER_TEXT)
            {
                mNewCollectionName = newCollectionName;
            }

            mRect.x += mRect.width;
            if (GUI.Button(mRect, "Create New Collection"))
            {
                CreateNewCollection();
            }

            mRect.x = MARGIN;
            mRect.y += mRect.height + CELL_HEIGHT;

            int selected = -1;
            if (mTypesLoaded)
            {
                EditorGUI.LabelField(mRect, "Select a Collection", EditorStyles.boldLabel);

                mRect.x += mRect.width;
                mRect.width = CELL_WIDTH;
                if (GUI.Button(mRect, new GUIContent("Delete Selected", "Delete Selected Collection")))
                {
                    if (PopDialogCommandsGUI("Delete Selected Collection", 
                                             "Are you sure you want to delete selected collection? All assets in the collection will be deleted!", 
                                             DIALOG_DELETE_BTN,
                                             DIALOG_CANCEL_BTN))
                    {
                        mAssetManager.RemoveAssetType(mAssetManager.CurrentAssetType);
                        if (mAssetManager.AssetTypes.Count > 0)
                        {
                            selected = 0;
                            SetupTypes(selected);
                        }
                    }
                }

                mRect.x += mRect.width;
                if (GUI.Button(mRect, new GUIContent("Rename Selected", "Rename Selected Collection")))
                {
                    savingStatus = "";
                    mNewCollectionName = mAssetManager.CurrentAssetType;
                    isEditCollectionName = true;
                }

                mRect.x = MARGIN;
                mRect.y += mRect.height + CELL_HEIGHT * 0.5f;

                int maxRowCellCount = 5;
                mRect.width = CELL_WIDTH * maxRowCellCount;
				if (mAssetManager.AssetTypes != null && mAssetManager.AssetTypes.Count > 0)
				{
                	mRect.height = Mathf.CeilToInt((float)mAssetManager.AssetTypes.Count / (float)maxRowCellCount) * CELL_HEIGHT;
				}
                if (mRect.height == 0)
                {
                    mRect.height = CELL_HEIGHT;
                }
				if (mSelectedTypeIndex >= 0)
				{
                    selected = GUI.SelectionGrid(mRect, mSelectedTypeIndex, assetTypesContent, maxRowCellCount, mCollectionNameButtonStyle);
				}
                if (selected != mSelectedTypeIndex)
                {
                    SetupTypes(selected);
                }

                if (!string.IsNullOrEmpty(GUI.tooltip))
                {
                    // hack to get tooltip to pop up in a box
                    GUI.Box(new Rect(Event.current.mousePosition.x, Event.current.mousePosition.y, 1, 1), new GUIContent("", GUI.tooltip));
                }

                mRect.y += mRect.height + CELL_HEIGHT * 1.5f;
                mRect.height = CELL_HEIGHT;
                mRect.width = CELL_WIDTH;
            }
        }

        private bool CreateNewCollection()
        {
            if (mNewCollectionName.Length > 0)
            {
                if (mAssetManager.AssetTypes.Contains(mNewCollectionName))
                {
                    PopDialogCommandsGUI("Create New Collection", 
                                         "Another collection with name " + mNewCollectionName + " already exists.", 
                                         DIALOG_OK_BTN);
                } else
                {
                    mAssetManager.AddAssetType(mNewCollectionName);
                    mNewCollectionName = "";
                    SetupTypes(mSelectedTypeIndex);
                    return true;
                }
            } else
            {
                PopDialogCommandsGUI("Create New Collection", 
                                     "Please provide a name for the new collection.", 
                                     DIALOG_OK_BTN);
            }
            return false;
        }

        private void EditCollectionUI()
        {
            mSelectionLabelStyle = new GUIStyle(EditorStyles.boldLabel);
            mSelectionLabelStyle.fontSize = 15;
            
            GUI.SetNextControlName("CollectionLabel");
            EditorGUI.LabelField(mRect, "Collections", mSelectionLabelStyle);
            mRect.y += mRect.height * 1.5f;

            mRect.width = CELL_WIDTH * 2;
            GUI.SetNextControlName("NewCollectionNameLabel");
            EditorGUI.LabelField(mRect, "Rename selected collection");

            mRect.y += mRect.height;
            GUI.Box(mRect, new GUIContent("", "Enter new collection name"));
            GUI.SetNextControlName("NewCollectionName");
            mNewCollectionName = EditorGUI.TextField(mRect, mNewCollectionName);
            
            mRect.y += mRect.height * 1.5f;
            mRect.width = CELL_WIDTH;
            if (GUI.Button(mRect, new GUIContent("Save", "Save all changes to the Platform server")))
            {
                savingStatus = "";
                if (!mAssetManager.AssetTypes [mSelectedTypeIndex].Equals(mNewCollectionName) && !mAssetManager.AssetTypes.Contains(mNewCollectionName) && !string.IsNullOrEmpty(mNewCollectionName))
                {
                    mAssetsLoaded = false;
                    mAssetManager.ChangeAssetType(mAssetManager.AssetTypes [mSelectedTypeIndex], mNewCollectionName, (bool succeeded) =>
                    {
                        if (succeeded)
                        {
                            mNewCollectionName = "";
                            SetupTypes(mSelectedTypeIndex);
                            isEditCollectionName = false;
                        } else
                        {
                            savingStatus = "Save failed. Please try again.";
                        }
                        mAssetsLoaded = true;
                    });
                } else
                {
                    PopDialogCommandsGUI("Rename collection",
                                         "Please provide another name for the collection. The new name must not be blank and cannot be the same as another collection.",
                                         DIALOG_OK_BTN);
                }
            }

            mRect.x += mRect.width;
            if (GUI.Button(mRect, "Cancel"))
            {
                isEditCollectionName = false;
            }

            if (!string.IsNullOrEmpty(savingStatus))
            {
                mRect.x = MARGIN;
                mRect.y += mRect.height * 1.5f;
                mRect.width = CELL_WIDTH * 2;
                EditorGUI.LabelField(mRect, savingStatus);
            }

            mRect.x = MARGIN;
            mRect.y += mRect.height + CELL_HEIGHT * 1.5f;
            mRect.height = CELL_HEIGHT;
            mRect.width = CELL_WIDTH;
        }

        private void UpdateSaveButtonText(bool buttonPressed)
        {
            saveButtonPressed = buttonPressed;
            if (saveButtonPressed)
            {
                currentSaveButtonText = saveButtonPressedText;
            } else
            {
                currentSaveButtonText = saveButtonNotPressedText;
            }
        }

        private void ManagerCommandsGUI()
        {
            EditorGUILayout.BeginHorizontal();
            mRect.y += mRect.height;
            mRect.height = mRect.height * 2f;
            if (GUI.Button(mRect, new GUIContent("RELOAD", "Reload the assets? All the unsaved changes will be removed permanently.")))
            {
                if (PopDialogCommandsGUI("Reload", 
                                          "Are you sure you want to reload the assets? All the unsaved changes will be removed permanently.",
                                          DIALOG_RELOAD_BTN, 
                                          DIALOG_CANCEL_BTN))
                {
                    Reload(null);
                }
            }
            mRect.x += mRect.width;
            mRect.x += BUTTON_SPACING;
            GUI.enabled = !saveButtonPressed;
            if (GUI.Button(mRect, currentSaveButtonText))
            {
                UpdateSaveButtonText(true);
                mAssetManager.SaveData((string error) => {
                    Repaint();
                    if (String.IsNullOrEmpty(error))
                    {
                        PopDialogInformGUI("Save",
                                           "All assets saved successfully.",
                                           DIALOG_OK_BTN);
                    } else
                    {
                        PopDialogInformGUI("Save",
                                           error,
                                           DIALOG_OK_BTN);
                    }
                    UpdateSaveButtonText(false);
                });
            }
            // Hide Help button
            /*
            mRect.x = 4*CELL_WIDTH + MARGIN - mRect.width;
            if (GUI.Button(mRect, new GUIContent("Help", "Open help documentation")))
            {
                Application.OpenURL("https://developer.npnf.com");
            }
            */
            mRect.y += mRect.height;
            EditorGUILayout.EndHorizontal();
        }

        private bool PopDialogInformGUI(string title, 
                                         string message, 
                                         string confirm = DIALOG_OK_BTN)
        {
            return EditorUtility.DisplayDialog(title, message, confirm);
        }

        private bool PopDialogCommandsGUI(string title, 
                                          string message = "Are you sure you want to perform this action?", 
                                          string confirm = "Confirm", 
                                          string cancel = null)
        {
            return EditorUtility.DisplayDialog(title, message, confirm, cancel);
        }
    }
}