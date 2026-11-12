
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using TMPro;
using Hashtable = ExitGames.Client.Photon.Hashtable;

// Minijuego de reflejos.
// El anfitrión controla la ronda y todos utilizan
// el reloj del servidor de Photon.

public class Ronda : MonoBehaviourPun, IOnEventCallback
{
    [SerializeField] private Button botonEmpezar;
    [SerializeField] private Button botonPulsar;
    [SerializeField] private TMP_Text rondaText;

    // Código del evento de inicio.
    private const byte EventoInicioRonda = 1;

    // Estado que controla el anfitrión.
    private bool rondaActiva;
    private bool hayGanador;
    private int numeroRonda;

    // Estado compartido de la ronda.
    private double horaYa;
    private bool esperandoYa;

    // ---------------- EVENTOS ----------------

    private void OnEnable()
    {
        PhotonNetwork.AddCallbackTarget(this);
    }

    private void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    private void Start()
    {
        botonPulsar.interactable = false;

        rondaText.text =
            "Esperando a que el anfitrión empiece una ronda";
    }

    private void Update()
    {
        // Solo el anfitrión puede empezar.
        botonEmpezar.interactable =
            PhotonNetwork.InRoom &&
            PhotonNetwork.IsMasterClient;

        // Todos utilizan el reloj de Photon.
        if (esperandoYa && PhotonNetwork.Time >= horaYa)
        {
            esperandoYa = false;

            rondaText.text = "¡YA! Pulsa el botón";
        }
    }

    // ---------------- ANFITRIÓN ----------------

    // Botón Empezar ronda.
    public void Empezar()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        numeroRonda++;

        rondaActiva = true;
        hayGanador = false;

        // El inicio será entre 2 y 5 segundos después.
        horaYa = PhotonNetwork.Time + Random.Range(2f, 5f);

        // Datos que enviamos a todos.
        object[] datos = { numeroRonda, horaYa };

        RaiseEventOptions opciones =
            new RaiseEventOptions
            {
                Receivers = ReceiverGroup.All
            };

        // Evento de inicio de ronda.
        PhotonNetwork.RaiseEvent(
            EventoInicioRonda,
            datos,
            opciones,
            SendOptions.SendReliable
        );
    }

    // Cliente -> anfitrión.
    // Recibe la pulsación del jugador.
    [PunRPC]
    private void CmdPulsar(PhotonMessageInfo info)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        // Comprobar que la ronda está abierta.
        if (!rondaActiva || hayGanador) return;

        // Comprobar si ha pulsado antes de tiempo.
        if (info.SentServerTime < horaYa)
        {
            photonView.RPC(
                nameof(RpcFalsaSalida),
                info.Sender
            );

            return;
        }

        // Ya tenemos ganador.
        hayGanador = true;
        rondaActiva = false;

        // Sumar un punto al ganador.
        SumarPunto(info.Sender);

        // Calcular tiempo de reacción.
        int milisegundos =
            (int)((info.SentServerTime - horaYa) * 1000);

        // Anunciar ganador a todos.
        photonView.RPC(
            nameof(RpcAnunciarGanador),
            RpcTarget.All,
            info.Sender.NickName,
            milisegundos
        );
    }

    // Sumar puntos al jugador.
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

    // ---------------- TODOS LOS JUGADORES ----------------

    // Recibir el evento de inicio.
    public void OnEvent(EventData evento)
    {
        if (evento.Code != EventoInicioRonda) return;

        // Solo aceptar eventos enviados por el anfitrión.
        if (PhotonNetwork.MasterClient == null ||
            evento.Sender != PhotonNetwork.MasterClient.ActorNumber)
            return;

        object[] datos = (object[])evento.CustomData;

        int numero = (int)datos[0];

        horaYa = (double)datos[1];

        esperandoYa = true;

        botonPulsar.interactable = true;

        rondaText.text =
            "Ronda " + numero + ": preparados...";
    }

    // Botón ¡PULSA!
    public void Pulsar()
    {
        // Un solo intento por ronda.
        botonPulsar.interactable = false;

        // Enviar comando al anfitrión.
        photonView.RPC(
            nameof(CmdPulsar),
            RpcTarget.MasterClient
        );
    }

    // Avisar de una falsa salida.
    [PunRPC]
    private void RpcFalsaSalida()
    {
        rondaText.text =
            "¡Falsa salida! Has pulsado antes de tiempo";
    }

    // Anunciar ganador a todos.
    [PunRPC]
    private void RpcAnunciarGanador(
        string nombre,
        int milisegundos)
    {
        esperandoYa = false;

        rondaText.text =
            "Gana " + nombre + " en " +
            milisegundos + " ms";

        botonPulsar.interactable = false;
    }
}
