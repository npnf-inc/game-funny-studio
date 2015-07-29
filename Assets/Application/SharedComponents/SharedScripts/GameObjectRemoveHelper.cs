using UnityEngine;
using System.Collections;

public class GameObjectRemoveHelper : MonoBehaviour {

	public static bool FindAndRemoveGameObject (GameObject createdObject, string objectName)
	{
		GameObject[] objs = GameObject.FindObjectsOfType<GameObject>();
		int counter = 0;
		foreach(GameObject obj in objs)
		{
			if (obj.name.Equals(objectName))
			{
				counter++;
			}
		}
		if (counter > 1)
		{
			Destroy(createdObject);
			return true;
		}

		return false;
	}
}
