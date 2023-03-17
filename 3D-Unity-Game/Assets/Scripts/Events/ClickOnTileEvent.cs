using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClickOnTileEvent
{
    private readonly List<Action<Tile>> _callbacks = new();

    public void Subscribe(Action<Tile> callback)
    {
        _callbacks.Add(callback);
    }

    public void Publish(Tile io)
    {
        foreach (var callback in _callbacks)
        {
            callback(io);
        }
    }
}
