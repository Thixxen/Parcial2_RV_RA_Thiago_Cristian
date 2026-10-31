using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement; // ¡Librería clave para cambiar o reiniciar niveles!
using UnityEngine.XR.ARFoundation;

public class GestorPuntuacion : MonoBehaviour
{
    public int latasDerribadas = 0;
    public int latasParaGanar = 10;

    [Header("Interfaz")]
    public TextMeshProUGUI textoContador;
    public GameObject cartelGanaste;

    void Start()
    {
        if (cartelGanaste != null) cartelGanaste.SetActive(false);
        ActualizarUI();
    }

    public void SumarLata()
    {
        latasDerribadas++;
        ActualizarUI();

        if (latasDerribadas >= latasParaGanar)
        {
            if (cartelGanaste != null) cartelGanaste.SetActive(true);
        }
    }

    void ActualizarUI()
    {
        if (textoContador != null)
        {
            textoContador.text = "Latas: " + latasDerribadas + " / " + latasParaGanar;
        }
    }

    // --- NUEVA FUNCIÓN PARA EL BOTÓN ---
    public void ReiniciarJuego()
    {
        // 1. Apagamos el cartel de victoria
        if (cartelGanaste != null) cartelGanaste.SetActive(false);

        // 2. Reiniciamos el contador de puntos
        latasDerribadas = 0;
        ActualizarUI();

        // 3. Buscamos y destruimos todas las latas que hayan quedado en el mapa
        // (Busca todos los objetos que tengan el script LataImpacto)
        LataImpacto[] latasEnEscena = FindObjectsByType<LataImpacto>(FindObjectsSortMode.None);
        foreach (LataImpacto lata in latasEnEscena)
        {
            Destroy(lata.gameObject);
        }

        // 4. (Opcional) Buscamos y destruimos las balas/proyectiles que sigan rodando
        Proyectil[] proyectiles = FindObjectsByType<Proyectil>(FindObjectsSortMode.None);
        foreach (Proyectil bala in proyectiles)
        {
            Destroy(bala.gameObject);
        }

        // 5. Nos aseguramos de que el tiempo corra normal
        Time.timeScale = 1f;
    }
}