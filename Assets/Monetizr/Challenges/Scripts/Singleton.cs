using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public abstract class Singleton<T> : MonoBehaviour where T : Component
{

	private static T instance;

    public static T GetInstance() {
			return instance;
	}

    public static T Initialize(string name)
    {
        if(instance)
            return instance;

        var go = new GameObject(name);
              
        instance = go.AddComponent<T>();

        //instance.Initialize();

        return instance;
    }


	/*public void Awake()
	{
		if (instance == null )
        {
			instance = this as T;
			//Debug.Log("Creating.." + gameObject.name);
		}
        else {
			if (instance != this) {
				Destroy (gameObject);
			}
		}
	}*/
}
