using UnityEngine;
using UnityEngine.UI;   // Button
using Photon.Pun;
using Photon.Realtime;  // RoomOptions, Player
using TMPro;

// Gestiona las salas: crear, unirse y mostrar en qué sala estamos.
public class GestorSala : MonoBehaviourPunCallbacks
{
    [Header("Paneles")]
    [SerializeField] private GameObject panelLobby;
    [SerializeField] private GameObject panelSala;

    [Header("Lobby")]
    [SerializeField] private TMP_InputField nombreSalaInput;
    [SerializeField] private TMP_Text mensajeLobbyText;
    [SerializeField] private Button[] botonesLobby;

    [Header("Sala")]
    [SerializeField] private TMP_Text infoSalaText;
    private const int MaxJugadores = 4;

    private void Start()
    {
        panelLobby.SetActive(true);
        panelSala.SetActive(false);
        ActivarBotones(false);              // Desactivados hasta estar conectados
        mensajeLobbyText.text = "Conectando con Photon...";

        // Nos conectamos a los servidores de Photon usando la configuración del archivo PhotonServerSettings
        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    // ---------- Callbacks de conexión a Photon ----------

    public override void OnConnectedToMaster()
    {
        Debug.Log("Conectado al servidor de Photon.");
        mensajeLobbyText.text = "Conectado. Escribe una sala o búscala.";
        ActivarBotones(true); // ¡Aquí es donde se activan los botones al fin!

        // Opcional: unirse al lobby por defecto si quieres usar salas aleatorias (JoinRandomRoom)
        PhotonNetwork.JoinLobby();
    }

    // ---------- Métodos de los Botones (Acciones) ----------

    public void CrearSala()
    {
        string nombre = nombreSalaInput.text.Trim();
        if (nombre.Length == 0)
        {
            mensajeLobbyText.text = "Escribe un nombre para la sala.";
            return;
        }
        ActivarBotones(false);
        mensajeLobbyText.text = "Creando la sala " + nombre + "...";
        RoomOptions opciones = new RoomOptions { MaxPlayers = MaxJugadores };
        PhotonNetwork.CreateRoom(nombre, opciones);
    }

    public void UnirseSala()
    {
        string nombre = nombreSalaInput.text.Trim();
        if (nombre.Length == 0)
        {
            mensajeLobbyText.text = "Escribe el nombre de la sala.";
            return;
        }
        ActivarBotones(false);
        mensajeLobbyText.text = "Entrando en " + nombre + "...";
        PhotonNetwork.JoinRoom(nombre);
    }

    public void UnirseAleatoria()
    {
        ActivarBotones(false);
        mensajeLobbyText.text = "Buscando una sala libre...";
        PhotonNetwork.JoinRandomRoom();
    }

    // ---------- Callbacks de salas ----------

    public override void OnJoinedRoom()
    {
        panelLobby.SetActive(false);
        panelSala.SetActive(true);
        ActualizarInfoSala();
        Debug.Log("Dentro de la sala " + PhotonNetwork.CurrentRoom.Name);
    }

    public override void OnCreateRoomFailed(short codigo, string mensaje)
    {
        mensajeLobbyText.text = "No se pudo crear la sala: " + mensaje;
        ActivarBotones(true);
    }

    public override void OnJoinRoomFailed(short codigo, string mensaje)
    {
        mensajeLobbyText.text = "No se pudo entrar: " + mensaje;
        ActivarBotones(true);
    }

    public override void OnJoinRandomFailed(short codigo, string mensaje)
    {
        // No hay ninguna sala libre: creamos una con nombre automático.
        mensajeLobbyText.text = "No hay salas libres. Creando una nueva...";
        PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = MaxJugadores });
    }

    public override void OnPlayerEnteredRoom(Player nuevoJugador)
    {
        ActualizarInfoSala();
    }

    public override void OnPlayerLeftRoom(Player jugador)
    {
        ActualizarInfoSala();
    }

    // ---------- Ayudantes ----------

    private void ActualizarInfoSala()
    {
        Room sala = PhotonNetwork.CurrentRoom;
        if (sala != null)
        {
            infoSalaText.text = "Sala: " + sala.Name
                              + " · Jugadores: " + sala.PlayerCount + "/" + sala.MaxPlayers;
        }
    }

    private void ActivarBotones(bool activos)
    {
        foreach (Button boton in botonesLobby)
        {
            if (boton != null)
            {
                boton.interactable = activos;
            }
        }
    }
}