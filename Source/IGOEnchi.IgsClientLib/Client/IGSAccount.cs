using System.Xml.Serialization;

namespace IGoEnchi
{
    public class IGSAccount
    {
        public IGSAccount() : this("igs.joyjoy.net", 6969, "guest", "", false)
        {
        }

        public IGSAccount(string server, int port, string name, string password, bool savePassword)
        {
            Server = server;
            Port = port;
            Name = name;
            Password = password;
            PasswordSpecified = savePassword;
        }

        [XmlElement("server")]
        public string Server { get; set; }

        [XmlElement("port")]
        public int Port { get; set; }

        [XmlElement("name")]
        public string Name { get; set; }

        [XmlElement("password")]
        public string Password { get; set; }

        [XmlIgnore]
        public bool PasswordSpecified { get; set; }

        public static IGSAccount DefaultAccount
        {
            get { return new IGSAccount(); }
        }
    }
}