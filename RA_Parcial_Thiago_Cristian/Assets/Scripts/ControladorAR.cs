
using System.Collections.Generic;
using UnityEngine;
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

    [Header("Cañón existente")]
    public CanonDisparo canonDisparo;

    private bool modoCanon = true;

    private GameObject punteroInstanciado;
    private List<ARRaycastHit> hits = new List<ARRaycastHit>();

    private Pose posePuntero;
    private bool poseValida = false;

    [Header("Interacción e Inercia")]
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

        if (canonDisparo == null)
        {
            canonDisparo = FindFirstObjectByType<CanonDisparo>();
        }
    }

    public void ElegirBola()
    {
        modoCanon = true;

        if (canonDisparo == null)
        {
            canonDisparo = FindFirstObjectByType<CanonDisparo>();
        }

        if (punteroInstanciado != null)
        {
            punteroInstanciado.SetActive(false);
        }

        Debug.Log("MODO DISPARO ACTIVADO");
    }

    public void ElegirCaja()
    {
        modoCanon = false;
        Debug.Log("MODO COLOCAR CAJAS ACTIVADO");
    }

    void Update()
    {
        ActualizarPuntero();

        // CONTROLES TÁCTILES
        if (Input.touchCount > 0)
        {
            Touch toque = Input.GetTouch(0);

            if (toque.phase == TouchPhase.Began)
            {
                toqueBloqueado =
                    EventSystem.current != null &&
                    EventSystem.current.IsPointerOverGameObject(toque.fingerId);

                if (toqueBloqueado)
                    return;

                ProcesarToque(toque.position);
            }

            if (toqueBloqueado)
            {
                if (toque.phase == TouchPhase.Ended ||
                    toque.phase == TouchPhase.Canceled)
                {
                    toqueBloqueado = false;
                }

                return;
            }

            if (toque.phase == TouchPhase.Moved &&
                objetoAgarrado != null &&
                !modoCanon)
            {
                Moviendo(toque.position);
            }

            if ((toque.phase == TouchPhase.Ended ||
                 toque.phase == TouchPhase.Canceled) &&
                objetoAgarrado != null)
            {
                Soltar();
            }
        }
        // CONTROLES PARA SIMULADOR / MOUSE
        else
        {
            if (Input.GetMouseButtonDown(0))
            {
                mouseBloqueado =
                    EventSystem.current != null &&
                    EventSystem.current.IsPointerOverGameObject();

                if (!mouseBloqueado)
                {
                    ProcesarToque(Input.mousePosition);
                }
            }

            if (Input.GetMouseButton(0) &&
                objetoAgarrado != null &&
                !mouseBloqueado &&
                !modoCanon)
            {
                Moviendo(Input.mousePosition);
            }

            if (Input.GetMouseButtonUp(0))
            {
                if (objetoAgarrado != null)
                {
                    Soltar();
                }

                mouseBloqueado = false;
            }
        }
    }

    // SEPARA COMPLETAMENTE EL DISPARO DE LA COLOCACIÓN AR
    void ProcesarToque(Vector2 posicion)
    {
        if (modoCanon)
        {
            if (canonDisparo != null)
            {
                canonDisparo.Disparar();
                Debug.Log("TOQUE EN MODO CAÑÓN");
            }
            else
            {
                Debug.LogWarning("No se encontró CanonDisparo.");
            }

            return;
        }

        // Solo se ejecuta en modo colocación de cajas.
        IniciarToque(posicion);
    }

    // RAYCAST AR PARA EL PUNTERO
    void ActualizarPuntero()
    {
        if (modoCanon)
        {
            if (punteroInstanciado != null)
            {
                punteroInstanciado.SetActive(false);
            }

            return;
        }

        Vector2 centro = new Vector2(
            Screen.width / 2,
            Screen.height / 2
        );

        if (raycastManager != null &&
            raycastManager.Raycast(
                centro,
                hits,
                TrackableType.Planes))
        {
            posePuntero = hits[0].pose;
            poseValida = true;

            if (punteroInstanciado != null)
            {
                punteroInstanciado.transform.SetPositionAndRotation(
                    posePuntero.position,
                    posePuntero.rotation
                );

                punteroInstanciado.SetActive(true);
            }
        }
        else
        {
            poseValida = false;

            if (punteroInstanciado != null)
            {
                punteroInstanciado.SetActive(false);
            }
        }
    }

    // COLOCAR O AGARRAR CAJAS
    void IniciarToque(Vector2 pos)
    {
        if (Camera.main == null)
            return;

        Ray rayo = Camera.main.ScreenPointToRay(pos);

        if (Physics.Raycast(rayo, out RaycastHit hitFisico))
        {
            Rigidbody rb =
                hitFisico.collider.GetComponentInParent<Rigidbody>();

            if (rb != null)
            {
                objetoAgarrado = rb.gameObject;
                rb.isKinematic = true;

                posAnterior = objetoAgarrado.transform.position;
                return;
            }
        }

        if (poseValida && prefabCaja != null)
        {
            Instantiate(
                prefabCaja,
                posePuntero.position + Vector3.up * 0.1f,
                posePuntero.rotation
            );

            Debug.Log("CAJA COLOCADA");
        }
    }

    // ARRASTRAR CAJAS
    void Moviendo(Vector2 pos)
    {
        if (raycastManager != null &&
            raycastManager.Raycast(
                pos,
                hits,
                TrackableType.Planes))
        {
            Vector3 nuevaPos = hits[0].pose.position;
            nuevaPos.y += 0.1f;

            objetoAgarrado.transform.position = nuevaPos;

            velocidadInercia =
                (nuevaPos - posAnterior) / Time.deltaTime;

            posAnterior = nuevaPos;
        }
    }

    // SOLTAR CAJAS CON INERCIA
    void Soltar()
    {
        if (objetoAgarrado == null)
            return;

        Rigidbody rb =
            objetoAgarrado.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.isKinematic = false;
            rb.linearVelocity = velocidadInercia;
        }

        objetoAgarrado = null;
    }
}
