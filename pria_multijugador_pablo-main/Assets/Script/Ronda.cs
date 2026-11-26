
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using TMPro;
using Hashtable = ExitGames.Client.Photon.Hashtable;

// Minijuego de reflejos.
// El cliente predice su tiempo y el anfitrión decide el ganador.

public class Ronda : MonoBehaviourPun, IOnEventCallback
{
    [SerializeField] private Button botonEmpezar;
    [SerializeField] private Button botonPulsar;
    [SerializeField] private TMP_Text rondaText;
    [SerializeField] private TMP_Text latenciaText;

    // Evento de inicio.
    private const byte EventoInicioRonda = 1;

    // Estado del anfitrión.
    private bool rondaActiva;
    private bool hayGanador;
    private int numeroRonda;

    // Estado de la ronda.
    private double horaYa;
    private bool esperandoYa;

    // Modelo cliente G10.
    private bool pendiente;
    private int miPrediccion;
    private float proximaMedida;

    // Registrar eventos.
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

        // Comprobar el reloj del servidor.
        if (esperandoYa && PhotonNetwork.Time >= horaYa)
        {
            esperandoYa = false;
            rondaText.text = "¡YA! Pulsa el botón";
        }

        // Medir el ping cada medio segundo.
        if (Time.time >= proximaMedida)
        {
            proximaMedida = Time.time + 0.5f;

            if (PhotonNetwork.InRoom)
            {
                latenciaText.text =
                    "Ping: " + PhotonNetwork.GetPing() + " ms";
            }
        }
    }

    // ---------------- ANFITRIÓN ----------------

    public void Empezar()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        if (!PhotonNetwork.InRoom) return;

        numeroRonda++;

        rondaActiva = true;
        hayGanador = false;

        horaYa =
            PhotonNetwork.Time + Random.Range(2f, 5f);

        object[] datos = { numeroRonda, horaYa };

        RaiseEventOptions opciones =
            new RaiseEventOptions
            {
                Receivers = ReceiverGroup.All
            };

        PhotonNetwork.RaiseEvent(
            EventoInicioRonda,
            datos,
            opciones,
            SendOptions.SendReliable
        );
    }

    // Cliente -> anfitrión.
    [PunRPC]
    private void CmdPulsar(PhotonMessageInfo info)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        if (!rondaActiva || hayGanador) return;

        // Detectar falsa salida.
        if (info.SentServerTime < horaYa)
        {
            photonView.RPC(
                nameof(RpcFalsaSalida),
                info.Sender
            );

            return;
        }

        hayGanador = true;
        rondaActiva = false;

        SumarPunto(info.Sender);

        int milisegundos =
            (int)((info.SentServerTime - horaYa) * 1000);

        // Anunciar el ganador a todos.
        photonView.RPC(
            nameof(RpcAnunciarGanador),
            RpcTarget.All,
            info.Sender.ActorNumber,
            info.Sender.NickName,
            milisegundos
        );
    }

    // Sumar punto al ganador.
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

    public void OnEvent(EventData evento)
    {
        if (evento.Code != EventoInicioRonda) return;

        // Comprobar que lo envía el anfitrión actual.
        if (PhotonNetwork.MasterClient == null ||
            evento.Sender != PhotonNetwork.MasterClient.ActorNumber)
            return;

        object[] datos = (object[])evento.CustomData;

        int numero = (int)datos[0];

        horaYa = (double)datos[1];

        esperandoYa = true;
        pendiente = false;

        botonPulsar.interactable = true;

        rondaText.text =
            "Ronda " + numero + ": preparados...";
    }

    // Botón ¡PULSA!
    public void Pulsar()
    {
        if (!botonPulsar.interactable) return;
        if (!PhotonNetwork.InRoom) return;

        botonPulsar.interactable = false;

        // PREDICCIÓN: calcular inmediatamente el tiempo.
        pendiente = true;

        miPrediccion =
            (int)((PhotonNetwork.Time - horaYa) * 1000);

        if (miPrediccion < 0)
        {
            rondaText.text =
                "Parece una falsa salida... (pendiente de confirmar)";
        }
        else
        {
            rondaText.text =
                "Has pulsado en " + miPrediccion +
                " ms. Esperando al anfitrión...";
        }

        // Enviar el comando al final.
        photonView.RPC(
            nameof(CmdPulsar),
            RpcTarget.MasterClient
        );
    }

    // Falsa salida.
    [PunRPC]
    private void RpcFalsaSalida()
    {
        pendiente = false;

        rondaText.text =
            "Confirmado: ¡falsa salida! Has pulsado antes de tiempo";
    }

    // RECONCILIACIÓN: el anfitrión confirma el resultado.
    [PunRPC]
    private void RpcAnunciarGanador(
        int actorGanador,
        string nombre,
        int milisegundos)
    {
        esperandoYa = false;

        botonPulsar.interactable = false;

        if (actorGanador ==
            PhotonNetwork.LocalPlayer.ActorNumber)
        {
            rondaText.text =
                "Confirmado: ¡has ganado! (" +
                milisegundos + " ms)";
        }
        else if (pendiente)
        {
            rondaText.text =
                "Pulsaste en " + miPrediccion +
                " ms, pero " + nombre +
                " fue más rápido (" +
                milisegundos + " ms)";
        }
        else
        {
            rondaText.text =
                "Gana " + nombre +
                " en " + milisegundos + " ms";
        }

        pendiente = false;
    }
}
