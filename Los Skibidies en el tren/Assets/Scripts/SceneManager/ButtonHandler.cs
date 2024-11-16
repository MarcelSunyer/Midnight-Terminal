using UnityEngine;
using UnityEngine.UI;

public class ButtonHandler : MonoBehaviour
{
    [SerializeField] private Button myButton; // Botón que ejecutará la acción

    // Enum con los métodos disponibles en MenuManager
    public enum MenuManagerFunction
    {
        JoinClient,
        CreateServer,
        SceneClient,
        SceneServer,
        MainMenu,
        EndGame
    }

    [SerializeField] private MenuManagerFunction selectedFunction; // Función a ejecutar en el Inspector

    private void Start()
    {
        // Asegura que el botón esté asignado en el Inspector, luego registra el evento
        if (myButton != null)
        {
            myButton.onClick.AddListener(OnButtonClicked);
        }
    }

    private void OnButtonClicked()
    {
        // Asegura que existe una instancia de MenuManager
        if (MenuManager.Instance == null)
        {
            Debug.LogError("No se encontró el MenuManager en la escena.");
            return;
        }

        // Ejecuta la función seleccionada en el Inspector
        switch (selectedFunction)
        {
            case MenuManagerFunction.JoinClient:
                MenuManager.Instance.JoinClient();
                break;
            case MenuManagerFunction.CreateServer:
                MenuManager.Instance.CreateServer();
                break;
            case MenuManagerFunction.SceneClient:
                MenuManager.Instance.SceneClient();
                break;
            case MenuManagerFunction.SceneServer:
                MenuManager.Instance.SceneServer();
                break;
            case MenuManagerFunction.MainMenu:
                MenuManager.Instance.MainMenu();
                break;
            case MenuManagerFunction.EndGame:
                MenuManager.Instance.EndGame();
                break;
            default:
                Debug.LogWarning("Función seleccionada no válida.");
                break;
        }
    }
}
