using UnityEngine;
using UnityEngine.UI;

public class ButtonHandler : MonoBehaviour
{
    [SerializeField] private Button myButton; // Bot�n que ejecutar� la acci�n

    // Enum con los m�todos disponibles en MenuManager
    public enum MenuManagerFunction
    {
        JoinClient,
        CreateServer,
        SceneClient,
        SceneServer,
        MainMenu,
        EndGame
    }

    [SerializeField] private MenuManagerFunction selectedFunction; // Funci�n a ejecutar en el Inspector

    private void Start()
    {
        // Asegura que el bot�n est� asignado en el Inspector, luego registra el evento
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
            Debug.LogError("No se encontr� el MenuManager en la escena.");
            return;
        }

        // Ejecuta la funci�n seleccionada en el Inspector
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
                Debug.LogWarning("Funci�n seleccionada no v�lida.");
                break;
        }
    }
}
