using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace UnityEditor.NPNFEditor
{
	public class PBXBuildFile : PBXObject
	{
		private const string FILE_REF_KEY = "fileRef";
		private const string SETTINGS_KEY = "settings";
		private const string ATTRIBUTES_KEY = "ATTRIBUTES";
		private const string WEAK_VALUE = "Weak";
		private const string COMPILER_FLAGS_KEY = "COMPILER_FLAGS";
		
		public string name;
		
		public PBXBuildFile( PBXFileReference fileRef, bool weak = false ) : base()
		{
			this.Add( FILE_REF_KEY, fileRef.guid );
			SetWeakLink( weak );
			name = fileRef.name;
		}

		public PBXBuildFile( string guid, PBXDictionary dictionary ) : base ( guid, dictionary )
		{
			if(!this.data.ContainsKey(SETTINGS_KEY))
				return;
			object settingsObj = this.data[SETTINGS_KEY];
			
			if(!(settingsObj is PBXDictionary))
				return;
			PBXDictionary settingsDict = (PBXDictionary) settingsObj;
			settingsDict.internalNewlines = false;
			
			if( !settingsDict.ContainsKey(ATTRIBUTES_KEY) )
				return;
			object attributesObj = settingsDict[ATTRIBUTES_KEY];
			
			if(!(attributesObj is PBXList))
				return;
			
			PBXList attributesCast = (PBXList)attributesObj;
			attributesCast.internalNewlines = false;
		}

		public bool SetWeakLink( bool weak = false )
		{
			PBXDictionary settings = null;
			PBXList attributes = null;
			
			if( !_data.ContainsKey( SETTINGS_KEY ) ) {
				if( weak ) {
					attributes = new PBXList();
					attributes.internalNewlines = false;
					attributes.Add( WEAK_VALUE );
					
					settings = new PBXDictionary();
					settings.Add( ATTRIBUTES_KEY, attributes );
					settings.internalNewlines = false;
					
					this.Add( SETTINGS_KEY, settings );
				}
				return true;
			}
			
			settings = _data[ SETTINGS_KEY ] as PBXDictionary;
			settings.internalNewlines = false;
			if( !settings.ContainsKey( ATTRIBUTES_KEY ) ) {
				if( weak ) {
					attributes = new PBXList();
					attributes.internalNewlines = false;
					attributes.Add( WEAK_VALUE );
					settings.Add( ATTRIBUTES_KEY, attributes );
					return true;
				}
				else {
					return false;
				}
			}
			else {
				attributes = settings[ ATTRIBUTES_KEY ] as PBXList;
			}
			
			attributes.internalNewlines = false;
			if( weak ) {
				attributes.Add( WEAK_VALUE );
			}
			else {
				attributes.Remove( WEAK_VALUE );
			}
			
			settings.Add( ATTRIBUTES_KEY, attributes );
			this.Add( SETTINGS_KEY, settings );
			
			return true;
		}
		
		public bool AddCompilerFlag( string flag )
		{
			if( !_data.ContainsKey( SETTINGS_KEY ) )
				_data[ SETTINGS_KEY ] = new PBXDictionary();
			
			if( !((PBXDictionary)_data[ SETTINGS_KEY ]).ContainsKey( COMPILER_FLAGS_KEY ) ) {
				((PBXDictionary)_data[ SETTINGS_KEY ]).Add( COMPILER_FLAGS_KEY, flag );
				return true;
			}
			
			string[] flags = ((string)((PBXDictionary)_data[ SETTINGS_KEY ])[ COMPILER_FLAGS_KEY ]).Split( ' ' );
			foreach( string item in flags ) {
				if( item.CompareTo( flag ) == 0 )
					return false;
			}
			
			((PBXDictionary)_data[ SETTINGS_KEY ])[ COMPILER_FLAGS_KEY ] = ( string.Join( " ", flags ) + " " + flag );
			return true;
		}
		
	}
}
