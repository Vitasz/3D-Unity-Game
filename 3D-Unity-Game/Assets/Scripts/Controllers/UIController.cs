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
        Sphere.ClickedTile.Height -= 1000;
        Sphere.ClickOnTile(Sphere.ClickedTile);
    }
    private void AddBuilding()
    {
        
        if (Sphere.ClickedTile == null) return;
        Sphere.ClickedTile._generateMesh.ClearDecorations();
        GameObject go = Instantiate(Building, Sphere.ClickedTile.chunk.transform.GetChild(0));
        Sphere.ClickedTile.AddBuilding(go);
        return;
    }

    private void DisableClicked()
    {
        Sphere.DisableClicked();
    }
}
