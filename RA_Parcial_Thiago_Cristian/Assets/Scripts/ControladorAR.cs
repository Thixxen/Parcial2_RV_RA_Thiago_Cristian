using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.EventSystems;

public class ControladorAR : MonoBehaviour
{
    [Header("Manejadores AR")]
    public ARRaycastManager raycastManager;
    public ARPlaneManager planeManager;
    public GameObject punteroVisualPrefab;

    [Header("Prefabs")]
    public GameObject prefabCaja;

    [Header("Cañón y Disparo")]
    public CanonDisparo canonDisparo;
    public Slider barraFuerza;

    // --- NUEVAS VARIABLES DE COLOR ---
    public Image imagenRellenoFuerza;
    public Gradient gradienteFuerza;

    public float fuerzaMaxima = 80f;
    public float velocidadCarga = 40f;

    private bool modoCanon = true;
    private float fuerzaActual = 0f;
    private bool estaCargando = false;

    private GameObject punteroInstanciado;
    private List<ARRaycastHit> hits = new List<ARRaycastHit>();
    private Pose posePuntero;
    private bool poseValida = false;

    [Header("Interacción e Inercia (Cajas)")]
    private GameObject objetoAgarrado;
    private Vector3 posAnterior;
    private Vector3 velocidadInercia;
    private bool toqueBloqueado = false;
    private bool mouseBloqueado = false;

    void Start()
    {
        if (punteroVisualPrefab != null)
        {
            punteroInstanciado = Instantiate(punteroVisualPrefab);
            punteroInstanciado.SetActive(false);
        }

        if (canonDisparo == null) canonDisparo = FindFirstObjectByType<CanonDisparo>();

        if (barraFuerza != null)
        {
            barraFuerza.maxValue = fuerzaMaxima;
            barraFuerza.value = 0f;
        }

        // Pintamos la barra con el color inicial (Fuerza 0)
        if (imagenRellenoFuerza != null)
        {
            imagenRellenoFuerza.color = gradienteFuerza.Evaluate(0f);
        }
    }

    public void ElegirBola()
    {
        modoCanon = true;
        if (canonDisparo == null) canonDisparo = FindFirstObjectByType<CanonDisparo>();
        if (punteroInstanciado != null) punteroInstanciado.SetActive(false);
    }

    public void ElegirCaja()
    {
        modoCanon = false;
    }

    void Update()
    {
        ActualizarPuntero();

        if (Input.touchCount > 0)
        {
            Touch toque = Input.GetTouch(0);

            if (toque.phase == TouchPhase.Began)
            {
                toqueBloqueado = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(toque.fingerId);
                if (toqueBloqueado) return;

                if (modoCanon) IniciarCarga();
                else IniciarToque(toque.position);
            }

            if (toqueBloqueado)
            {
                if (toque.phase == TouchPhase.Ended || toque.phase == TouchPhase.Canceled) toqueBloqueado = false;
                return;
            }

            if (toque.phase == TouchPhase.Stationary || toque.phase == TouchPhase.Moved)
            {
                if (modoCanon && estaCargando) CargarFuerza();
                else if (!modoCanon && objetoAgarrado != null) Moviendo(toque.position);
            }

            if (toque.phase == TouchPhase.Ended || toque.phase == TouchPhase.Canceled)
            {
                if (modoCanon && estaCargando) EjecutarDisparo();
                else if (!modoCanon && objetoAgarrado != null) Soltar();
            }
        }
        else
        {
            if (Input.GetMouseButtonDown(0))
            {
                mouseBloqueado = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
                if (mouseBloqueado) return;

                if (modoCanon) IniciarCarga();
                else IniciarToque(Input.mousePosition);
            }

            if (Input.GetMouseButton(0) && !mouseBloqueado)
            {
                if (modoCanon && estaCargando) CargarFuerza();
                else if (!modoCanon && objetoAgarrado != null) Moviendo(Input.mousePosition);
            }

            if (Input.GetMouseButtonUp(0))
            {
                if (modoCanon && estaCargando) EjecutarDisparo();
                else if (!modoCanon && objetoAgarrado != null) Soltar();
                mouseBloqueado = false;
            }
        }
    }

    void IniciarCarga()
    {
        estaCargando = true;
        fuerzaActual = 10f;
    }

    void CargarFuerza()
    {
        fuerzaActual += velocidadCarga * Time.deltaTime;

        if (barraFuerza != null) barraFuerza.value = fuerzaActual;

        // --- LÓGICA DE COLOR ---
        if (imagenRellenoFuerza != null)
        {
            // Calculamos un porcentaje de 0 a 1 (ej: 40 de fuerza sobre 80 máximo = 0.5)
            float porcentaje = fuerzaActual / fuerzaMaxima;

            // Le pedimos al gradiente que nos dé el color exacto para ese porcentaje
            imagenRellenoFuerza.color = gradienteFuerza.Evaluate(porcentaje);
        }

        if (fuerzaActual >= fuerzaMaxima)
        {
            fuerzaActual = fuerzaMaxima;
            EjecutarDisparo();
        }
    }

    void EjecutarDisparo()
    {
        if (canonDisparo != null)
        {
            canonDisparo.Disparar(fuerzaActual);
        }

        estaCargando = false;
        fuerzaActual = 0f;
        if (barraFuerza != null) barraFuerza.value = 0f;

        // Reiniciamos el color al soltar
        if (imagenRellenoFuerza != null) imagenRellenoFuerza.color = gradienteFuerza.Evaluate(0f);
    }

    void ActualizarPuntero()
    {
        if (modoCanon)
        {
            if (punteroInstanciado != null) punteroInstanciado.SetActive(false);
            return;
        }
        Vector2 centro = new Vector2(Screen.width / 2, Screen.height / 2);
        if (raycastManager != null && raycastManager.Raycast(centro, hits, TrackableType.PlaneWithinPolygon))
        {
            posePuntero = hits[0].pose;
            poseValida = true;
            if (punteroInstanciado != null)
            {
                punteroInstanciado.transform.SetPositionAndRotation(posePuntero.position, posePuntero.rotation);
                punteroInstanciado.SetActive(true);
            }
        }
        else
        {
            poseValida = false;
            if (punteroInstanciado != null) punteroInstanciado.SetActive(false);
        }
    }

    void IniciarToque(Vector2 pos)
    {
        if (Camera.main == null) return;
        Ray rayo = Camera.main.ScreenPointToRay(pos);
        if (Physics.Raycast(rayo, out RaycastHit hitFisico))
        {
            Rigidbody rb = hitFisico.collider.GetComponentInParent<Rigidbody>();
            if (rb != null)
            {
                objetoAgarrado = rb.gameObject;
                rb.isKinematic = true;
                posAnterior = objetoAgarrado.transform.position;
                return;
            }
        }
        if (poseValida && prefabCaja != null) Instantiate(prefabCaja, posePuntero.position, posePuntero.rotation);
    }

    void Moviendo(Vector2 pos)
    {
        if (raycastManager != null && raycastManager.Raycast(pos, hits, TrackableType.PlaneWithinPolygon))
        {
            Vector3 nuevaPos = hits[0].pose.position;
            objetoAgarrado.transform.position = nuevaPos;
            velocidadInercia = (nuevaPos - posAnterior) / Time.deltaTime;
            posAnterior = nuevaPos;
        }
    }

    void Soltar()
    {
        if (objetoAgarrado == null) return;
        Rigidbody rb = objetoAgarrado.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.linearVelocity = velocidadInercia;
        }
        objetoAgarrado = null;
    }
}