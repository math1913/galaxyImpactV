using System.Collections.Generic;
using UnityEngine;

public class AchievementMenu : MonoBehaviour
{
    [Header("Referencias")]
    public AchievementAPIClient api;
    public Transform contentPanel;        // Content del ScrollView
    public GameObject logroPrefab;

    [Header("Usuario")]
    public long userId;

    [Header("Layout")]
    public float itemHeight = 140f;       // Debe coincidir con el prefab
    public float itemSpacing = 15f;       // Igual que el VerticalLayoutGroup

    private List<AchievementAPIClient.AchievementDTO> achievements;

    private void Start()
    {
        userId = PlayerPrefs.GetInt("userId", 1);
        LoadAchievements();
    }

    public async void LoadAchievements()
    {
        achievements = await api.GetAchievements(userId);

        if (achievements == null)
        {
            Debug.LogError("No se pudieron cargar los logros.");
            return;
        }

        // Limpia anteriores
        foreach (Transform child in contentPanel)
            Destroy(child.gameObject);

        // Instancia nuevos
        foreach (var logro in achievements)
        {
            GameObject item = Instantiate(logroPrefab, contentPanel);
            var ui = item.GetComponent<AchievementUIItem>();
            ui.SetData(logro);

            // Aseguramos escala correcta
            var rt = item.GetComponent<RectTransform>();
            rt.localScale = Vector3.one;
        }

        var contentRt = contentPanel.GetComponent<RectTransform>();
        int count = achievements.Count;

        float totalHeight = count * itemHeight + (count - 1) * itemSpacing;
        // mantenemos el ancho, solo cambiamos Y
        contentRt.sizeDelta = new Vector2(contentRt.sizeDelta.x, totalHeight);

        Debug.Log($"Se cargaron {achievements.Count} logros. Altura content={totalHeight}");
    }
}
