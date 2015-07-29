using System;
using System.Collections;
using System.Collections.Generic;
using NPNF.Core.CollectionModule;
using NPNF.Core;
using NPNF.Core.Admin;
using NPNF.UI;
using NPNF.Lib.JsonUtil;
using NPNF;
using UnityEngine;

public delegate void LoadedEventHandler(object sender, EventArgs e);

public class AssetManager : ScriptableObject, ITableDataSource
{
    private const string USER_ID = "asset_manager";
	private const string NAME_FIELD = "NAME";
	private const string DESCRIPTION_FIELD = "DESCRIPTION";

    public List<string> AssetTypes { get; private set; }
	public string CurrentAssetType { internal get; set; }
	private Dictionary<string, List<UpdatableAsset>> mAssets = new Dictionary<string, List<UpdatableAsset>>();
	private Dictionary<string, List<string>> mAssetFields;

    private AdminManager mAdmin;
    private CoroutineQueue mQueue;

    private void OnEnable()
	{
        mQueue = new CoroutineQueue();
        mAdmin = AdminManager.Create(NPNFSettings.Instance, NPNFSettings.Instance, USER_ID, mQueue);
        ResetData();
	}

    public void Update()
    {
        if (mQueue != null)
        {
            mQueue.Update();
        }
    }

    private void ResetData()
    {
        mAssetFields = null; // force reset of asset fields
        mAssets = new Dictionary<string, List<UpdatableAsset>>();
        AssetTypes = new List<string>();
        CurrentAssetType = null;
    }

    public void GetVersions(Action<List<NPNF.Core.Configuration.Version>, NPNFError> callback)
    {
        mAdmin.GetAllVersions((List<NPNF.Core.Configuration.Version> versions, NPNFError error)=>{
            callback(versions, error);
        });
    }

    public void SetVersion(string version)
    {
        mAdmin.UpdateVersions(version);
    }

    public void LoadAssets(Action<NPNFError> callback)
	{
		mAdmin.GetAllAssets((List<Asset> assets, NPNFError error)=>{
			mAssets = new Dictionary<string, List<UpdatableAsset>>();
            if (assets != null)
			{
				foreach(Asset asset in assets)
				{
					string assetType = asset.AssetType;
					List<UpdatableAsset> updatableAssets = null;
					if (mAssets.ContainsKey(assetType))
					{
						updatableAssets = mAssets[assetType];
					}
					else
					{
						updatableAssets = new List<UpdatableAsset>();
					}
					updatableAssets.Add(new UpdatableAsset(asset));
					mAssets[assetType] = updatableAssets;
				}
			}
            callback(error);
		});
	}

	public void LoadTypes(Action<NPNFError> callback)
	{
		Asset.GetAllTypes ((List<string> types, NPNFError error) => {
			// check if assettypes empty null
			if (types != null && types.Count != 0)
			{
				AssetTypes = types;
				CurrentAssetType = AssetTypes[0];
			}
			else
			{
				AssetTypes = new List<string>();
				CurrentAssetType = null;
			}
			callback(error);
		});
	}

