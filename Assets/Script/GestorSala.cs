
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
    [SerializeField] private TMP_InputField nombreJugadorInput; // G05
    [SerializeField] private TMP_Text mensajeLobbyText;
    [SerializeField] private Button[] botonesLobby;

    [Header("Sala")]
    [SerializeField] private TMP_Text infoSalaText;

    private const int MaxJugadores = 4;

    private void Start()
    {
        panelLobby.SetActive(true);
        panelSala.SetActive(false);
        ActivarBotones(false);
        mensajeLobbyText.text = "Conectando con Photon...";

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
        ActivarBotones(true);

        PhotonNetwork.JoinLobby();
    }

    // ---------- Métodos de los Botones ----------

    public void CrearSala()
    {
        PonerNombre(); // G05

        string nombre = nombreSalaInput.text.Trim();

        if (nombre.Length == 0)
        {
            mensajeLobbyText.text = "Escribe un nombre para la sala.";
            return;
        }

        ActivarBotones(false);

        mensajeLobbyText.text = "Creando la sala " + nombre + "...";

        RoomOptions opciones = new RoomOptions
        {
            MaxPlayers = MaxJugadores
        };

        PhotonNetwork.CreateRoom(nombre, opciones);
    }

    public void UnirseSala()
    {
        PonerNombre(); // G05

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
        PonerNombre(); // G05
        ActivarBotones(false);

        mensajeLobbyText.text = "Buscando una sala libre...";
        PhotonNetwork.JoinRandomRoom();
    }

    // ---------- Salir de la sala (G05) ----------

    public void SalirSala()
    {
        PhotonNetwork.LeaveRoom();
    }

    // ---------- Callbacks de salas ----------

    public override void OnJoinedRoom()
    {
        panelLobby.SetActive(false);
        panelSala.SetActive(true);
        ActualizarInfoSala();

        Debug.Log("Dentro de la sala " + PhotonNetwork.CurrentRoom.Name);
    }

    public override void OnLeftRoom()
    {
        panelSala.SetActive(false);
        panelLobby.SetActive(true);
        mensajeLobbyText.text = "Has salido de la sala.";
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
        mensajeLobbyText.text = "No hay salas libres. Creando una nueva...";

        PhotonNetwork.CreateRoom(null, new RoomOptions
        {
            MaxPlayers = MaxJugadores
        });
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

    private void PonerNombre() // G05
    {
        string nombre = nombreJugadorInput.text.Trim();

        if (nombre.Length > 0)
        {
            PhotonNetwork.NickName = nombre;
        }
    }

    private void ActualizarInfoSala()
    {
        Room sala = PhotonNetwork.CurrentRoom;

        if (sala != null)
        {
            infoSalaText.text = "Sala: " + sala.Name
                              + " · Jugadores: " + sala.PlayerCount
                              + "/" + sala.MaxPlayers;
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
