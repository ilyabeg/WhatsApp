using System.Collections.Concurrent;
using System.Net;
using System.Text;
using System.Text.Json;

namespace Client
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
            DataPacket packet = new DataPacket();

            string reciever = InputReciecer();
            string message = InputMessage();

            packet.Reciever = reciever;
            packet.Message = message;

            return packet;
        }

        private static string InputReciecer()
        {
            Console.WriteLine("Please insert the user you'd like to message:");
            string reciever = Console.ReadLine().Trim();

            //if (!users.ContainsKey(reciever))
            //    throw new Exception($"User {reciever} does not exist.");

            return reciever;
        }

        private static string InputMessage()
        {
            Console.WriteLine("Please insert the message you'd like to send:");
            string message = Console.ReadLine().Trim();

            if (string.IsNullOrWhiteSpace(message))
                throw new Exception("Invalid message input.");

            return message;
        }
    }
}
