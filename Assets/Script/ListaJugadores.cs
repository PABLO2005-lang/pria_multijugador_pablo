
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using Hashtable = ExitGames.Client.Photon.Hashtable;

// Muestra quién está en la sala, sus puntos
// y avisa de las entradas y salidas.
public class ListaJugadores : MonoBehaviourPunCallbacks
{
    [SerializeField] private TMP_Text jugadoresText;
    [SerializeField] private TMP_Text avisoText;

    // A4: referencia al chat.
    [SerializeField] private Chat chat;

    // A5: referencia al Toggle.
    [SerializeField] private Toggle listoToggle;

    public override void OnJoinedRoom()
    {
        avisoText.text = "Has entrado como " + PhotonNetwork.NickName;

        AvisarMensajeSistema(
            PhotonNetwork.NickName + " ha entrado en la sala"
        );

        // Comenzar sin estar listo.
        if (listoToggle != null)
        {
            listoToggle.SetIsOnWithoutNotify(false);
            ActualizarTextoEstadoListo(false);
        }

        CambiarListo(false);

        Refrescar();
    }

    public override void OnPlayerEnteredRoom(Player jugador)
    {
        avisoText.text = jugador.NickName + " ha entrado";

        AvisarMensajeSistema(
            jugador.NickName + " ha entrado en la sala"
        );

        Refrescar();
    }

    public override void OnPlayerLeftRoom(Player jugador)
    {
        avisoText.text = jugador.NickName + " se ha ido";

        AvisarMensajeSistema(
            jugador.NickName + " ha salido de la sala"
        );

        Refrescar();
    }

    public override void OnMasterClientSwitched(Player nuevoAnfitrion)
    {
        avisoText.text = nuevoAnfitrion.NickName + " es el nuevo anfitrión";

        AvisarMensajeSistema(
            nuevoAnfitrion.NickName + " es el nuevo anfitrión"
        );

        Refrescar();
    }

    // A4: avisos del sistema en el chat.
    private void AvisarMensajeSistema(string mensaje)
    {
        if (chat != null)
        {
            chat.MostrarMensajeSistema(mensaje);
        }
        else
        {
            Debug.LogWarning("A4: falta enlazar el Chat en el Inspector.");
        }
    }

    // A5: cambiar el estado de listo.
    public void CambiarListo(bool listo)
    {
        Debug.Log("Toggle cambiado: " + listo);

        // Actualizar el texto visual.
        ActualizarTextoEstadoListo(listo);

        if (!PhotonNetwork.InRoom) return;

        Hashtable propiedades = new Hashtable();
        propiedades["listo"] = listo;

        // Enviar el estado actualizado a Photon.
        PhotonNetwork.LocalPlayer.SetCustomProperties(propiedades);
    }

    // Actualizar el texto del Toggle.
    private void ActualizarTextoEstadoListo(bool listo)
    {
        if (listoToggle == null) return;

        string texto = listo ? "LISTO" : "NO LISTO";

        TMP_Text textoTMP =
            listoToggle.GetComponentInChildren<TMP_Text>(true);

        if (textoTMP != null)
        {
            textoTMP.text = texto;
            return;
        }

        Text textoUI =
            listoToggle.GetComponentInChildren<Text>(true);

        if (textoUI != null)
        {
            textoUI.text = texto;
        }
    }

    // Actualizar cuando Photon recibe cambios.
    public override void OnPlayerPropertiesUpdate(
        Player jugador,
        Hashtable propiedades)
    {
        if (propiedades.ContainsKey("listo"))
        {
            Refrescar();
        }
        else if (propiedades.ContainsKey("puntos"))
        {
            Refrescar();
        }
    }

    private void Refrescar()
    {
        if (jugadoresText == null) return;

        string texto = "";

        foreach (Player jugador in PhotonNetwork.PlayerList)
        {
            texto += Describir(jugador) + "\n";
        }

        jugadoresText.text = texto;
    }

    private string Describir(Player jugador)
    {
        string linea = jugador.NickName;

        if (jugador.IsMasterClient)
            linea += "  [anfitrión]";

        if (jugador.IsLocal)
            linea += "  (tú)";

        // A5: mostrar el estado.
        bool listo = false;

        if (jugador.CustomProperties.TryGetValue(
            "listo", out object estadoListo))
        {
            if (estadoListo is bool)
                listo = (bool)estadoListo;
        }

        linea += listo ? "  · LISTO" : "  · NO LISTO";

        // G08: mostrar los puntos.
        if (jugador.CustomProperties.TryGetValue(
            "puntos", out object puntos))
        {
            linea += "  · " + (int)puntos + " pts";
        }

        return linea;
    }
}
