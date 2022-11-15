using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public Transform RightController;

    public GameObject Rayo;
    
    public GameObject buttonAudioSource;

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

        if (Reicast)
        {
            if (Hit.collider.name == "Iniciar" && OVRInput.GetDown(OVRInput.Button.SecondaryIndexTrigger))
            {
                //Efectos de sonido
                GameObject sound = Instantiate(buttonAudioSource, Hit.collider.transform);
                Destroy(sound, sound.GetComponent<AudioSource>().clip.length);

                SceneManager.LoadScene("SceneSelect", LoadSceneMode.Single);
            }
            if (Hit.collider.name == "Tutorial" && OVRInput.GetDown(OVRInput.Button.SecondaryIndexTrigger))
            {
                //Efectos de sonido
                GameObject sound = Instantiate(buttonAudioSource, Hit.collider.transform);
                Destroy(sound, sound.GetComponent<AudioSource>().clip.length);

                SceneManager.LoadScene("Tutorial", LoadSceneMode.Single);
            }
        }
    }
}
