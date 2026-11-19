
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class PruebaAvisos : MonoBehaviour
{
    public AvisosRPC avisos;

    public void EnviarAvisoTodos()
    {
        avisos.AvisarATodos("La partida va a comenzar");
    }

    public void EnviarAvisoJugador()
    {
        Player[] jugadores = PhotonNetwork.PlayerList;

        foreach (Player jugador in jugadores)
        {
            if (jugador != PhotonNetwork.LocalPlayer)
            {
                avisos.AvisarAJugador(
                    "Este mensaje es para ti",
                    jugador
                );

                break;
            }
        }
    }
}
