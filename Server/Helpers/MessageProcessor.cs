
namespace Server.Helpers
{
    internal class MessageProcessor
    {
        public static string GetRecieverID(string msg)
        {
            int start = msg.IndexOf('@');
            int end = msg.IndexOf(' ');

            if (start != 0 || end == -1) throw new Exception();

            string id = msg.Substring(start + 1, end - 1);

            if (id == null || id.IsWhiteSpace()) throw new Exception();

            return id;
        }

        public static string GetActualMsg(string msg)
        {
            int start = msg.IndexOf(' ');

            if (start == -1) throw new Exception();

            string message = msg.Substring(start + 1);

            if (message == null || message.IsWhiteSpace()) throw new Exception();

            return message;
        }
    }
}
