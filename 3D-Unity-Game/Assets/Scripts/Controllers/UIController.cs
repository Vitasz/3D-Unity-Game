using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    public Hexasphere Sphere;
    public Button ClearDecs;
    public Button AddBuild;
    public Button DisableButton;
    public Button DowbButton;
    public Button UpButton;
    public Button SaveButton;
    public Button LoadButton;

    public GameObject Building;

    void Start()
    {
        ClearDecs.onClick.AddListener(ClearDecorations);
        AddBuild.onClick.AddListener(AddBuilding);
        DisableButton.onClick.AddListener(DisableClicked);
        UpButton.onClick.AddListener(Up);
        DowbButton.onClick.AddListener(Down);
        SaveButton.onClick.AddListener(Save);
        LoadButton.onClick.AddListener(Load);
    }
    private void ClearDecorations()
    {
        
    }
    private void Up()
    {
        if (Sphere.ClickedTile == null) return;
        Sphere.ClickedTile.Height += 1;
        Sphere.ClickOnTile(Sphere.ClickedTile);
    }
    private void Down()
    {
        if (Sphere.ClickedTile == null) return;
        Sphere.ClickedTile.Height -= 1;
        Sphere.ClickOnTile(Sphere.ClickedTile);
    }
    private void AddBuilding()
    {
        if (Sphere.ClickedTile.Neighbours.Count != 6) return;
        if (Sphere.ClickedTile == null) return;
        GameObject go = Instantiate(Building);
        go.GetComponentInChildren<Building>().tile = Sphere.ClickedTile;
        Sphere.ClickedTile.AddBuilding(go);
        return;
    }

    private void DisableClicked()
    {
        Sphere.DisableClicked();
    }
    private void Save()
    {
        Sphere.Save();
    }
    private void Load()
    {
        Sphere.Load("Saves/Save1.json");
    }
}
