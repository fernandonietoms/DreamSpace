using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DreamSpaceControl : MonoBehaviour
{
    // GameObject que almacena la cámara del espectador
    public GameObject POVCam;

    // GameObject que representa al rayo
    public GameObject rightRay;

    //Informacion del rayo
    RaycastHit rightHit;

    //bool del rayo golpeando
    bool rightRaycast;

    // Objeto que almacena la posición del control derecho
    public Transform RightController;

    // Directional Light (Iluminación en la escena)
    public GameObject mDirLight;

    #region Variables del panel de UI
    // Gameobject que hace referencia al panel del menu
    public GameObject Panel;

    // Propiedades de animación del panel
    public Animator PanelAnim;
    // Booleano que indica al sistema si el panel de menú está activo
    public static bool IsPanelActive;

    // Objetos que administran el cambio a sección previa / próxima
    public GameObject PrevButton, NextButton;

    // Objeto que resalta al elemento activo de la UI
    public GameObject Iterator;

    // Objeto que resalta la categoría activa
    public GameObject CatIterator;

    // Objeto que mostrará el nombre de la categoría seleccionada
    public GameObject ElementHeader;

    // Lista de materiales que indicará el nombre de la categoría seleccionada
    public Material[] ElemHeaderMats;

    // Vector de posición que almacena la posición sobre la que se colocará el iterador
    private Vector3 SelectedUIElem;

    // Indicador del valor activo del iterador
    private int ActiveIterator;
    // Listas que almacenarán objetos y materiales activos
    private GameObject[] ActiveGameObjList = new GameObject[100];
    private Material[] ActiveMatList = new Material[100];

    #region Listas de Objetos y Materiales
    // Arreglos que contienen los elementos de cada categoría
    public GameObject[] GO_Cat1, GO_Cat2, GO_Cat3, GO_Cat4, GO_Cat5, GO_Cat6, GO_Cat7, GO_Cat8;
    public Material[] Mat_Cat1, Mat_Cat2, Mat_Cat3, Mat_Cat4, Mat_Cat5, Mat_Cat6, Mat_Cat7, Mat_Cat8;

    //// Arreglo que contiene los valores temporales de las listas seleccionadas
    private GameObject[] TempGO;
    //public Material[] TempMat;
    #endregion
    #endregion

    //private GameObject ActiveObject;

    // Lista de objetos que van a ser alterados (como los corridos fierro alv bien alterados)
    // Contiene a los planos que se colocarán en la UI y que cambiarán de material para mostrar los productos
    public GameObject[] UIElements;

    // Lista de objetos y materiales que se podrán instanciar (lista de objetos y materiales activos dentro de la UI)
    public GameObject[] GameItems;
    public Material[] GameMaterials;

    public Material[] UIMaterials;
    private bool ParedColor;


    #region Variables para controlar el Raycast fuera del menú
    // Booleano que indica si se instanciará o se borrará
    private bool Deleter;

    // Objeto que se instanciará
    public GameObject ActiveObject;
    // Objeto que el usuario está sujetando
    private GameObject HeldObject;

    // Verifica si el usuario está sujetando un objetos
    private bool HasObjectInHand;

    // Booleano que indica si hay que usar las capas para colocar elementos o no usarlas para cambiar el material de los objetos
    private bool UseLayer;

    // Valor que indica la máscara activa
    private int ActiveLayer;

    // Valores de layers válidas (Pisos, Paredes, Techos, en ese orden)
    private int layerMask1 = 1 << 8;
    private int layerMask2 = 1 << 9;
    private int layerMask3 = 1 << 10;
    //          Máscara en la que existirán los objetos instanciados
    private int layerMask4 = 1 << 11;
    //          Máscara en la que estarán los colliders desagrupados
    private int layerMask5 = 1 << 12;

    // Material activo
    private Material ActiveMaterial;
    #endregion

    #region Control por Oculus y Teclado
    public static bool RightIndexTrigger, LeftIndexTrigger, RightHandTrigger, LeftHandTrigger;

    [Header("Fuentes de Audio")]
    public GameObject dayAmbientAudioSource;
    public GameObject nightAmbientAudioSource;
    public GameObject openMenuAudioSource;
    public GameObject closeMenuAudioSource;
    public GameObject button2AudioSource;
    public GameObject button3AudioSource;
    public GameObject button4AudioSource;
    public GameObject destroyObjAudioSource;
    public GameObject instansiateObjAudioSource;
    public GameObject putObjAudioSource;
    public GameObject paintObjAudioSource;
    public GameObject stepsAudioSource;

    //Booleanos para control de iluminación
    bool isDayLight;
    bool playedOnce;

    public Material materialDia;
    public Material materialNoche;

    // Función que controla el input para Oculus y Teclado
    private void MixedInput()
    {
        if (Input.GetKeyDown(KeyCode.Space) || OVRInput.GetDown(OVRInput.Button.SecondaryIndexTrigger))
        {
            RightIndexTrigger = true;
        }
        else
        {
            RightIndexTrigger = false;
        }

        if (Input.GetKeyDown(KeyCode.Tab) || OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger))
        {
            LeftIndexTrigger = true;
        }
        else
        {
            LeftIndexTrigger = false;
        }

        if (Input.GetKeyDown(KeyCode.E) || OVRInput.GetDown(OVRInput.Button.SecondaryHandTrigger))
        {
            RightHandTrigger = true;
        }
        else
        {
            RightHandTrigger = false;
        }

        if (Input.GetKeyDown(KeyCode.Q) || OVRInput.GetDown(OVRInput.Button.PrimaryHandTrigger))
        {
            LeftHandTrigger = true;
        }
        else
        {
            LeftHandTrigger = false;
        }
    }
    #endregion

    //Eliminar o modificar estas funciones en cada codigo de cada escena, activan una camara que no se usa para VR
    IEnumerator ActivatePOV ()
    {
        POVCam.SetActive(false);
        yield return new WaitForSeconds(0.2f);
        POVCam.SetActive(true);
        yield break;
    }

    // Start is called before the first frame update
    void Start()
    {
        isDayLight = true; 

        StartCoroutine(ActivatePOV());

        IsPanelActive = false;

        Iterator.SetActive(false);
        ActiveIterator = 0;
        //FillActiveLists(GO_Cat5, Mat_Cat5);
        //SetUIActiveLists();
        CatIterator.SetActive(false);

        ParedColor = false;

        #region Inicialización de valores fuera del UI
        ActiveObject = GameItems[0];
        UseLayer = true;
        ActiveLayer = layerMask1;

        rightRay.GetComponent<LineRenderer>().material.color = Color.blue;
        Deleter = false;
        #endregion
    }

    // Update is called once per frame
    void Update()
    {
        // Booleano que representa que el Raycast tocó algún objeto en un rango predeterminado
        rightRaycast = Physics.Raycast(RightController.position, RightController.TransformDirection(Vector3.forward), out rightHit, 10);

        DrawRay(rightRay, RightController, rightHit, rightRaycast);

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            FillActiveLists(GO_Cat4, Mat_Cat4);
            UseLayer = false;
            ActiveLayer = layerMask5;
            ElementHeader.GetComponent<Renderer>().material = ElemHeaderMats[3];
            ParedColor = true;
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SetUIActiveLists();
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            ActiveIterator--;
            SetUIActiveLists();
            ActivatePrevNext(TempGO);

            Iterator.SetActive(false);
        }

        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            ActiveIterator++;
            SetUIActiveLists();
            ActivatePrevNext(TempGO);

            Iterator.SetActive(false);
        }

        // Se encarga de detectar la entrada de usuario, ya sea por teclado o por Oculus Touch
        MixedInput();

        // Si se presiona el gatillo derecho,
        // se ejecuta el código de Raycast
        if (IsPanelActive && RightIndexTrigger)
        {
            MenuRaycast();
        }

        // Si se presiona el gatillo izquierdo,
        // Se activa la función que muestra u oculta el panel del menú
        if (LeftIndexTrigger)
        {
            ShowPanel();

            // Resetear el Rayo fuera de la UI
            RayReset();
            // Resetear el modo de Borrado
            Deleter = false;
            rightRay.GetComponent<LineRenderer>().material.color = Color.blue;
        }

        #region Acciones en Update fuera de la UI
        if (!IsPanelActive)
        {
            if (!Deleter)
            {
                GeneralRaycast();
            } else
            {
                DeleteRaycast();
            }
            
        }

        // Rotar el objeto que sujeta el usuario
        if (HeldObject)
        {
            ObjRotate();
        }

        if (LeftHandTrigger && !isDayLight)
        {
            Daylight();
        }

        if(LeftHandTrigger && isDayLight)
        {
            NightLight();
        }

        //Efectos de sonido de caminar
        playStepsSound();

       
        #endregion
    }

    #region UI Panel Controller
    // Función que administra el comportamiento del Raycast cuando el menú está activo
    private void MenuRaycast()
    {
        //SE MUEVEN ESTAS LINEAS AL UPDATE
        //// Booleano que representa que el Raycast tocó algún objeto en un rango predeterminado
        //bool rightRaycast = Physics.Raycast(RightController.position, RightController.TransformDirection(Vector3.forward), out rightHit, 2);

        //DrawRay(rightRay, RightController, rightHit, rightRaycast);
        
        if (rightRaycast)
        {
            if (rightHit.collider.name.Contains("OBJ"))
            {
                //Efectos de sonido
                GameObject sound = Instantiate(button4AudioSource, PanelAnim.gameObject.transform);
                Destroy(sound, sound.GetComponent<AudioSource>().clip.length);

                //Panel.GetComponent<Renderer>().material = Hit.collider.GetComponent<Renderer>().material;
                DoAction(rightHit.collider.name);
                // Activa el Iterador visual
                if (!Iterator.activeSelf)
                {
                    Iterator.SetActive(true);
                }
                // Crea y aigna un vector de posición para el iterador visual, que coincide con el elemento seleccionado

                Iterator.transform.SetParent(rightHit.collider.transform);
                Iterator.transform.localPosition = new Vector3(0, 0.01f, 0);
                
            }

            if (rightHit.collider.name.Contains("OPC"))
            {
                //Efectos de sonido
                GameObject sound = Instantiate(button3AudioSource, PanelAnim.gameObject.transform);
                Destroy(sound, sound.GetComponent<AudioSource>().clip.length);

                DoOption(rightHit.collider.name);
            }
            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            // Si el usuario selecciona un elemento que se refiere a una nueva categoría
            // - Reinicia el iterador activo (porque el menú se muestra desde la primera página de contenido)
            // - Se llama a la función que modifica las listas activas de acuerdo con la categoría seleccionada
            // - Finalmente se llama a la función que modifica la UI de acuerdo con las listas que se modificaron
            if (rightHit.collider.name.Contains("CAT"))
            {
                //Efectos de sonido
                GameObject sound = Instantiate(button2AudioSource, PanelAnim.gameObject.transform);
                Destroy(sound, sound.GetComponent<AudioSource>().clip.length);

                if (!CatIterator.activeSelf)
                {
                    CatIterator.SetActive(true);
                }
                ActiveIterator = 0;

                // Se reposiciona el objeto que resalta la categoría seleccionada
                //CatIterator.transform.position = new Vector3(Hit.collider.transform.position.x,
                //                                            Hit.collider.transform.position.y + 0.0001f,
                //                                            Hit.collider.transform.position.z /*+ 0.0001f*/);

                CatIterator.transform.SetParent(rightHit.collider.transform);
                CatIterator.transform.localPosition = new Vector3(0, 0.01f, 0);


                // Desactiva el iterador visual cuando se cambia de página
                Iterator.SetActive(false);

                // Selecciones por categoría;
                // Armarios
                if (rightHit.collider.name == "CAT1")
                {
                    UseLayer = true;
                    ActiveLayer = layerMask1;
                    FillActiveLists(GO_Cat1, Mat_Cat1);
                    ElementHeader.GetComponent<Renderer>().material = ElemHeaderMats[0];
                    ParedColor = false;
                }
                // Mesas
                if (rightHit.collider.name == "CAT2")
                {
                    UseLayer = true;
                    ActiveLayer = layerMask1;
                    FillActiveLists(GO_Cat2, Mat_Cat2);
                    ElementHeader.GetComponent<Renderer>().material = ElemHeaderMats[1];
                    ParedColor = false;
                }
                // Camas
                if (rightHit.collider.name == "CAT3")
                {
                    UseLayer = true;
                    ActiveLayer = layerMask1;
                    FillActiveLists(GO_Cat3, Mat_Cat3);
                    ElementHeader.GetComponent<Renderer>().material = ElemHeaderMats[2];
                    ParedColor = false;
                }
                // Pared / Color
                if (rightHit.collider.name == "CAT4")
                {
                    FillActiveLists(GO_Cat4, Mat_Cat4);
                    UseLayer = false;
                    ActiveLayer = layerMask5;
                    ElementHeader.GetComponent<Renderer>().material = ElemHeaderMats[3];
                    ParedColor = true;
                }
                // Sillones
                if (rightHit.collider.name == "CAT5")
                {
                    FillActiveLists(GO_Cat5, Mat_Cat5);
                    UseLayer = true;
                    ActiveLayer = layerMask1;
                    ElementHeader.GetComponent<Renderer>().material = ElemHeaderMats[4];
                    ParedColor = false;
                }
                // Oficina
                if (rightHit.collider.name == "CAT6")
                {
                    UseLayer = true;
                    ActiveLayer = layerMask1;
                    FillActiveLists(GO_Cat6, Mat_Cat6);
                    ElementHeader.GetComponent<Renderer>().material = ElemHeaderMats[5];
                    ParedColor = false;
                }
                // Espejos
                if (rightHit.collider.name == "CAT7")
                {
                    UseLayer = true;
                    ActiveLayer = layerMask1;
                    FillActiveLists(GO_Cat7, Mat_Cat7);
                    UseLayer = true;
                    ActiveLayer = layerMask1;
                    ElementHeader.GetComponent<Renderer>().material = ElemHeaderMats[6];
                    ParedColor = false;
                }
                // Lamparas
                if (rightHit.collider.name == "CAT8")
                {
                    UseLayer = true;
                    ActiveLayer = layerMask3;
                    FillActiveLists(GO_Cat8, Mat_Cat8);
                    ElementHeader.GetComponent<Renderer>().material = ElemHeaderMats[7];
                    ParedColor = false;
                }

                SetUIActiveLists();
            }

            // Cambio de página
            // - Se incrementa o decrementa el iterador activo, y se llama a la función que actualiza las listas activas
            // - Se llama a la función que activa o desactiva los botones de siguiente o previo
            if (rightHit.collider.name == "Prev" && ActiveIterator > 0)
            {
                //Efectos de sonido
                GameObject sound = Instantiate(button2AudioSource, PanelAnim.gameObject.transform);
                Destroy(sound, sound.GetComponent<AudioSource>().clip.length);

                ActiveIterator--;
                SetUIActiveLists();
                ActivatePrevNext(TempGO);

                Iterator.SetActive(false);
            }
            if (rightHit.collider.name == "Next")
            {
                //Efectos de sonido
                GameObject sound = Instantiate(button2AudioSource, PanelAnim.gameObject.transform);
                Destroy(sound, sound.GetComponent<AudioSource>().clip.length);

                ActiveIterator++;
                SetUIActiveLists();
                ActivatePrevNext(TempGO);

                Iterator.SetActive(false);
            }

            // Si el usuario selecciona el botón de cerrar con el raycast
            if (rightHit.collider.name == "Close")
            {
                ShowPanel();
            }
        }
    }

    // Función que se encarga de mostrar u ocultar el panel de la UI cuando
    private void ShowPanel()
    {
        // Se activa la animación que muestra u oculta el panel dependiendo del estado actual del mismo
        // Además, se cambia el booleano que indica el estado del panel, de acuerdo al estado al que se va a cambiar
        switch (IsPanelActive)
        {
            case true:
                PanelAnim.Play("PanelHide");
                //Efectos de sonido
                GameObject sound = Instantiate(closeMenuAudioSource, PanelAnim.gameObject.transform);
                Destroy(sound, sound.GetComponent<AudioSource>().clip.length);
                IsPanelActive = false;
                break;
            case false:
                PanelAnim.Play("PanelShow");
                //Efectos de sonido
                GameObject sound2 = Instantiate(openMenuAudioSource, PanelAnim.gameObject.transform);
                Destroy(sound2, sound2.GetComponent<AudioSource>().clip.length);
                IsPanelActive = true;
                break;
        }
    }

    // Función que se encarga de llenar las listas activas según la categoría que selecciona el usuario
    // Toma como argumento a las listas de objetos y materiales que corresponden a una categoría específica
    private void FillActiveLists(GameObject[] OriginalGOList, Material[] OriginalMatList)
    {
        // Se rellenan las listas con valores vacíos para evitar errores
        for (int x = 0; x < 100; x++)
        {
            ActiveGameObjList[x] = null;
            ActiveMatList[x] = null;
        }

        // Asigna a cada valor de las listas, ahora vacías, el valor que le corresponde de las
        // listas que se tomaron como argumento de la función
        for (int x = 0; x < OriginalGOList.Length; x++)
        {
            ActiveGameObjList[x] = OriginalGOList[x];
            ActiveMatList[x] = OriginalMatList[x];
        }

        // Llama a la función que se encarga de activar o desactivar los botones de previo o sigiente
        ActivatePrevNext(OriginalGOList);
        // Asigna la lista que se tomó de argumento a una lista temporal, para conservar su valor de longitud
        TempGO = OriginalGOList;
    }

    // Función que se encarga de llenar las listas que contienen los objetos activos para el menú
    private void SetUIActiveLists()
    {
        // Se crea una variable independiente del ciclo para respetar la posición inicial de las listas
        int y = 0;
        // Se calcula el valor de posición inicial en la que se realiza la búsqueda dentro de las listas originales
        // Se obtiene multiplicando el número de elementos que caben en la UI por el iterador de páginas
        int StartingPoint = GameItems.Length * ActiveIterator;

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        // El ciclo se ejecuta tantas veces como elementos de la UI
        for (int n = (StartingPoint); n < (StartingPoint + GameItems.Length); n++)
        {
            // Si el elemento de la lista activa está vacío, se vacía el elemento correspondiente de la lista de objetos activo,
            // de otro modo, el objeto de la lista se asigna como objeto activo
            if (ActiveGameObjList[n] != null)
            {
                GameItems[y] = ActiveGameObjList[n];
            }
            else
            {
                GameItems[y] = null;
            }

            // El siguiente segmento debe de cambiarse por un método de instanciado de prefabs en las posiciones designadas para cada item

            // Si el elemento de la lista activa está vacío, simplemente se desactiva el elemento de la UI que le corresponde,
            // de otro modo, se activa el elemento de la UI, y su material es sustituido por el sprite que le corresponde
            if (ActiveMatList[n] != null)
            {

                Debug.Log("got in");
                UIElements[y].SetActive(true);

                GameMaterials[y] = ActiveMatList[n];

                UIElements[y].GetComponent<Renderer>().material = GameMaterials[y];

                if (ParedColor == true)
                {
                    //UIElements[y].GetComponent<Renderer>().material = UIMaterials[y];
                    Debug.Log("ASDGA");
                }
            }
            else
            {
                // Se puede cambiar por un set active del objeto e el panel al que se le asignará el material
                UIElements[y].SetActive(false);
                //GameMaterials[y] = Panel.GetComponent<Renderer>().material;
            }

            //UIElements[y].GetComponent<Renderer>().material = GameMaterials[y];

            y++;
        }
    }

    // Función que se encarga de activar o desactivar los botones de "previo" o "siguiente" cuando es necesario
    // Toma como argumento la lista de la categoría seleccionada para usar su longitud en la toma de decisiones
    private void ActivatePrevNext(GameObject[] LIST)
    {
        // Si hay más elementos de los que caben dentro de la UI, y el iterador indica que se encuentra en la primera página,
        // se activa únicamente el botón de Siguiente. El botón de Previo se desactiva
        if (LIST.Length > GameItems.Length && ActiveIterator == 0)
        {
            PrevButton.SetActive(false);
            NextButton.SetActive(true);
        }
        // Si hay más elementos de los que caben en la UI, y el iterador indica que se encuentra después de la primera página,
        // se activa el botón de Previo. Se verifica si hay que activar el botón de Siguiente
        else if (LIST.Length > GameItems.Length && ActiveIterator > 0)
        {
            PrevButton.SetActive(true);
            // Se encarga de verificar si existen elementos válidos en la lista después de los elementos activos,
            // para activar el botón de Siguiente página
            if (ActiveGameObjList[(ActiveIterator * GameItems.Length) + GameItems.Length] == null)
            {
                NextButton.SetActive(false);
            }
            else
            {
                NextButton.SetActive(true);
            }
            //NextButton.SetActive(true);
        }
        // Si hay menos elementos de los que caben en la UI, se desactivan ambos botones
        else if (LIST.Length <= GameItems.Length)
        {
            PrevButton.SetActive(false);
            NextButton.SetActive(false);
        }

    }
    #endregion

    // Función que se encarga de asignar los objetos y materiales activos para el usuario
    private void DoAction(string chosen)
    {
        int Selection = 0;
        switch (chosen)
        {
            case "OBJ1":
                Selection = 0;
                break;
            case "OBJ2":
                Selection = 1;
                break;
            case "OBJ3":
                Selection = 2;
                break;
            case "OBJ4":
                Selection = 3;
                break;
            case "OBJ5":
                Selection = 4;
                break;
            case "OBJ6":
                Selection = 5;
                break;
            case "OBJ7":
                Selection = 6;
                break;
            case "OBJ8":
                Selection = 7;
                break;
            case "OBJ9":
                Selection = 8;
                break;
            case "OBJ10":
                Selection = 9;
                break;
            case "OBJ11":
                Selection = 10;
                break;
            case "OBJ12":
                Selection = 11;
                break;
            case "OBJ13":
                Selection = 12;
                break;
            case "OBJ14":
                Selection = 13;
                break;
            case "OBJ15":
                Selection = 14;
                break;
            default:
                break;
        }

        if (GameItems[Selection])
        {
            ActiveObject = GameItems[Selection];
        }
        if (GameMaterials[Selection])
        {
            ActiveMaterial = GameMaterials[Selection];
        }
    }

    private void DoOption(string chosen)
    {
        switch (chosen)
        {
            // Menú Principal
            case "OPC1":
                SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
                break;
            // Cambiar Escenario
            case "OPC2":
                SceneManager.LoadScene("SceneSelect", LoadSceneMode.Single);
                break;
            // Reiniciar Escenario
            case "OPC3":
                Scene thisScene = SceneManager.GetActiveScene();
                SceneManager.LoadScene(thisScene.name, LoadSceneMode.Single);
                break;
            // Terminar
            case "OPC4":
                SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
                break;
            // Tutorial
            case "OPC5":
                SceneManager.LoadScene("Tutorial", LoadSceneMode.Single);
                break;
            case "OPC6":
                break;
            case "OPC7":
                break;
            case "OPC8":
                break;
        }
    }

    #region Control de instanciado y general
    private void GeneralRaycast()
    {
        RaycastHit hit;
        bool mRaycast;
        if (!UseLayer)
        {
            mRaycast = Physics.Raycast(RightController.position, RightController.TransformDirection(Vector3.forward), out hit, 20/*, ActiveLayer*/);
            if (mRaycast)
            {
                if (RightIndexTrigger/* && !HasObjectInHand*/)
                {
                    //Efectos de sonido PINTAR PAREDES
                    GameObject sound = Instantiate(paintObjAudioSource, PanelAnim.gameObject.transform);
                    Destroy(sound, sound.GetComponent<AudioSource>().clip.length);
                    hit.collider.GetComponent<Renderer>().material = ActiveMaterial;
                }
            }
        }
        else
        {
            mRaycast = Physics.Raycast(RightController.position, RightController.TransformDirection(Vector3.forward), out hit, 20, ActiveLayer);
            if (mRaycast)
            {
                // Si el usuario está sujetando un objeto y presiona el botón de soltar, se desactiva el booleano de sujeción,
                // por lo que según los otros algoritmos hará que el objeto que estaba sujetando deje de moverse
                if (HasObjectInHand && RightIndexTrigger)
                {
                    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    //HeldObject.GetComponent<MeshRenderer>().material.SetFloat("_Hologram", 0);
                    //Color color = HeldObject.GetComponent<MeshRenderer>().material.color;
                    //color.a = 1f;
                    //HeldObject.GetComponent<MeshRenderer>().material.color = color;
                    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    
                    //Efectos de sonido
                    GameObject sound = Instantiate(putObjAudioSource, PanelAnim.gameObject.transform);
                    Destroy(sound, sound.GetComponent<AudioSource>().clip.length);

                    HasObjectInHand = false;
                }

                // Si el usuario no está sujetando un objeto, presiona el botón:
                // Instancia al objeto en el punto que hace contacto el Raycast
                // Indica que el usuario está sujetando un objeto
                if (!HasObjectInHand && RightIndexTrigger)
                {
                    HeldObject = Instantiate(ActiveObject, hit.point, Quaternion.identity);

                    //Efectos de sonido
                    GameObject sound = Instantiate(instansiateObjAudioSource, PanelAnim.gameObject.transform);
                    Destroy(sound, sound.GetComponent<AudioSource>().clip.length);
                    
                    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    //HeldObject.GetComponent<MeshRenderer>().material.SetFloat("_Hologram", 1);
                    //Color color = HeldObject.GetComponent<MeshRenderer>().material.color;
                    //color.a = 0;//0.5f;
                    //HeldObject.GetComponent<MeshRenderer>().material.color = color;
                    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

                    HasObjectInHand = true;
                }
                // Si el objeto con el que hace contacto el Raycast, y el objeto que el usuario está sujetando
                // no son iguales:
                // El objeto sujetado se posiciona en el punto de contacto del Raycast
                else if (hit.collider.gameObject != HeldObject && HasObjectInHand)
                {
                    HeldObject.transform.position = hit.point;

                    ////Efectos de sonido
                    //GameObject sound = Instantiate(putObjAudioSource, PanelAnim.gameObject.transform);
                    //Destroy(sound, sound.GetComponent<AudioSource>().clip.length);
                }
            }

            else
            {
                RayReset();
            }

            if (RightHandTrigger)
            {
                RayReset();
                Deleter = true;
                rightRay.GetComponent<LineRenderer>().material.color = Color.red;
            }
        }
    }

    // Función que se encarga de borrar objetos
    private void DeleteRaycast()
    {
        RaycastHit hit;
        bool mRaycast;
        mRaycast = Physics.Raycast(RightController.position, RightController.TransformDirection(Vector3.forward), out hit, 20, layerMask4);
        if (mRaycast && RightIndexTrigger)
        {
            //Destroy(hit.transform.gameObject);

            // PRUEBAS 27 DE MAYO DE 2021
            if (hit.collider.CompareTag("Root"))
            {
                //Efectos de sonido
                GameObject sound = Instantiate(destroyObjAudioSource, PanelAnim.gameObject.transform);
                Destroy(sound, sound.GetComponent<AudioSource>().clip.length);

                Destroy(hit.collider.gameObject);
            }
            else if (hit.transform.parent.CompareTag("Root"))
            {
                //Efectos de sonido
                GameObject sound = Instantiate(destroyObjAudioSource, PanelAnim.gameObject.transform);
                Destroy(sound, sound.GetComponent<AudioSource>().clip.length);

                Destroy(hit.transform.parent.gameObject);
            }
            else if (hit.transform.parent.parent.CompareTag("Root"))
            {
                //Efectos de sonido
                GameObject sound = Instantiate(destroyObjAudioSource, PanelAnim.gameObject.transform);
                Destroy(sound, sound.GetComponent<AudioSource>().clip.length);

                Destroy(hit.transform.parent.parent.gameObject);
            }
            //--------------------------------------

        }

        if (RightHandTrigger)
        {
            //RayReset();
            Deleter = false;
            rightRay.GetComponent<LineRenderer>().material.color = Color.blue;
        }
    }

    // Función que reinicia el estado del raycast (destruye el objeto sujetado, y reinicia el booleano)
    private void RayReset()
    {
        Destroy(HeldObject);
        HasObjectInHand = false;
    }

    // Función que se encarga de controlar la rotación de los objetos que está sujetando el objeto
    private void ObjRotate()
    {
        float yRotValue = HeldObject.transform.rotation.y;
        if (yRotValue <= 360)
        {
            yRotValue = 0;
        }
        if (yRotValue >= 0)
        {
            yRotValue = 360;
        }

        if (OVRInput.Get(OVRInput.Button.SecondaryThumbstickUp))
        {
            HeldObject.transform.Rotate(HeldObject.transform.rotation.x,
                yRotValue + 1,
                HeldObject.transform.rotation.z);
        }

        if (OVRInput.Get(OVRInput.Button.SecondaryThumbstickDown))
        {
            HeldObject.transform.Rotate(HeldObject.transform.rotation.x,
                yRotValue - 1,
                HeldObject.transform.rotation.z);
        }
    }

    //Seria mejor un metodo que al presionar el boton se cambie a la rotacion especifica para que parezca de noche y con otra presion del boton parezca de día
    // Función que hace rotar el componente de iluminación de la escena
    private void Daylight()
    {
        Debug.LogError("Haciendo de día");
        dayAmbientAudioSource.SetActive(true);
        nightAmbientAudioSource.SetActive(false);
        float xValue = mDirLight.transform.rotation.x;
        //if (LeftHandTrigger)
        //{
        //    //if (xValue <= 360)
        //    //{
        //    //    Debug.LogError("Reinicio");
        //    //    xValue = 0;
        //    //}

        //    //mDirLight.transform.rotation = new Quaternion(xValue+1,mDirLight.transform.rotation.y, mDirLight.transform.rotation.z, mDirLight.transform.rotation.w);

        //    mDirLight.transform.Rotate(xValue + 1, 0, 0);
        //        //mDirLight.transform.rotation.y,
        //        //mDirLight.transform.rotation.z,
        //        //Space.World);
        //}
       
        //if (xValue <= 360 && xValue <= 0)
        //{
        //    dayAmbientAudioSource.SetActive(false);
        //    nightAmbientAudioSource.SetActive(true);
        //    Debug.LogError("Noche");
        //}
        //else
        //{
        //    dayAmbientAudioSource.SetActive(true);
        //    nightAmbientAudioSource.SetActive(false);
        //    Debug.LogError("Dia");
        //}

        mDirLight.transform.rotation = new Quaternion(0.4082179f, -0.2345697f, 0.1093816f, 0.8754261f); ;

        RenderSettings.skybox = materialDia;

        //isDayLight = true;
        StartCoroutine(waitAndToggle());
    }

    public void NightLight()
    {
        Debug.LogError("Haciendo de noche");
        dayAmbientAudioSource.SetActive(false);
        nightAmbientAudioSource.SetActive(true);
        mDirLight.transform.rotation = new Quaternion(0.9622502f, 0.02255757f, 0.2578341f, -0.08418602f);

        RenderSettings.skybox = materialNoche;

        //isDayLight = false;
        StartCoroutine(waitAndToggle());
    }

    IEnumerator waitAndToggle()
    {
        yield return new WaitForSeconds(.5f);
        if (isDayLight)
        {
            isDayLight = false;
        }
        else
        {
            isDayLight = true;
        }
    }

    public void playStepsSound()
    {
        Vector2 leftJoystick = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick);
        if (leftJoystick.magnitude > 0.01)
        {
            if (playedOnce == false)
            {
                //Efectos de sonido
                GameObject sound = Instantiate(stepsAudioSource, gameObject.transform);
                //Destroy(sound, sound.GetComponent<AudioSource>().clip.length);
                playedOnce = true;
            }
        }    
        else
        {
            GameObject theSound = GameObject.Find("Steps AudioSource(Clone)");
            theSound.GetComponent<AudioSource>().volume /= 2;
            theSound.GetComponent<AudioSource>().volume /= 2;
            Destroy(theSound, 0.25f);
            playedOnce = false;
        }
    }

    void DrawRay(GameObject rayObj, Transform controller, RaycastHit hit, bool isHitting)
    {
        if (isHitting)
        {
            LineRenderer ray = rayObj.GetComponent<LineRenderer>();
            ray.enabled = true;
            ray.SetPosition(0, controller.localPosition);
            ray.SetPosition(1, controller.InverseTransformPoint(hit.point));
        }
        else
        {
            rayObj.GetComponent<LineRenderer>().enabled = false;
        }
    }
    #endregion
}
