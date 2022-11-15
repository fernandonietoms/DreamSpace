using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CortinaDeHumo : MonoBehaviour
{
    public GameObject diapositiva;
    public GameObject diapositivaContenido;

    public GameObject button;

    public Material[] contenido;

    int index = 0;
    // Start is called before the first frame update
    void Start()
    {
        diapositiva.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (OVRInput.GetDown(OVRInput.Button.Three))
        {
            diapositiva.SetActive(!diapositiva.activeSelf);
        }

        if (OVRInput.GetDown(OVRInput.Button.Four))
        {
            diapositivaContenido.GetComponent<MeshRenderer>().material = contenido[index];
            index++;
        }

        if (index == 1 || index == 2)
        {
            button.SetActive(true);
        }
        else
        {
            button.SetActive(false);
        }
    }
}
