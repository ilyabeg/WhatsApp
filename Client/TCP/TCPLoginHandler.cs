using Client.Client_Related;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Client.TCP
{
    internal class TCPLoginHandler
    {

        /// <summary>
        /// UDP Listener that listens out for remote clients only to block them from using a taken username.
        /// </summary>
        public static void LoginListener(UdpClient loginClient)
        {
            try
            {
                IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0); // listen to any remote user
                while (true)
                {
                    byte[] recievedBytes = loginClient.Receive(ref remoteEndPoint);
                    // block user from taking already existing name 
                    if (CheckTakenUsernameSignal(recievedBytes))
                        UsernameAuthorizer.FreeUsername = false;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"[SYSTEM] Error! Connection to Network lost due to: {e.Message}");
            }
        }

        private static bool CheckTakenUsernameSignal(byte[] recievedBytes)
        {
            string recievedMessage = Encoding.UTF8.GetString(recievedBytes);
            return recievedMessage.Equals("$USERNAME_IS_TAKEN$");
        }
    }
}
