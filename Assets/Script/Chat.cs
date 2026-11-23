using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using ExitGames.Client.Photon;
using TMPro;

// Chat de la sala.
public class Chat : MonoBehaviourPun
{
    [SerializeField] private TMP_InputField mensajeInput;
    [SerializeField] private TMP_Text chatText;
    [SerializeField] private Toggle importanteToggle;

    private const int MaxLineas = 8;
    private readonly List<string> lineas = new List<string>();

    // Registrar el tipo de dato en Photon.
    private void Awake()
    {
        PhotonPeer.RegisterType(
            typeof(MensajeChat),
            255,
            MensajeChat.Serializar,
            MensajeChat.Deserializar
        );
    }

    // Botón Enviar.
    public void Enviar()
    {
        string texto = mensajeInput.text.Trim();

        if (texto.Length == 0 || !PhotonNetwork.InRoom)
            return;

        // Crear el mensaje.
        MensajeChat mensaje = new MensajeChat();

        mensaje.Texto = texto;
        mensaje.Hora = DateTime.Now.ToString("HH:mm");
        mensaje.Importante = importanteToggle.isOn;

        // Enviar a todos los jugadores.
        photonView.RPC(
            nameof(RecibirMensaje),
            RpcTarget.All,
            mensaje
        );

        mensajeInput.text = "";
        mensajeInput.ActivateInputField();
    }

    // Recibir el mensaje de Photon.
    [PunRPC]
    private void RecibirMensaje(
        MensajeChat mensaje,
        PhotonMessageInfo info)
    {
        string linea = "[" + mensaje.Hora + "] "
            + info.Sender.NickName + ": "
            + mensaje.Texto;

        if (mensaje.Importante)
        {
            linea = "<b>" + linea + "</b>";
        }

        Escribir(linea);
    }

    // A4: mostrar mensajes del sistema en el chat.
    public void MostrarMensajeSistema(string mensaje)
    {
        string linea = "<color=#00BFFF><b>[SISTEMA]</b> "
            + mensaje + "</color>";

        Escribir(linea);
    }

    // Añadir mensajes al chat.
    private void Escribir(string linea)
    {
        lineas.Add(linea);

        if (lineas.Count > MaxLineas)
            lineas.RemoveAt(0);

        chatText.text = string.Join("\n", lineas);
    }
}