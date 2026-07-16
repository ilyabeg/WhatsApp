namespace Client.Clients
{
    internal class StringParser
    {
        public static string ParseRemoteUser(string msg)
        {
            int start = msg.IndexOf('@');
            int end = msg.IndexOf(' ');

            if (start != 0 || end == -1) throw new Exception("Invalid userID input");

            string id = msg.Substring(start + 1, end - 1);

            if (id == null || id.IsWhiteSpace()) throw new Exception("Invalid userID input");

            return id;
        }

        public static string ParseActualMsg(string msg)
        {
            int start = msg.IndexOf(' ');

            if (start == -1) throw new Exception("Invalid message input");

            string message = msg.Substring(start + 1);

            if (message == null || message.IsWhiteSpace()) throw new Exception("Invalid message input");

            return message;
        }
    }
}
