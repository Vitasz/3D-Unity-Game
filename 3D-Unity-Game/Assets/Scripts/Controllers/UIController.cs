using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    public Hexasphere sphere;
    public Button clearDecs;
    public Button addBuild;
    public Button disableButton;
    public Button downButton;
    public Button upButton;
    public Button saveButton;
    public Button loadButton;

    public GameObject building;

    void Start()
    {
        clearDecs.onClick.AddListener(ClearDecorations);
        addBuild.onClick.AddListener(AddBuilding);
        disableButton.onClick.AddListener(DisableClicked);
        upButton.onClick.AddListener(Up);
        downButton.onClick.AddListener(Down);
        // saveButton.onClick.AddListener(Save);
        // loadButton.onClick.AddListener(Load);
    }
    private void ClearDecorations()
    {
        sphere.ClickedTile.RemoveObjects();
    }
    
    private void Up()
    {
        if (sphere.ClickedTile == null) return;
        
        sphere.ClickedTile.Height += 1;
        sphere.ClickOnTile(sphere.ClickedTile);
    }
    
    private void Down()
    {
        if (sphere.ClickedTile == null) return;
        
        sphere.ClickedTile.Height -= 1;
        sphere.ClickOnTile(sphere.ClickedTile);
    }
    
    private void AddBuilding()
    {
        var tile = sphere.ClickedTile;
        
        if (tile == null) return;
        if (tile.Neighbours.Count != 6) return;

        var go = Instantiate(building);
        go.GetComponentInChildren<Building>().tile = tile;
        tile.AddBuilding(go);
        return;
    }

    private void DisableClicked() => sphere.DisableClicked();
    
    private void Save()
    {
        // Sphere.Save();
    }
    private void Load()
    {
        // Sphere.Load("Saves/Save1.json");
    }
}
