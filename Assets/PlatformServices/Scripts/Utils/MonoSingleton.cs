using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonoSingleton<T> : MonoBehaviour where T: MonoSingleton<T>
{
    private static T instance;
    public static T Instance
    {
        get
        {
            if (instance != null)
            {
                return instance;
            }
            
            if (instance == null)
            {
                instance = FindObjectOfType<T>();
            }

            return instance;
        }
    }
    protected virtual void Awake()
    {
        if (instance == null)
        {
            instance = this as T;
        }
        else if ( instance != this)
        {
            Debug.LogError($"Destroyed Duplicate Singleton: { this.gameObject} In Scene: {this.gameObject.scene}");
            Destroy(this.gameObject);
        }
    }


}
