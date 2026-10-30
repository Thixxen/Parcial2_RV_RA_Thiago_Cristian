
using UnityEngine;

public class Proyectil : MonoBehaviour
{
    public float tiempoVida = 5f;

    void Start()
    {
        Destroy(gameObject, tiempoVida);
    }

    void OnCollisionEnter(Collision collision)
    {
        Destroy(gameObject);
    }
}
