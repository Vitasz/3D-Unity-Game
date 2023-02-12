using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEditor.Progress;

public struct ObjectOnLine
{
    public Item item;
    public GameObject GO { get; set; }
    public int NextPosition { get; set; }
}
public struct PositionOnConveor
{
    public Vector3 pos;
    public bool empty;
}
public class Conveyor : MonoBehaviour
{
    public IO input;
    public IO output;
    public LineRenderer line;
    public float speed = 10f;
    private readonly List<ObjectOnLine> items = new();
    private readonly List<PositionOnConveor> positions = new();
    private readonly float distanceBetween = 0.15f;
    public void Start()
    {
        Debug.Log("START");
        AddPosition(line.GetPosition(0));
        for (int i = 1; i < line.positionCount; i++)
        {
            Vector3 distance = line.GetPosition(i) - line.GetPosition(i - 1);
            Vector3 direction = distance.normalized;
            float length = distance.magnitude;
            int count = (int)(length / distanceBetween);
            float delta = (length - distanceBetween * count) / count;
            for (int j = 1; j <= count; j++)
            {
                AddPosition(line.GetPosition(i-1) + direction*(distanceBetween*j + delta * j));
            }
            
        }
        StartCoroutine(Move());
    }
    private void AddPosition(Vector3 position)
    {
        positions.Add(new PositionOnConveor
        {
            pos = position,
            empty = true
        }); 
    }
    IEnumerator Move()
    {
        while (true)
        {
            List<(GameObject go, Vector3 from, Vector3 to)> toMove = new();
            for (int i = items.Count - 1; i >= 0; i--)
            {
                ObjectOnLine item = items[i];
                GameObject go = items[i].GO;
                int next = items[i].NextPosition;
                if (next == positions.Count)
                {
                    Output(i);
                    continue;
                }
                if (!positions[next].empty) continue;
                toMove.Add((go, positions[next - 1].pos, positions[next].pos));
                SetAtPosition(next, false);
                SetAtPosition(next - 1, true);
                item.NextPosition++;
                items[i] = item;
            }
            yield return StartCoroutine(SmoothMove(toMove));
            if (positions[0].empty)
                input.CanOutput = true;
        }
    }
    IEnumerator SmoothMove(List<(GameObject go, Vector3 from, Vector3 to)> objects)
    {
        if (objects.Count == 0) yield return new WaitForEndOfFrame();
        float alpha = 0f;
        for (int i = 0; i < objects.Count; i++)
        {
            objects[i].go.transform.LookAt(objects[i].to);
        }
        while (alpha < 1f) {
            alpha += speed * Time.deltaTime;
            for (int j = 0; j < objects.Count; j++)
            {
                (GameObject go, Vector3 from, Vector3 to) = objects[j];
                go.transform.position = Vector3.Lerp(from, to, alpha);
            }
            yield return new WaitForEndOfFrame();
        }
        yield return null;
    }
    private void SetAtPosition(int index, bool isEmpty)
    {
        positions[index] = new PositionOnConveor
        {
            pos = positions[index].pos,
            empty = isEmpty
        };
    }
    public void Get(Item now)
    {
        if (now == null) return;

        GameObject gameObject = Instantiate(now.Prefab, transform);
        gameObject.transform.parent = transform;
        gameObject.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
        gameObject.transform.localPosition = input.transform.position;
        gameObject.transform.LookAt(positions[1].pos);
        items.Add(new ObjectOnLine
        {
            item = now,
            GO = gameObject,
            NextPosition = 1
        });
        input.CanOutput = false;
        SetAtPosition(0, false);
    }
    private bool Output(int index)
    {
        bool ok = output.Input(items[index].item);
        if (!ok) return false;
        Destroy(items[index].GO);
        positions[items[index].NextPosition - 1] = new PositionOnConveor
        {
            pos = positions[items[index].NextPosition - 1].pos,
            empty = true
        };
        items.RemoveAt(index);
        return true;
    }
}
