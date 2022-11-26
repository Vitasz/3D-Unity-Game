using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Building : Entity
{
    protected Team _team;
    protected Tile _tile;
    protected List<Conveyor> _enters;
    protected List<Conveyor> _exits;

    public Building(Tile tile, Team team)
    {
        _tile = tile;
        _team = team;
    }

    public virtual void Output() {}

    public virtual void Input() {}
}
