using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class Resourse : Entity
{
    public int MaxDurability;
    private int currDurabillity;
    public float _radius = 0f;
    public int MaxPerTile = 0;
    public float Radius
    {
        get { return _radius; }
        set
        {
            _radius = value / transform.localScale.x / 3.2f;   // устанавливаем новое значение свойства
        }
    }

    [SerializeField]
    public Item drop;
    public Resourse()
    {
        currDurabillity = MaxDurability;
    }
    public TypeOfItem GetResource()
    {
        currDurabillity--;
        if (currDurabillity == 0)
        {
            Destruct();
        }
        return drop.typeOfItem;
    }
    public void GenerateTexture()
    {
        int cnt = (int)(UnityEngine.Random.value * MaxPerTile * _radius / 10) + 10;
        for (int i = 0; i < cnt; i++)
        {
            GameObject now = Instantiate(gameObjects[(int)(UnityEngine.Random.value * 10) % gameObjects.Count], transform);
            now.transform.localPosition = new Vector3((UnityEngine.Random.value - 0.5f) * 2 * _radius, 0,
                (UnityEngine.Random.value - 0.5f) * 2 * _radius);
            now.transform.localRotation = Quaternion.Euler(0f, UnityEngine.Random.value * 180f, 0f);
        }
    }

    public void Destruct()
    {
        Destroy(gameObject.transform);
    }
}
