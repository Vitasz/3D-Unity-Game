using System.Collections;
using System.Collections.Generic;
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

    public GameObject Building;
    public GameController Controller;

    void Start()
    {
        ClearDecs.onClick.AddListener(ClearDecorations);
        AddBuild.onClick.AddListener(AddBuilding);
        DisableButton.onClick.AddListener(DisableClicked);
        UpButton.onClick.AddListener(Up);
        DowbButton.onClick.AddListener(Down);
    }
    private void ClearDecorations()
    {
        Sphere.ClickedTile?._generateMesh.ClearDecorations();
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
        
        if (Sphere.ClickedTile == null) return;
        
        foreach (Tile tile in Sphere.Tiles)
        {
            if (tile._type == Type_of_Tiles.Water) continue;
            GameObject go = Instantiate(Building);
            Controller.buildings.Add(go.GetComponent<Building>());
            tile._generateMesh.ClearDecorations();
            tile.AddBuilding(go);
            //Sphere.ClickedTile._generateMesh.ClearDecorations();
            //Sphere.ClickedTile.AddBuilding(go);
        }

    }

    private void DisableClicked()
    {
        Sphere.DisableClicked();
    }
}
