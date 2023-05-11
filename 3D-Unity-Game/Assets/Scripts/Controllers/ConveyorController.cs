using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ConveyorController : MonoBehaviour
{
    public IO prevClick;
    public GameObject ConveyorPrefab;
    public Hexasphere Hexasphere;

    public void Awake()
    {
        EventAggregator.ClickOnObject.Subscribe(OnClickOnIO);
    }
    private void OnClickOnIO(object obj)
    {
        if (obj is not IO)
        {
            prevClick = null;
            return;
        }
        IO now = (IO)obj;
        if (prevClick != null && now != prevClick)
        {
            CreateConveyor(prevClick, now);
            prevClick = null;
            return;
        }
        prevClick = now;
        return;
    }
    private void CreateConveyor(IO from, IO to)
    {
        if (from.Conveyor != null || to.Conveyor != null) return;
        if (from.controller == to.controller) return;
        GameObject newConveyor = Instantiate(ConveyorPrefab);
        Conveyor conveyor = newConveyor.GetComponent<Conveyor>();
        conveyor.input = from;
        conveyor.output = to;
        from.Conveyor = to.Conveyor = conveyor;
        Vector3[] positions = GetPositions(from, to);
        conveyor.line.positionCount = positions.Length;
        conveyor.line.SetPositions(positions);

    }
    private Vector3[] GetPositions(IO from, IO to)
    {
        var way = Hexasphere.FindWay(from.Building.tile, to.Building.tile);
        List<Vector3> positions = new (){ from.transform.position };
        foreach (var x in way) positions.Add(x.GenerateMesh.Center);
        positions.Add(to.transform.position);
        return positions.ToArray();
    }
}
