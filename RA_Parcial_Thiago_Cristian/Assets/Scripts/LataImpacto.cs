using UnityEngine;

public class LataImpacto : MonoBehaviour
{
    private bool yaPuntuada = false;

    void OnCollisionEnter(Collision col)
    {
        // Si la lata choca contra el proyectil y no había sido puntuada antes
        if (!yaPuntuada && col.gameObject.CompareTag("Proyectil"))
        {
            yaPuntuada = true; // La marcamos para que no sume doble

            // Buscamos el gestor y le sumamos 1 punto
            GestorPuntuacion gestor = FindFirstObjectByType<GestorPuntuacion>();
            if (gestor != null)
            {
                gestor.SumarLata();
            }

            // Opcional: Cambiar el color de la lata para indicar que ya fue derribada
            // GetComponent<MeshRenderer>().material.color = Color.gray;
        }
    }
}