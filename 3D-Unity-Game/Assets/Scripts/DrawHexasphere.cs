
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class DrawHexasphere : MonoBehaviour
{
    [Min(5f)]
    [SerializeField] private float radius = 10f;
    [Range(1, 50)]
    [SerializeField] private int divisions = 4;
    [Range(0.1f, 1f)]
    [SerializeField] private float hexSize = 1f;

    private Mesh _mesh;
    private MeshFilter _meshFilter;
    private Hexasphere _hexasphere;
    private MeshCollider _meshCollider;

    public float rotationSpeed = 2f;
    private void Start()
    {
        _meshFilter = GetComponent<MeshFilter>();
        _meshCollider = GetComponent<MeshCollider>();
        DrawHexasphereMesh();
    }


    private void DrawHexasphereMesh()
    {
        _hexasphere = new Hexasphere(radius, divisions, hexSize);

        _mesh = new Mesh();

        _meshFilter.mesh = _mesh;
        _meshCollider.sharedMesh = _mesh;
        

        _mesh.vertices = _hexasphere.MeshDetails.Vertices.ToArray();
        _mesh.triangles = _hexasphere.MeshDetails.Triangles.ToArray();
        _mesh.colors = _hexasphere.MeshDetails.Colors.ToArray();
        _mesh.RecalculateNormals();

        _meshCollider.convex = true;
    }
    private void _OnMouseDown()
    {
        Ray inputRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(inputRay, out hit))
        {
            Vector3 position = transform.InverseTransformPoint(hit.point);
            Tile tile = _hexasphere.GetTile(position);
            tile.SetColor(new Color(Random.value, Random.value, Random.value));
            _hexasphere.UpdateMeshDetails();
            _mesh.colors = _hexasphere.MeshDetails.Colors.ToArray();
        }
    }
    private void OnMouseDrag()
    {
        float XaxisRotation = Input.GetAxis("Mouse X") * rotationSpeed;
        float YaxisRotation = Input.GetAxis("Mouse Y") * rotationSpeed;
        // select the axis by which you want to rotate the GameObject
        transform.Rotate(new Vector3(YaxisRotation, -XaxisRotation,0));
    }
}
