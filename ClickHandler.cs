using LGen.LSimulate;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClickHandler : MonoBehaviour
{
    Camera camera;

    // Start is called before the first frame update
    void Start()
    {
        camera = GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        // code from https://answers.unity.com/questions/1605687/click-on-mesh.html
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = camera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 100))
            {
                GameObject hitObject = hit.collider.gameObject;
                
                Simulation.Instance.PrintAgentForGameObject(hitObject.transform.parent.gameObject);
            }
        }
    }
}
