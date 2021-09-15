using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameObjectPool : ObjectPool<GameObject>
{
    public GameObjectPool(Transform deactivationParent)
    {
        base.deactivate = (GameObject g) => 
        { 
            g.transform.position = Vector3.zero;
            g.SetActive(false); 
            g.transform.parent = deactivationParent; 
        };
        base.activate = (GameObject g) => { g.SetActive(true); };
    }
}

public class ObjectPool<T>
{
    public Stack<T> availableObjects = new Stack<T>();

    public delegate T CreateFunction();
    public CreateFunction create;

    public delegate void DeactivateFunction(T obj);
    public DeactivateFunction deactivate;
    public DeactivateFunction activate;

    public T GetOrCreate()
    {
        if (availableObjects.Count <= 0) return create();

        T obj = availableObjects.Pop();
        activate(obj);

        return obj;
    }

    public void PreGenerateObjects(int count)
    {
        for(int i = 0; i < count; i++) availableObjects.Push(create());
    }

    public void Return(T obj)
    {
        availableObjects.Push(obj);
        deactivate(obj);
    }
}