using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AchievementUIItem : MonoBehaviour
{
    public Image icon;
    public Image panel;  // ← Agrega esta referencia al panel del item
    public TextMeshProUGUI title;
    public TextMeshProUGUI descripcion;
    public Slider sliderProgreso;
    public TextMeshProUGUI txtProgreso;

    [Header("Colores")]
    public Color colorNormal = new Color(1,1,1,0.2f);  // semi transparente
    public Color colorCompletado = new Color(0.2f,1,0.2f,0.5f); // verde suave

    public void SetData(AchievementAPIClient.AchievementDTO dto)
    {
        title.text = dto.nombre;
        descripcion.text = dto.descripcion;

        sliderProgreso.maxValue = dto.objetivo;
        sliderProgreso.value = dto.progresoActual;
        if (dto.progresoActual > dto.objetivo)
            txtProgreso.text = dto.objetivo + " / " + dto.objetivo;
        else
            txtProgreso.text = dto.progresoActual + " / " + dto.objetivo;

        panel.color = dto.completado ? colorCompletado : colorNormal; //cambia el color si lo completa
        Debug.Log($"Logro: {dto.codigo}  Categoria: {dto.categoria} Tipo: {dto.tipo}");
    }
}
