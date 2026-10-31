using UnityEngine;

public class CanonDisparo : MonoBehaviour
{
    public Transform puntoDisparo;
    public GameObject prefabProyectil;

    // Ahora recibe la fuerza como parámetro desde el ControladorAR
    public void Disparar(float fuerzaAplicada)
    {
        if (puntoDisparo == null || prefabProyectil == null)
        {
            Debug.LogError("Faltan referencias del cañón.");
            return;
        }

        GameObject proyectil = Instantiate(
            prefabProyectil,
            puntoDisparo.position,
            puntoDisparo.rotation
        );

        Rigidbody rb = proyectil.GetComponentInChildren<Rigidbody>();

        if (rb == null)
        {
            Debug.LogError("No se encontró Rigidbody en la esfera.");
            Destroy(proyectil);
            return;
        }

        rb.isKinematic = false;
        rb.useGravity = true;
        rb.linearDamping = 0f;
        rb.constraints = RigidbodyConstraints.None;

        Vector3 direccion = puntoDisparo.up.normalized;

        // Multiplicamos la dirección por la fuerza cargada
        rb.linearVelocity = direccion * fuerzaAplicada;

        Debug.Log("Disparo realizado con fuerza: " + fuerzaAplicada);
    }
}