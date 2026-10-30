
using UnityEngine;

public class CanonDisparo : MonoBehaviour
{
    public Transform puntoDisparo;
    public GameObject prefabProyectil;
    public float velocidadDisparo = 40f;

    public void Disparar()
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

        rb.linearVelocity = direccion * velocidadDisparo;

        Debug.Log("Rigidbody encontrado: " + rb.name);
        Debug.Log("Velocidad aplicada: " + rb.linearVelocity);
    }
}
