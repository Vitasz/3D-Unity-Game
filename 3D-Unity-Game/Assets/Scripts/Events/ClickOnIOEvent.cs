using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClickOnObjectEvent
{
    private readonly List<Action<object>> _callbacks = new();

    public void Subscribe(Action<object> callback)
    {
        _callbacks.Add(callback);
    }
    public void Publish(object io)
    {
        foreach(var callback in _callbacks)
        {
            callback(io);
        }
    }
}
