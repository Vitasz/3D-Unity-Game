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
    void Start()
    {
        //Calls the TaskOnClick/TaskWithParameters/ButtonClicked method when you click the Button
        ClearDecs.onClick.AddListener(ClearDecorations);
        AddBuild.onClick.AddListener(AddBuilding);
        DisableButton.onClick.AddListener(DisableClicked);
    }
    private void ClearDecorations()
    {
        Sphere.ClickedTile?._generateMesh.ClearDecorations();
    }
    private void AddBuilding()
    {

    }
    private void DisableClicked()
    {
        Sphere.DisableClicked();
    }
}
