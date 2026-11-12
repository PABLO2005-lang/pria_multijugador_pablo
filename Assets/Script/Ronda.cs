
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using Hashtable = ExitGames.Client.Photon.Hashtable;

// Minijuego de reflejos.
public class Ronda : MonoBehaviourPun
{
    [SerializeField] private Button botonEmpezar;
    [SerializeField] private Button botonPulsar;
    [SerializeField] private TMP_Text rondaText;

    // Estado que solo utiliza el anfitrión.
    private bool rondaActiva;
    private bool hayGanador;

    private void Start()
    {
        botonPulsar.interactable = false;
        rondaText.text = "Esperando a que el anfitrión empiece una ronda";
    }

    private void Update()
    {
        botonEmpezar.interactable =
            PhotonNetwork.InRoom && PhotonNetwork.IsMasterClient;
    }

    // El anfitrión empieza la ronda.
    public void Empezar()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        rondaActiva = true;
        hayGanador = false;

        photonView.RPC(
            nameof(RpcRondaEmpieza),
            RpcTarget.All
        );
    }

    // El anfitrión recibe los intentos.
    [PunRPC]
    private void CmdPulsar(PhotonMessageInfo info)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        if (!rondaActiva || hayGanador) return;

        hayGanador = true;
        rondaActiva = false;

        SumarPunto(info.Sender);

        photonView.RPC(
            nameof(RpcAnunciarGanador),
            RpcTarget.All,
            info.Sender.NickName
        );
    }

    // Sumar punto al jugador ganador.
    private void SumarPunto(Player jugador)
    {
        int puntos = 0;

        if (jugador.CustomProperties.TryGetValue(
            "puntos", out object valor))
        {
            puntos = (int)valor;
        }

        Hashtable propiedades = new Hashtable();
        propiedades["puntos"] = puntos + 1;

        jugador.SetCustomProperties(propiedades);
    }

    // Todos reciben el inicio de la ronda.
    [PunRPC]
    private void RpcRondaEmpieza()
    {
        rondaText.text = "¡YA! Pulsa el botón";
        botonPulsar.interactable = true;
    }

    // El jugador pulsa el botón.
    public void Pulsar()
    {
        botonPulsar.interactable = false;

        photonView.RPC(
            nameof(CmdPulsar),
            RpcTarget.MasterClient
        );
    }

    // Todos reciben el resultado.
    [PunRPC]
    private void RpcAnunciarGanador(string nombre)
    {
        rondaText.text = "Gana " + nombre;
        botonPulsar.interactable = false;
    }
}
