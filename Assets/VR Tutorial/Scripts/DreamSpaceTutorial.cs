using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

// Código mixto entre el tutorial y el control general de DreamSpace
public class DreamSpaceTutorial : MonoBehaviour
{
    // GameObject que contiene la cámara del espectador
    public GameObject POVCam;

    #region Tutorial Variables
    public GameObject ObjCat, WallCat/*, CatIterator*/;

    public Transform RightController;

    public Animator Banner;
    public GameObject BannerBackground;

    public Material[] BannerMats, BannerBgdMats, BannerInnerMats, BannerBgdInnerMats;

    public GameObject Rayo;

    private int Stage, SubStage, SubStage2;
    #endregion

    #region DreamSpace Variables
    //// GameObject que representa al rayo
    //public GameObject Rayo;

    //// Objeto que almacena la posición del control derecho
    //public Transform RightController;

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

    // Material activo
    private Material ActiveMaterial;

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
    public GameObject stepsAudioSource;

    bool isDayLight;
    bool playedOnce;

    #endregion
    #endregion

    #region Control por Oculus y Teclado
    public static bool RightIndexTrigger, LeftIndexTrigger, RightHandTrigger, LeftHandTrigger;

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

        if (Input.GetKeyDown(KeyCode.Q) || OVRInput.Get(OVRInput.Button.PrimaryHandTrigger))
        {
            LeftHandTrigger = true;
        }
        else
        {
            LeftHandTrigger = false;
        }
    }
    #endregion

    IEnumerator ActivatePOV()
    {
        POVCam.SetActive(false);
        yield return new WaitForSeconds(0.2f);
        POVCam.SetActive(true);
        yield break;
    }

    void Start()
    {
        // Permite que en la pantalla del ordenador se observe el punto de vista del usuario, sin distorsión
        StartCoroutine("ActivatePOV");
        Debug.LogError("Doing Activate POV");

        #region DreamSpaceStart
        Iterator.SetActive(false);
        ActiveIterator = 0;
        //FillActiveLists(GO_Cat5, Mat_Cat5);
        SetUIActiveLists();
        CatIterator.SetActive(false);

        #region Inicialización de valores fuera del UI
        ActiveObject = GameItems[0];
        UseLayer = true;
        ActiveLayer = layerMask1;


        Rayo.GetComponent<LineRenderer>().material.color = Color.blue;
        Deleter = false;
        #endregion
        #endregion

        #region Tutorial Start
        Stage = 0;
        SubStage = 0;
        SubStage2 = 0;

        Banner.GetComponent<Renderer>().material = BannerMats[Stage];
        Banner.Play("BannerFirstShow");
        #endregion
    }
    
    void Update()
    {
        #region DreamSpace Update
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
        if ((Stage == 3 && (SubStage == 0 || SubStage == 3)) || (Stage == 9 && (SubStage == 3 || SubStage == 6))) /////////////////////////////////////////////////////////////////////////////
        {
            if (LeftIndexTrigger)
            {
                ShowPanel();

                // Resetear el Rayo fuera de la UI
                RayReset();
                // Resetear el modo de Borrado
                Deleter = false;
                Rayo.GetComponent<LineRenderer>().material.color = Color.blue;
            }
        }

        #region Acciones en Update fuera de la UI
        if (!IsPanelActive)
        {
            if (!Deleter)
            {
                GeneralRaycast();
            }
            else
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

        if (LeftHandTrigger && isDayLight)
        {
            NightLight();
        }

        //Efectos de sonido de caminar
        playStepsSound();

        #endregion
        #endregion

        #region Tutorial Update
        if (Input.GetKeyDown(KeyCode.Q))
        {
            Stage++;
            StartCoroutine("ResetBanner");
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            SubStage++;
            StartCoroutine("ResetMenuBanner");
        }

        switch (Stage)
        {
            case 0:
                Stage0();
                break;

            case 1:
                Stage1();
                break;

            case 2:
                Stage2();
                break;

            case 3:
                Stage3();
                break;

            case 4:
                Stage4();
                break;

            case 5:
                Stage5();
                break;

            case 6:
                Stage6();
                break;

            case 7:
                Stage7();
                break;

            case 8:
                Stage8();
                break;

            case 9:
                Stage9();
                break;
            case 10:
                Stage10();
                break;
        }
        #endregion
    }

    #region Tutorial
    // Comprobar movimiento
    private void Stage0()
    {
        // Si el usuario mueve el joystick de movimiento
        if (OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick) != new Vector2(0, 0))
        {
            StartCoroutine(ResetBanner());

            Stage = 1;
        }
    }

    // Comprobar control de iluminación
    private void Stage1()
    {
        // Si el usuario suelta el botón para controlar la iluminación
        if (OVRInput.GetUp(OVRInput.Button.PrimaryHandTrigger))
        {
            StartCoroutine(ResetBanner());

            Stage = 2;
        }
    }

    // Comprobar la selección de elementos de la UI
    private void Stage2()
    {
        Rayo.SetActive(true);
        // Si el usuario selecciona el banner
        RaycastHit Hit;

        if (Physics.Raycast(RightController.position, RightController.TransformDirection(Vector3.forward), out Hit, 10))
        {
            if (Hit.collider.name == "InstructionBanner" && OVRInput.GetDown(OVRInput.Button.SecondaryIndexTrigger))
            {
                StartCoroutine(ResetBanner());

                Stage = 3;
            }
        }

    }

    // Controlar la apertura de interfaz de usuario
    private void Stage3()
    {
        // Si el usuario presiona el botón para abrir el menú
        if (OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger) && SubStage == 0)
        {
            StartCoroutine(ResetMenuBanner());

            SubStage = 1;

            ObjCat.SetActive(true);
            WallCat.SetActive(false);
        }

        RaycastHit Hit;
        bool rayo = Physics.Raycast(RightController.position, RightController.TransformDirection(Vector3.forward), out Hit, 10);

        // Si selecciona una categoría
        if (SubStage == 1)
        {
            Debug.LogError("Orden1");
            if (rayo && OVRInput.GetDown(OVRInput.Button.SecondaryIndexTrigger) && Hit.collider.name.Contains("CAT"))
            {
                Debug.LogError("Orden2");
                StartCoroutine(ResetMenuBanner());
                
                SubStage = 2;
            }
        }

        // Si selecciona un elemento
        if (SubStage == 2)
        {
            if (rayo && OVRInput.GetDown(OVRInput.Button.SecondaryIndexTrigger) && Hit.collider.name.Contains("OBJ"))
            {
                StartCoroutine(ResetMenuBanner());

                SubStage = 3;
            }
        }

        // Cuando cierre el menú
        if (SubStage == 3)
        {
            if (OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger))
            {
                StartCoroutine(ResetBanner());

                Stage = 4;
            }
        }
    }

    // Aparecer el objeto seleccionado
    private void Stage4()
    {
        RaycastHit Hit;
        bool rayo = Physics.Raycast(RightController.position, RightController.TransformDirection(Vector3.forward), out Hit, 20, ActiveLayer);

        if (OVRInput.GetDown(OVRInput.Button.SecondaryIndexTrigger) && rayo)
        {
            StartCoroutine(ResetBanner());

            Stage = 5;
        }
    }

    // Rotar el objeto sostenido
    private void Stage5()
    {
        if (OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick).y > 0)
        {
            StartCoroutine(ResetBanner());

            Stage = 6;
        }
    }

    // Colocar el objeto seleccionado
    private void Stage6()
    {
        RaycastHit Hit;
        bool rayo = Physics.Raycast(RightController.position, RightController.TransformDirection(Vector3.forward), out Hit, 20, ActiveLayer);

        if (OVRInput.GetDown(OVRInput.Button.SecondaryIndexTrigger) && rayo)
        {
            StartCoroutine(ResetBanner());

            Stage = 7;
        }
    }

    // Acceder al modo de borrado
    private void Stage7()
    {
        if (OVRInput.GetDown(OVRInput.Button.SecondaryHandTrigger))
        {
            StartCoroutine(ResetBanner());

            Stage = 8;
        }
    }

    // Borrar el objeto al que se apunta
    private void Stage8()
    {
        if (OVRInput.GetDown(OVRInput.Button.SecondaryIndexTrigger))
        {
            StartCoroutine(ResetBanner());

            Stage = 9;
        }
    }

    // Abrir el menú para acceder al modo de pintar paredes
    private void Stage9()
    {
        if (SubStage == 3 && OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger))
        {
            StartCoroutine(ResetMenuBanner());

            SubStage = 4;

            ObjCat.SetActive(false);
            CatIterator.SetActive(false);
            WallCat.SetActive(true);
        }

        RaycastHit Hit;
        bool rayo = Physics.Raycast(RightController.position, RightController.TransformDirection(Vector3.forward), out Hit, 10);

        // Seleccionar categoría de paredes
        if (SubStage == 4)
        {
            if (rayo && OVRInput.GetDown(OVRInput.Button.SecondaryIndexTrigger) && Hit.collider.name == "CAT4")
            {
                StartCoroutine(ResetMenuBanner());

                SubStage = 5;
            }
        }

        // Seleccionar una textura
        if (SubStage == 5 && OVRInput.GetDown(OVRInput.Button.SecondaryIndexTrigger) && Hit.collider.name.Contains("OBJ"))
        {
            StartCoroutine(ResetMenuBanner());

            SubStage = 6;
        }

        // Cerrar el menú
        if (SubStage == 6 && OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger))
        {
            StartCoroutine(ResetBanner());

            Stage = 10;
        }
    }

    // Pintar una pared
    private void Stage10()
    {
        if (OVRInput.GetDown(OVRInput.Button.SecondaryIndexTrigger))
        {
            if (OVRInput.GetDown(OVRInput.Button.SecondaryIndexTrigger))
            {
                StartCoroutine(ResetBanner());

                Stage = 11;

                StartCoroutine(ReturnToMenu());
            }
        }
    }

    IEnumerator ResetBanner()
    {
        Banner.Play("BannerHide");
        // Cambiar el material del banner
        yield return new WaitForSeconds(0.5f);
        Banner.GetComponent<Renderer>().material = BannerMats[Stage /* - 1 */];
        BannerBackground.GetComponent<Renderer>().material = BannerBgdMats[Stage];
        Banner.Play("BannerShow");

        yield return 0;
    }

    IEnumerator ResetMenuBanner()
    {
        Banner.Play("BannerMenuHide");
        // Cambiar el material del banner
        yield return new WaitForSeconds(0.5f);
        Banner.GetComponent<Renderer>().material = BannerInnerMats[SubStage - 1];
        BannerBackground.GetComponent<Renderer>().material = BannerBgdInnerMats[SubStage - 1];
        Banner.Play("BannerMenuShow");

        yield return 0;
    }

    IEnumerator ReturnToMenu()
    {
        yield return new WaitForSeconds(3);
        SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
        yield return 0;
    }
    #endregion

    #region DreamSpace Control
    #region UI Panel Controller
    // Función que administra el comportamiento del Raycast cuando el menú está activo
    private void MenuRaycast()
    {
        RaycastHit Hit;

        // Booleano que representa que el Raycast tocó algún objeto en un rango predeterminado
        bool Reicast = Physics.Raycast(RightController.position, RightController.TransformDirection(Vector3.forward), out Hit, 200);

        if (Reicast)
        {
            if (Hit.collider.name.Contains("OBJ"))
            {
                //Efectos de sonido
                GameObject sound = Instantiate(button4AudioSource, PanelAnim.gameObject.transform);
                Destroy(sound, sound.GetComponent<AudioSource>().clip.length);

                //Panel.GetComponent<Renderer>().material = Hit.collider.GetComponent<Renderer>().material;
                DoAction(Hit.collider.name);
                // Activa el Iterador visual
                Iterator.SetActive(true);
                // Crea y aigna un vector de posición para el iterador visual, que coincide con el elemento seleccionado
                //SelectedUIElem = new Vector3(Hit.collider.transform.position.x,
                //                             Hit.collider.transform.position.y,
                //                             Hit.collider.transform.position.z + 0.0001f);
                //Iterator.transform.position = SelectedUIElem;
                Iterator.transform.SetParent(Hit.collider.transform);
                Iterator.transform.localPosition = new Vector3(0, 0.01f, 0);

            }

            if (Hit.collider.name.Contains("OPC"))
            {
                //Efectos de sonido
                GameObject sound = Instantiate(button3AudioSource, PanelAnim.gameObject.transform);
                Destroy(sound, sound.GetComponent<AudioSource>().clip.length);

                //DoOption(Hit.collider.name);
            }
            /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            // Si el usuario selecciona un elemento que se refiere a una nueva categoría
            // - Reinicia el iterador activo (porque el menú se muestra desde la primera página de contenido)
            // - Se llama a la función que modifica las listas activas de acuerdo con la categoría seleccionada
            // - Finalmente se llama a la función que modifica la UI de acuerdo con las listas que se modificaron
            if (Hit.collider.name.Contains("CAT"))
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

                CatIterator.transform.SetParent(Hit.collider.transform);
                CatIterator.transform.localPosition = new Vector3(0, 0.01f, 0);


                // Desactiva el iterador visual cuando se cambia de página
                Iterator.SetActive(false);

                // Selecciones por categoría;
                if (Hit.collider.name == "CAT1")
                {
                    FillActiveLists(GO_Cat1, Mat_Cat1);
                    ElementHeader.GetComponent<Renderer>().material = ElemHeaderMats[0];
                }
                if (Hit.collider.name == "CAT2")
                {
                    FillActiveLists(GO_Cat2, Mat_Cat2);
                    ElementHeader.GetComponent<Renderer>().material = ElemHeaderMats[1];
                }
                if (Hit.collider.name == "CAT3")
                {
                    FillActiveLists(GO_Cat3, Mat_Cat3);
                    ElementHeader.GetComponent<Renderer>().material = ElemHeaderMats[2];
                }
                if (Hit.collider.name == "CAT4")
                {
                    FillActiveLists(GO_Cat4, Mat_Cat4);
                    UseLayer = false;
                    ElementHeader.GetComponent<Renderer>().material = ElemHeaderMats[3];
                }
                if (Hit.collider.name == "CAT5")
                {
                    FillActiveLists(GO_Cat5, Mat_Cat5);
                    UseLayer = true;
                    ActiveLayer = layerMask1;
                    ElementHeader.GetComponent<Renderer>().material = ElemHeaderMats[4];
                }
                if (Hit.collider.name == "CAT6")
                {
                    FillActiveLists(GO_Cat6, Mat_Cat6);
                    ElementHeader.GetComponent<Renderer>().material = ElemHeaderMats[5];
                }
                if (Hit.collider.name == "CAT7")
                {
                    FillActiveLists(GO_Cat7, Mat_Cat1);
                    ElementHeader.GetComponent<Renderer>().material = ElemHeaderMats[6];
                }
                if (Hit.collider.name == "CAT8")
                {
                    FillActiveLists(GO_Cat8, Mat_Cat2);
                    ElementHeader.GetComponent<Renderer>().material = ElemHeaderMats[7];
                }

                SetUIActiveLists();
            }

            // Cambio de página
            // - Se incrementa o decrementa el iterador activo, y se llama a la función que actualiza las listas activas
            // - Se llama a la función que activa o desactiva los botones de siguiente o previo
            if (Hit.collider.name == "Prev" && ActiveIterator > 0)
            {
                //Efectos de sonido
                GameObject sound = Instantiate(button2AudioSource, PanelAnim.gameObject.transform);
                Destroy(sound, sound.GetComponent<AudioSource>().clip.length);

                ActiveIterator--;
                SetUIActiveLists();
                ActivatePrevNext(TempGO);

                Iterator.SetActive(false);
            }
            if (Hit.collider.name == "Next")
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
            if (Hit.collider.name == "Close")
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
                UIElements[y].SetActive(true);

                GameMaterials[y] = ActiveMatList[n];

                UIElements[y].GetComponent<Renderer>().material = GameMaterials[y];
            }
            else
            {
                // Se puede cambiar por un set active del objeto e el panel al que se le asignará el material
                UIElements[y].SetActive(false);
                //GameMaterials[y] = Panel.GetComponent<Renderer>().material;


            }

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

    #region Control de instanciado y general
    private void GeneralRaycast()
    {
        RaycastHit hit;
        bool mRaycast;
        if (!UseLayer)
        {
            mRaycast = Physics.Raycast(RightController.position, RightController.TransformDirection(Vector3.forward), out hit, 20);
            if (mRaycast)
            {
                if (RightIndexTrigger/* && !HasObjectInHand*/)
                {
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
                    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    
                    //Efectos de sonido
                    GameObject sound = Instantiate(putObjAudioSource, PanelAnim.gameObject.transform);
                    Destroy(sound, sound.GetComponent<AudioSource>().clip.length);

                    HasObjectInHand = false;
                    Debug.LogError("Orden 3");
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
                    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

                    HasObjectInHand = true;

                    Debug.LogError("Orden 4");
                }
                // Si el objeto con el que hace contacto el Raycast, y el objeto que el usuario está sujetando
                // no son iguales:
                // El objeto sujetado se posiciona en el punto de contacto del Raycast
                else if (hit.collider.gameObject != HeldObject && HasObjectInHand)
                {
                    HeldObject.transform.position = hit.point;
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
                Rayo.GetComponent<LineRenderer>().material.color = Color.red;
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
            //Efectos de sonido
            GameObject sound = Instantiate(destroyObjAudioSource, PanelAnim.gameObject.transform);
            Destroy(sound, sound.GetComponent<AudioSource>().clip.length);

            Destroy(hit.transform.gameObject);
        }

        if (RightHandTrigger)
        {
            //RayReset();
            Deleter = false;
            Rayo.GetComponent<LineRenderer>().material.color = Color.blue;
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

    // Función que hace rotar el componente de iluminación de la escena
    private void Daylight()
    {
        Debug.LogError("Haciendo de día");
        dayAmbientAudioSource.SetActive(true);
        nightAmbientAudioSource.SetActive(false);

        //if (LeftHandTrigger)
        //{
        //    float xValue = mDirLight.transform.rotation.x;
        //    if (xValue <= 360)
        //    {
        //        xValue = 0;
        //    }

        //    mDirLight.transform.Rotate(xValue + 1,
        //        mDirLight.transform.rotation.y,
        //        mDirLight.transform.rotation.z,
        //        Space.World);
        //}

        mDirLight.transform.rotation = new Quaternion(0.4082179f, -0.2345697f, 0.1093816f, 0.8754261f); ;
        //isDayLight = true;
        StartCoroutine(waitAndToggle());
    }

    public void NightLight()
    {
        Debug.LogError("Haciendo de noche");
        dayAmbientAudioSource.SetActive(false);
        nightAmbientAudioSource.SetActive(true);
        mDirLight.transform.rotation = new Quaternion(0.9622502f, 0.02255757f, 0.2578341f, -0.08418602f);
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
    #endregion
    #endregion
}
