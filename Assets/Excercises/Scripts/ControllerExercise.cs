using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControllerExercise : MonoBehaviour
{
    public Material MyMaterial;
    public GameObject Prefab;

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < 10; i++)
        {
            GameObject instancePrefab = Instantiate(Prefab);
            instancePrefab.transform.position = new Vector3(2 * i, 0, 0);
            instancePrefab.transform.parent = this.transform;
            instancePrefab.transform.localScale = new Vector3(1, 1, 1);
            instancePrefab.AddComponent<Rotate3DObject>();

            if (IsVisibleFrom(instancePrefab.transform.GetComponent<Renderer>().bounds, Camera.main) == true)
            {
                Debug.Log("INSTANCE[" + i + "] IS VISIBLE");
            }
            else
            {
                Debug.Log("INSTANCE[" + i + "] IS NOT VISIBLE");
            }
        }

        ApplyMaterialOnObjects(this.gameObject.transform.root.gameObject, MyMaterial);
    }

    private bool IsVisibleFrom(Bounds _bounds, Camera _camera)
    {
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(_camera);
        return GeometryUtility.TestPlanesAABB(planes, _bounds);
    }

    private void ApplyMaterialOnObjects(GameObject _go, Material _material)
    {
        foreach (Transform child in _go.transform)
        {
            ApplyMaterialOnObjects(child.gameObject, _material);
        }
        if (_go.GetComponent<Renderer>() != null)
        {
            _go.GetComponent<Renderer>().material = _material;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
