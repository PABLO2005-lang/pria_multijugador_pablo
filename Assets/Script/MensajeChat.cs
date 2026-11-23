
using System.IO;

// Clase que contiene los datos de un mensaje.
public class MensajeChat
{
    public string Texto;
    public string Hora;
    public bool Importante;

    // Convertir el mensaje en bytes.
    public static byte[] Serializar(object objeto)
    {
        MensajeChat m = (MensajeChat)objeto;

        using (MemoryStream memoria = new MemoryStream())
        using (BinaryWriter escritor = new BinaryWriter(memoria))
        {
            escritor.Write(m.Texto);
            escritor.Write(m.Hora);
            escritor.Write(m.Importante);

            escritor.Flush();
            return memoria.ToArray();
        }
    }

    // Reconstruir el mensaje desde los bytes.
    public static object Deserializar(byte[] datos)
    {
        using (MemoryStream memoria = new MemoryStream(datos))
        using (BinaryReader lector = new BinaryReader(memoria))
        {
            MensajeChat m = new MensajeChat();

            m.Texto = lector.ReadString();
            m.Hora = lector.ReadString();
            m.Importante = lector.ReadBoolean();

            return m;
        }
    }
}
