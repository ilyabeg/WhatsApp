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

        public static DataPacket CreateNew()
        {
            try
            {
                DataPacket packet = new DataPacket();

                string reciever = InputReciecer();
                string message = InputMessage();

                packet.Reciever = reciever;
                packet.Message = message;

                return packet;
            }
            catch (Exception e)
            {
                Console.WriteLine($"[SYSTEM] Error! Couldn't create data packet due to {e.Message}");
                return null;
            }
        }

        private static string InputReciecer()
        {
            Console.WriteLine("[SYSTEM] Please insert the destination you'd like to message:");
            string reciever = Console.ReadLine().Trim();
            return reciever;
        }

        private static string InputMessage()
        {
            Console.WriteLine("[SYSTEM] Please insert the message you'd like to send:");
            string message = Console.ReadLine().Trim();

            if (string.IsNullOrWhiteSpace(message))
                throw new Exception("[SYSTEM] Invalid message input.");

            return message;
        }

        public static void ProcessDataPacket(byte[] recievedBytes)
        {
            DataPacket recievedPacket = TransferData(recievedBytes);
            Printer.PrintDataPacket(recievedPacket);
        }
    }
}