	public void SaveData(Action<string> saveCallback)
	{
        string output = "";
        bool processedAllAssets = false;
        int counter = 0;
		bool savingSomething = false;

        Action TriggerSaveCallbackIfReady = () => {
			if (processedAllAssets && counter == 0 && saveCallback != null)
			{
				saveCallback(output);
			}
		};

        foreach(List<UpdatableAsset> assets in mAssets.Values)
		{
			for (int i = assets.Count-1; i >= 0; --i)
			{
                UpdatableAsset asset = assets[i];

                if (asset.CurrentStatus != UpdatableAsset.Status.EXISTING || asset.Modified)
                {
                    counter++;
					savingSomething = true;

                    Action<Asset, NPNFError> innerSaveCallback = (Asset resultAsset, NPNFError error) => {
                        counter--;
                        string assetName = (resultAsset != null ? resultAsset.Name : asset.Name);
                        if (error != null)
                        {
                            Debug.Log("Saving asset failed: " + assetName);
                            output += "Saving asset failed: " + assetName + "\n";
                        }
                        else
                        {
                            Debug.Log("Saving asset succeeded: " + assetName);
                            if (resultAsset != null)
                            {
                                asset.BackingAsset = resultAsset;
                            }
                        }
                        TriggerSaveCallbackIfReady();
                    };
                    if (asset.CurrentStatus == UpdatableAsset.Status.NEW || asset.Modified)
					{
						if (!AssetNameExists(asset)) 
						{
                            if (asset.Modified)
                                mAdmin.UpdateAsset(asset, innerSaveCallback);
                            else
							    mAdmin.CreateAsset(asset, innerSaveCallback);
						}
						else
						{
                            counter--;
                            Debug.Log("Save failed: Asset name already exists: " + asset.Name);
							output += "Save failed: Asset name already exists: " + asset.Name + "\n";
						}
					} else if (asset.CurrentStatus == UpdatableAsset.Status.DELETED)
                    {
						mAdmin.DeleteAsset(asset, (Asset resultAsset, NPNFError error)=>{
                            counter--;
							string assetName = (resultAsset != null ? resultAsset.Name : asset.Name);
							if (error != null)
							{
								Debug.Log("Deleting asset failed: " + assetName);
                                output += "Deleting asset failed: " + assetName;
                            }
                            else
                            {
								Debug.Log("Deleting asset succeeded: " + assetName);
								List<UpdatableAsset> assetsToRemoveFrom = mAssets[resultAsset.AssetType];
								assetsToRemoveFrom.Remove(asset);
							}
                            TriggerSaveCallbackIfReady();
                        });
					}
                }
            }
        }
		if (!savingSomething)
        {
            saveCallback("No changes were made. Nothing to save.");
        } else
        {
            processedAllAssets = true;
            TriggerSaveCallbackIfReady();
        }
    }

	private bool AssetNameExists(UpdatableAsset updateAsset)
	{
		foreach (var p in mAssets)
		{
			foreach (UpdatableAsset asset in p.Value)
			{
				if (updateAsset != asset && updateAsset.Name == asset.Name)
				{
					return true;
				}
			}
		}
		return false;
	}

	public void AddAssetType(string value)
	{
		if (!mAssets.ContainsKey(value))
		{
			AssetTypes.Add(value);
			mAssets.Add(value, new List<UpdatableAsset>());
		}
	}

	public void RemoveAssetType(string value)
	{
		List<UpdatableAsset> assets = mAssets[value];
		foreach (UpdatableAsset asset in assets)
		{
			if (asset.CurrentStatus != UpdatableAsset.Status.NEW)
			{
				mAdmin.DeleteAsset(asset, (Asset resultAsset, NPNFError error) => {
					string assetName = (resultAsset != null ? resultAsset.Name : "<Name Unknown>");
					if (error != null)
					{
						Debug.Log("Deleting asset failed: " + assetName);
					}
					else
					{
						Debug.Log("Deleting asset succeeded: " + assetName);

					}
				});
			}
		}
		mAssets.Remove(value);
		AssetTypes.Remove(value);
        if (CurrentAssetType == value)
        {
            CurrentAssetType = null;
        }
	}

