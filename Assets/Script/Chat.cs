
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;

// Chat de la sala. Necesita un PhotonView.
public class Chat : MonoBehaviourPun
{
    [SerializeField] private TMP_InputField mensajeInput;
    [SerializeField] private TMP_Text chatText;

    private const int MaxLineas = 8;
    private readonly List<string> lineas = new List<string>();

    // Botón Enviar
    public void Enviar()
    {
        string texto = mensajeInput.text.Trim();

        if (texto.Length == 0 || !PhotonNetwork.InRoom)
            return;

        // Enviar el mensaje a todos los jugadores.
        photonView.RPC(
            nameof(RecibirMensaje),
            RpcTarget.All,
            texto
        );

        mensajeInput.text = "";
        mensajeInput.ActivateInputField();
    }

    // Método que ejecutarán todos los jugadores.
    [PunRPC]
    private void RecibirMensaje(string texto, PhotonMessageInfo info)
    {
        Escribir(info.Sender.NickName + ": " + texto);
    }

    private void Escribir(string linea)
    {
        lineas.Add(linea);

        if (lineas.Count > MaxLineas)
            lineas.RemoveAt(0);

        chatText.text = string.Join("\n", lineas);
    }
}
