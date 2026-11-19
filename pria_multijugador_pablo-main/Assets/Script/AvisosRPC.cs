
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class AvisosRPC : MonoBehaviourPun
{
    [PunRPC]
    public void MostrarAviso(string mensaje)
    {
        Debug.Log("AVISO DEL ANFITRION: " + mensaje);
    }

    public void AvisarATodos(string mensaje)
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            Debug.Log("Solo el anfitrion puede enviar avisos.");
            return;
        }

        photonView.RPC(
            "MostrarAviso",
            RpcTarget.All,
            mensaje
        );
    }

    public void AvisarAJugador(string mensaje, Player jugador)
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            Debug.Log("Solo el anfitrion puede enviar avisos.");
            return;
        }

        photonView.RPC(
            "MostrarAviso",
            jugador,
            mensaje
        );
    }
}