    public void ChangeAssetType(string oldValue, string newValue, Action<bool> callback)
    {
        List<UpdatableAsset> assets = mAssets[oldValue];

        if (assets != null && assets.Count > 0)
        {
            foreach (UpdatableAsset asset in assets)
            {
                asset.AssetType = newValue;
            }
            mAssets.Remove(oldValue);
            mAssets.Add(newValue, assets);
            SaveData((string callbackStr) => {
                if (string.IsNullOrEmpty(callbackStr))
                {
                    AssetTypes[AssetTypes.IndexOf(oldValue)] = newValue;
                    CurrentAssetType = newValue;
                }
                else
                {
                    assets = mAssets[newValue];
                    foreach (UpdatableAsset asset in assets)
                    {
                        asset.AssetType = oldValue;
                    }
                    mAssets.Remove(newValue);
                    mAssets.Add(oldValue, assets);
                }

                if (callback != null)
                    callback(string.IsNullOrEmpty(callbackStr));
            });
        }
        else
        {
            mAssets.Remove(oldValue);
            mAssets.Add(newValue, assets);
            AssetTypes[AssetTypes.IndexOf(oldValue)] = newValue;
            CurrentAssetType = newValue;
            if (callback != null)
                callback(true);

        }
        
    }
    
    public List<UpdatableAsset> GetAssets(string assetType)
	{
		return mAssets[assetType];
    }

	public List<string> GetAssetFields(string assetType)
	{
        if (assetType == null)
        {
            throw new ArgumentNullException(assetType);
        }
		if (mAssetFields == null)
		{
			mAssetFields = new Dictionary<string, List<string>>();
		}
		if (!mAssetFields.ContainsKey(assetType))
		{
			List<string> fieldList = new List<string>();
            fieldList.Add(NAME_FIELD);
            fieldList.Add(DESCRIPTION_FIELD);
            if (mAssets.ContainsKey(assetType))
            {
			    List<UpdatableAsset> assets = mAssets[assetType];
                foreach(UpdatableAsset asset in assets)
                {
                    if (asset.Custom != null)
                    {
                        foreach(string field in asset.Custom.Keys)
                        {
                            if (!fieldList.Contains(field))
                            {
                                fieldList.Add(field);
                            }
                        }
                    }
                }
            }
            else
            {
                mAssets.Add(assetType, new List<UpdatableAsset>());
            }
			mAssetFields[assetType] = fieldList;
		}
		return mAssetFields[assetType];
	}

	public void AddAsset (UpdatableAsset asset)
	{
		mAssets[asset.AssetType].Add(asset);
	}

	public void RemoveAsset (UpdatableAsset asset)
	{
		mAssets[asset.AssetType].Remove(asset);
	}

	public void AddAssetField(string fieldName, string assetType)
	{
		List<string> fields = GetAssetFields(assetType);
		if (!fields.Contains(fieldName))
		{
			fields.Add(fieldName);
		}
	}

	public void RemoveAssetField(int fieldIndex, string assetType)
	{
		List<string> fields = GetAssetFields(assetType);
		if(fieldIndex < fields.Count)
		{
			string fieldName = fields[fieldIndex];
			fields.RemoveAt(fieldIndex);
			List<UpdatableAsset> assets = new List<UpdatableAsset>(mAssets[CurrentAssetType]);
			foreach(UpdatableAsset asset in assets)
			{
				asset.RemoveCustomField(fieldName);
			}
		}
	}

	#region ITableDataSource implementation

	public void AddNewField (string newFieldName)
	{
		AddAssetField (newFieldName, CurrentAssetType);
	}
	
	public void RemoveField(int fieldIndex)
	{
		RemoveAssetField (fieldIndex, CurrentAssetType);
	}
	
	public bool IsFieldRemovable(int fieldIndex)
	{
		// Custom field has index > 1
		return fieldIndex > 1;
	}

	public int NumFields
	{
		get
		{
			int num = 0;
			if (FieldNames != null)
			{
				num = FieldNames.Count;
			}
			return num;
		}
	}

	public int NumEntries
	{
		get
		{
			int num = 0;
			if (CurrentAssetType != null)
			{
				num = mAssets[CurrentAssetType].Count;
			}
			return num;
		}
	}

	public List<string> FieldNames {
		get
		{
			List<string> fieldNames = null;
			if (CurrentAssetType != null)
			{
				fieldNames = GetAssetFields(CurrentAssetType);
			}
			return fieldNames;
		}
	}

