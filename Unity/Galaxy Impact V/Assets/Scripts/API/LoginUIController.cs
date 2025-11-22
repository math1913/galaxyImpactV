using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class LoginUIController : MonoBehaviour
{

    public TMP_InputField usernameField;
    public TMP_InputField passwordField;
    public TMP_Text messageText;
    public AuthService authService;


    public async void OnLoginButton()
    {
        string user = usernameField.text;
        string pass = passwordField.text;


        var response = await authService.Login(user, pass);


        if (response.status == 200)
        {
        messageText.text = "Login Correcto";
        SceneManager.LoadScene("MainMenu");
        }
        else
        {
        messageText.text = "Error: Credenciales incorrectas";
        }
    }


    public void OnRegisterButton()
    {
        SceneManager.LoadScene("registrer");
    }
}