using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace UnityEditor.CountlyXCodeEditor
{
	public class XCConfigurationList : PBXObject
	{	
//		XCBuildConfigurationList buildConfigurations;
//		bool defaultConfigurationIsVisible = false;
//		string defaultConfigurationName;
		
		public XCConfigurationList( string guid, PBXDictionary dictionary ) : base( guid, dictionary ) {	
		}
	}
}
