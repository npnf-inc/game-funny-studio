using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

namespace NPNF.UI
{
	public interface ITableDataSource
	{
		int NumFields { get; }
		int NumEntries { get; }
		List<string> FieldNames { get; }

		void AddNewField(string newFieldName);
		bool IsFieldRemovable(int fieldIndex);
		void RemoveField(int fieldIndex);
		bool AddNewEntry(int entryIndex);
		object GetEntryValue(int entryIndex, int fieldIndex);
		string GetEntryTooltip(int entryIndex, int fieldIndex);
		void UpdateEntryValue(int entryIndex, int fieldIndex, string value);

		void RestoreEntry(int entryIndex);
		void DeleteEntry(int entryIndex);

		TableAdapter.Status GetEntryStatus (int entryIndex);
	}
}