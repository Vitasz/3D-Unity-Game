using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public List<Building> buildings;

    private const uint MaxTimer = 60;
    private uint _timer = 0;

    public void FixedUpdate()
    {
        _timer++;

        /*if (_timer == MaxTimer)
        {
            foreach (var building in buildings)
            {
                if (building is Generator gen)
                {
                    gen.GetProdaction();
                    //Debug.Log(gen.CurrSaveEnegry);
                }
            }

            _timer = 0;
        }*/
    }
}
