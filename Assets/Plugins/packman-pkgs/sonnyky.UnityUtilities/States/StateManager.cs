using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Base class for State Manager
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class StateManager<T>
{
    protected StateManager<T> thisInstance;
    public abstract void Tick();
    public virtual void OnStateEnter() { }
    public virtual void OnStateExit() { }

    private T myObject;

    public StateManager(T MyObject)
    {
        myObject = MyObject;
    }
}
