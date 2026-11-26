
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

// Una caja de la escena con PhotonView. Se pinta del color de su dueño
// y, al hacer clic en ella, pides que pase a ser tuya.
// En el PhotonView: Ownership Transfer = Takeover.
public class CajaPropiedad : MonoBehaviourPun
{
    [SerializeField] private Renderer cuerpo;
    [SerializeField] private TMP_Text duenioText;

    private void Update()
    {
        Player duenio = photonView.Owner;

        if (duenio == null)
        {
            // Objeto de escena que aún no es de nadie
            cuerpo.material.color = Color.gray;
            duenioText.text = "De la escena";
        }
        else
        {
            cuerpo.material.color = ColorDe(duenio.ActorNumber);
            duenioText.text = "Es de " + duenio.NickName
                + (photonView.IsMine ? " (tú)" : "");
        }
    }

    // Clic con el ratón sobre la caja (necesita un Collider)
    private void OnMouseDown()
    {
        if (photonView.Owner == PhotonNetwork.LocalPlayer) return;

        photonView.RequestOwnership();
    }

    // Un color distinto para cada número de jugador
    private static Color ColorDe(int numero)
    {
        Color[] colores = {
            Color.red, Color.blue, Color.green, Color.yellow,
            Color.cyan, Color.magenta, Color.white, Color.black
        };

        return colores[(numero - 1) % colores.Length];
    }
}
