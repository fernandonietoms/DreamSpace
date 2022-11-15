using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSelect : MonoBehaviour
{
    public Transform RightController;

    public GameObject Rayo;

    [Header("Fuentes de audio")]
    public GameObject selectAudioSource;

    // Start is called before the first frame update
    void Start()
    {
        Rayo.GetComponent<LineRenderer>().material.color = Color.blue;
    }

    // Update is called once per frame
    void Update()
    {
        RaycastHit Hit;
        // Booleano que representa que el Raycast tocó algún objeto en un rango predeterminado
        bool Reicast = Physics.Raycast(RightController.position, RightController.TransformDirection(Vector3.forward), out Hit, 10);

        if (Reicast && OVRInput.GetDown(OVRInput.Button.SecondaryIndexTrigger))
        {
            if (Hit.collider.name == "Casa1")
            {

                //Efectos de sonido
                GameObject sound = Instantiate(selectAudioSource, Hit.collider.transform);
                Destroy(sound, sound.GetComponent<AudioSource>().clip.length);

                //SceneManager.LoadScene("BarrancaHonda", LoadSceneMode.Single);
                StartCoroutine(goToSelectedHouse(Hit.collider.name));
            }
            if (Hit.collider.name == "Casa2" && OVRInput.GetDown(OVRInput.Button.SecondaryIndexTrigger))
            {
                //Efectos de sonido
                GameObject sound = Instantiate(selectAudioSource, Hit.collider.transform);
                Destroy(sound, sound.GetComponent<AudioSource>().clip.length);

                //SceneManager.LoadScene("Casa2", LoadSceneMode.Single);
                StartCoroutine(goToSelectedHouse(Hit.collider.name));
            }
        }
    }
    // METODO QUE ACCEDE A TODAS LAS ESCENAS DE CASAS 
    // SOLO CON EL NOMBRE DE LA CASA COMO PARAMETRO 
    // (ESCENAS DEBEN TENER MISMO NOMBRE QUE MODELO DE LA CASA)
    IEnumerator goToSelectedHouse(string houseName)
    {
        yield return new WaitForSeconds(2);
        SceneManager.LoadScene(houseName, LoadSceneMode.Single);
    }

}
