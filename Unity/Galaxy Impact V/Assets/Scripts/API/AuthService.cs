using UnityEngine;
using UnityEngine.Networking;
using System.Text;
using System.Threading.Tasks;


public class AuthService : MonoBehaviour
{
    private const string LOGIN_URL = "http://localhost:8080/api/auth/login";
    private const string REGISTER_URL = "http://localhost:8080/api/users";


    public async Task<LoginResponse> Login(string username, string password)
    {
        LoginRequest request = new LoginRequest { username = username, password = password };
        string json = JsonUtility.ToJson(request);


        using (UnityWebRequest www = new UnityWebRequest(LOGIN_URL, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");


            var operation = www.SendWebRequest();
            while (!operation.isDone) await Task.Yield();


            Debug.Log("Respuesta Login: " + www.downloadHandler.text);


            if (www.result != UnityWebRequest.Result.Success)
            {
                return new LoginResponse { message = www.error, status = 500 };
            }


            return JsonUtility.FromJson<LoginResponse>(www.downloadHandler.text);
        }
    }


    public async Task<RegisterResponse> Register(string username, string email, string password)
    {
        RegisterRequest request = new RegisterRequest { username = username, email = email, password = password };
        string json = JsonUtility.ToJson(request);


        using (UnityWebRequest www = new UnityWebRequest(REGISTER_URL, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");


            var operation = www.SendWebRequest();
            while (!operation.isDone) await Task.Yield();


            Debug.Log("Respuesta Registro: " + www.downloadHandler.text);


            if (www.result != UnityWebRequest.Result.Success)
            {
                return new RegisterResponse { username = "", email = "", idUsuario = -1 };
            }


            return JsonUtility.FromJson<RegisterResponse>(www.downloadHandler.text);
        }
    }
}


// ============================
// Login Models
// ============================
[System.Serializable]
public class LoginRequest
{
    public string username;
    public string password;
}


[System.Serializable]
public class LoginResponse
{
    public string message;
    public string user;
    public int status;
}


// ============================
// Register Models
// ============================
[System.Serializable]
public class RegisterRequest
{
    public string username;
    public string email;
    public string password;
}


[System.Serializable]
public class RegisterResponse
{
    public int idUsuario;
    public string username;
    public string email;
}