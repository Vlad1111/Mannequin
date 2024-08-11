using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterailScript : MonoBehaviour
{
    public static Material BaseMaterial;
    public static Material SelectedMaterial;
    public static Material SpriteMaterial;
    public static Mesh QuadMesh;

    public Material baseMaterial;
    public Material selectedMaterial;
    public Material spriteMaterial;

    public Mesh quadMesh;

    // Start is called before the first frame update
    void Start()
    {
        BaseMaterial = baseMaterial;
        SelectedMaterial = selectedMaterial;
        SpriteMaterial = spriteMaterial;
        QuadMesh = quadMesh;
    }
}
