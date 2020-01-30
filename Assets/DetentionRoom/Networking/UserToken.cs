using Bolt;
using UdpKit;

namespace DetentionRoom.Networking
{
    public class UserToken : IProtocolToken
    {
        public string Username;
        public string Password;
        
        public UserToken(){}

        public UserToken(string username, string password)
        {
            Username = username;
            Password = password;
        }
        
        public void Read(UdpPacket packet)
        {
            Username = packet.ReadString();
            Password = packet.ReadString();
        }

        public void Write(UdpPacket packet)
        {
            packet.WriteString(Username);
            packet.WriteString(Password);
        }
    }
}