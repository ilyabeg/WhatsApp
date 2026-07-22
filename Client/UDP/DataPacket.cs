using Client.Clients;
using System.Net;
using System.Text;
using System.Text.Json;

namespace Client.UDP
{
    internal class DataPacket
    {
        public string? Author { get; set; } = null;
        public string? Message { get; set; } = null;
        public string? Reciever { get; set; } = null;

        public static DataPacket TransferData(byte[] bytes)
        {
            string str = Encoding.UTF8.GetString(bytes);
            return JsonSerializer.Deserialize<DataPacket>(str);
        }

        public void ProcessDataPacket(byte[] recievedBytes)
        {
            DataPacket recievedPacket = TransferData(recievedBytes);
        }
    }
}
