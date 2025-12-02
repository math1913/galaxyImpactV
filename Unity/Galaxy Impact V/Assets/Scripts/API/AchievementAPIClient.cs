using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public class AchievementAPIClient : MonoBehaviour
{
    public string baseUrl = "http://localhost:8080/api/achievements";

    // ---------- MODELO REQUEST PARA updateBatch ----------
    [Serializable]
    public class AchievementBatchRequest
    {
        public long userId;

        public int killsNormal;
        public int killsFast;
        public int killsTank;
        public int killsShooter;
        public int minutesPlayed;

        public int score;

        public int pickupHealth;
        public int pickupShield;
        public int pickupAmmo;
        public int pickupExp;
    }

    // ---------- MODELO RESPUESTA LOGROS ----------
    [Serializable]
    public class AchievementDTO
    {
        public string codigo;
        public string nombre;
        public string descripcion;
        
        public int progresoActual;
        public int objetivo;

        public bool completado;

        public int puntosRecompensa;
        public string fechaDesbloqueo;
        public string categoria;
        public string tipo;
    }

    // ============================================================
    // POST /updateBatch
    // ============================================================
    public async Task<bool> SendBatch(AchievementBatchRequest batch)
    {
        string url = $"{baseUrl}/updateBatch";

        string json = JsonUtility.ToJson(batch);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);

        using UnityWebRequest req = new UnityWebRequest(url, "POST");
        req.uploadHandler = new UploadHandlerRaw(bodyRaw);
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");

        var op = req.SendWebRequest();

        while (!op.isDone)
            await Task.Yield();

        if (req.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Error enviando batch: " + req.error);
            return false;
        }

        Debug.Log("Batch enviado correctamente");
        return true;
    }

    // ============================================================
    // GET /user/{id}
    // ============================================================
    public async Task<List<AchievementDTO>> GetAchievements(long userId)
    {
        string url = $"{baseUrl}/user/{userId}";

        using UnityWebRequest req = UnityWebRequest.Get(url);

        var op = req.SendWebRequest();
        while (!op.isDone)
            await Task.Yield();

        if (req.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Error obteniendo logros: " + req.error);
            return null;
        }

        // JSON es un array → necesitamos un wrapper para JsonUtility
        string rawJson = req.downloadHandler.text;

        // JsonUtility no soporta arrays directos → envolvemos
        string wrapped = "{\"items\":" + rawJson + "}";
        AchievementListWrapper wrapper = JsonUtility.FromJson<AchievementListWrapper>(wrapped);

        return wrapper.items;
    }

    [Serializable]
    public class AchievementListWrapper
    {
        public List<AchievementDTO> items;
    }
}