	public object GetEntryValue (int entryIndex, int fieldIndex)
	{
		object entryValue = "";
		if (CurrentAssetType != null)
		{
			string fieldValue = mAssetFields[CurrentAssetType][fieldIndex];
			UpdatableAsset asset = mAssets[CurrentAssetType][entryIndex];
			switch (fieldValue)
			{
			case NAME_FIELD:
				entryValue = asset.Name;
				break;
			case DESCRIPTION_FIELD:
				entryValue = asset.Description;
				break;
			default:
				if (asset.Custom != null && asset.Custom.ContainsKey(fieldValue))
				{
                    entryValue = asset.Custom[fieldValue];
				}
				break;
			}
		}
		return entryValue;
	}

	public string GetEntryTooltip (int entryIndex, int fieldIndex)
	{
		string tooltipStr = "";
		if (CurrentAssetType != null) {
			string fieldValue = mAssetFields [CurrentAssetType] [fieldIndex];
			switch (fieldValue) {
			case NAME_FIELD:
				tooltipStr = "Enter asset name (required)";
				break;
			case DESCRIPTION_FIELD:
				tooltipStr = "Enter asset description";
				break;
			default:
				tooltipStr = string.Format ("Enter a value for the {0} field", fieldValue);
				break;
			}
		}
		return tooltipStr;
	}

	public void UpdateEntryValue (int entryIndex, int fieldIndex, string value)
	{
		if (CurrentAssetType != null)
		{
			string fieldValue = mAssetFields[CurrentAssetType][fieldIndex];
			UpdatableAsset asset = mAssets[CurrentAssetType][entryIndex];
			switch (fieldValue)
			{
			case NAME_FIELD:
				asset.Name = value;
				break;
			case DESCRIPTION_FIELD:
				asset.Description = value;
				break;
			default:
				if (asset.Custom == null)
				{
					asset.Custom = new Dictionary<string, object>();
				}
				asset.UpdateCustomField(fieldValue, value);
				break;
			}
		}
	}

	public bool AddNewEntry(int entryIndex)
	{
		bool success = false;
		if (CurrentAssetType != null)
		{
			UpdatableAsset asset = new UpdatableAsset(CurrentAssetType);
			AddAsset(asset);
			success = true;
		}
		return success;
	}

	public void RestoreEntry (int entryIndex)
	{
		if (CurrentAssetType != null)
		{
			UpdatableAsset asset = mAssets[CurrentAssetType][entryIndex];
			asset.Restore();
		}
	}

	public void DeleteEntry (int entryIndex)
	{
		if (CurrentAssetType != null)
		{
			UpdatableAsset asset = mAssets[CurrentAssetType][entryIndex];
			if (asset.CurrentStatus == UpdatableAsset.Status.NEW)
			{
				mAssets[asset.AssetType].Remove(asset);
			}
			else
			{
				asset.MarkDeleted();
			}
		}
	}

	public TableAdapter.Status GetEntryStatus (int entryIndex)
	{
		TableAdapter.Status ret = TableAdapter.Status.UNMODIFIED;
		if (CurrentAssetType != null && entryIndex < mAssets[CurrentAssetType].Count)
		{
			UpdatableAsset asset = mAssets[CurrentAssetType][entryIndex];
			switch (asset.CurrentStatus)
			{
			case UpdatableAsset.Status.EXISTING:
				if (!asset.Modified)
					ret = TableAdapter.Status.UNMODIFIED;
				else
					ret = TableAdapter.Status.MODIFIED;
				break;
			case UpdatableAsset.Status.NEW:
				ret = TableAdapter.Status.NEW;
				break;
			case UpdatableAsset.Status.DELETED:
				ret = TableAdapter.Status.DELETED;
				break;
			}
		}
		return ret;
	}

	#endregion
}
