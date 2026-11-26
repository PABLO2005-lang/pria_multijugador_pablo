
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

// Conexión de la escena Arena (UD03): conecta con Photon y entra
// directamente en la sala «arena». No usa el lobby de la P1.
public class ConexionArena : MonoBehaviourPunCallbacks
{
    [SerializeField] private TMP_Text estadoText;

    private void Start()
    {
        estadoText.text = "Conectando con Photon...";
        PhotonNetwork.NickName = "Jugador" + Random.Range(1000, 10000);
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        estadoText.text = "Conectado. Entrando en la sala arena...";

        // Si la sala existe, entra; si no, la crea.
        RoomOptions opciones = new RoomOptions { MaxPlayers = 8 };
        PhotonNetwork.JoinOrCreateRoom("arena", opciones, TypedLobby.Default);
    }

    public override void OnJoinedRoom()
    {
        estadoText.text = "Sala arena · Jugadores: " + PhotonNetwork.CurrentRoom.PlayerCount
            + "\nTú eres " + PhotonNetwork.NickName
            + " (número " + PhotonNetwork.LocalPlayer.ActorNumber + ")";
    }

    public override void OnPlayerEnteredRoom(Player nuevo)
    {
        OnJoinedRoom();
    }

    public override void OnPlayerLeftRoom(Player otro)
    {
        OnJoinedRoom();
    }

    public override void OnDisconnected(DisconnectCause causa)
    {
        estadoText.text = "Desconectado: " + causa;
    }
}
