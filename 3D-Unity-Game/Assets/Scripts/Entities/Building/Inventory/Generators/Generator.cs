using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Generator : BuildingWithStackInventory
{
    [SerializeField]
    public uint MaxProduction;
    [SerializeField]
    public uint MaxSaveEnegry;

    protected uint _currProduction = 0;
    protected uint _currSaveEnegry = 0;

    // TODO: delete and make method
    public uint CurrSaveEnegry { get => _currSaveEnegry; }

    public Generator(Tile tile, Team team) : base(tile, team) {  }

    public void Awake()
    {
        _currProduction = MaxProduction;
    }

    public void GetProdaction()
    {
        _currSaveEnegry = Math.Min(MaxSaveEnegry, _currSaveEnegry + _currProduction);
    }
}
