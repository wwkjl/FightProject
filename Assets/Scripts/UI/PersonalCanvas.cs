using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PersonalCanvas : MonoBehaviour
{
    public Transform canvasTransform;

    void Update()
    {
        canvasTransform.LookAt(canvasTransform.position + Camera.main.transform.forward);
    }
}
