using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;

/// <summary>
/// UDP protokollaa käyttävä viesti palvelin joka toimii soketeilla.
/// palvelin tallentaa käyttäjät ja toimii viestin välittäjänä
///
/// </summary>
namespace UDPServer
{
    class Program
    {
        static void Main(string[] args)
        {
            Socket palvelin = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            IPEndPoint iep = new IPEndPoint(IPAddress.Any, 25000);
            palvelin.Bind(iep);
            byte[] rec = new byte[3000];
            int paljon = 0;
            IPEndPoint iap = new IPEndPoint(IPAddress.Any, 0);
            EndPoint senderRemote = (EndPoint)iap;
            EndPoint[] kayttajatIP = new EndPoint[20];
            int maara = 0;
            string viesti = "";
            while (true)
            {
                //viesti tulee muodossa:
                //action;username;message
                paljon = palvelin.ReceiveFrom(rec, ref senderRemote);
                viesti = System.Text.Encoding.ASCII.GetString(rec, 0, paljon);
                string[] viestisplit = viesti.Split(';',2);
                Console.WriteLine(viesti);
                //case
                switch (viestisplit[0])
                {
                    case "Register":

                        //TODO Register the client to the database 
                        string connetionString = @"Data Source=DESKTOP-MM4FH53;Initial Catalog=Users;User ID=sa;Password=fy42Klo00";
                        SqlConnection cnn = new SqlConnection(connetionString);

                        string[] userAndPass = viestisplit[1].Split(';');
                        string sql = "select count(username) from chat.users where username=@UserID";
                        SqlCommand cmd = new SqlCommand(sql, cnn);
                        SqlParameter[] param = new SqlParameter[2];
                        param[0] = new SqlParameter("@UserID", userAndPass[0]);
                        param[1] = new SqlParameter("@pwd", userAndPass[1]);
                        cmd.Parameters.Add(param[0]);
                        //cmd.Parameters.Add(param[1]);
                        cnn.Open();
                        object res = cmd.ExecuteScalar();
                        if (Convert.ToInt32(res) == 0)
                        {
                            sql = "INSERT INTO chat.users (username, pass) OUTPUT Inserted.username VALUES (@UserID, @pwd)";
                            cmd = new SqlCommand(sql, cnn);
                            param[0] = new SqlParameter("@UserID", userAndPass[0]);
                            param[1] = new SqlParameter("@pwd", userAndPass[1]);
                            cmd.Parameters.Add(param[0]);
                            cmd.Parameters.Add(param[1]);
                            res = cmd.ExecuteScalar();
                            if(Convert.ToString(res).Equals(userAndPass[0]))
                            {
                                string response = "Registeration successful";
                                byte[] resByte = System.Text.Encoding.ASCII.GetBytes(response);
                                palvelin.SendTo(resByte, senderRemote);
                                cnn.Close();
                            }
                            

                        }
                        else
                        {
                            string response = "registeration failed";
                            byte[] resByte = System.Text.Encoding.ASCII.GetBytes(response);
                            palvelin.SendTo(resByte, senderRemote);
                            cnn.Close();
                            
                        }
                        break;

                    case "Login":
                        //TODO check that the user exists
                        //and connect to the server
                        string connetionString2 = @"Data Source=DESKTOP-MM4FH53;Initial Catalog=Users;User ID=sa;Password=fy42Klo00";
                        SqlConnection cnn2 = new SqlConnection(connetionString2);

                        string[] userAndPass2 = viestisplit[1].Split(';');
                        string sql2 = "select count(username) from chat.users where username=@UserID and pass=@pwd";
                        SqlCommand cmd2 = new SqlCommand(sql2, cnn2);
                        SqlParameter[] param2 = new SqlParameter[2];
                        param2[0] = new SqlParameter("@UserID", userAndPass2[0]);
                        param2[1] = new SqlParameter("@pwd", userAndPass2[1]);
                        cmd2.Parameters.Add(param2[0]);
                        cmd2.Parameters.Add(param2[1]);
                        cnn2.Open();
                        object res2 = cmd2.ExecuteScalar();
                        if (Convert.ToInt32(res2) > 0)
                        {
                            string response = "Login Successful";
                            byte[] resByte = System.Text.Encoding.ASCII.GetBytes(response);
                            palvelin.SendTo(resByte, senderRemote);
                            cnn2.Close();
                            //Close();
                        }
                        else
                        {
                            string response = "Login failed";
                            byte[] resByte = System.Text.Encoding.ASCII.GetBytes(response);
                            palvelin.SendTo(resByte, senderRemote);
                            cnn2.Close();
                            //MessageBox.Show("Invalid login");
                        }
                        break;


                    case "Message":
                        SendMessage(viestisplit[1], maara, kayttajatIP, palvelin, senderRemote);
                        //TODO send message to other people

                        break;

                    case "Connect":
                        kayttajatIP[maara] = senderRemote;
                        maara++;

                        break;

                    default:
                        string virhe = "Something went wrong";
                        byte[] viestii = System.Text.Encoding.ASCII.GetBytes(virhe);
                        palvelin.SendTo(viestii, senderRemote);
                        break;

                }

                

            }
            
        }

        /// <summary>
        /// Sends a message to all users that have connected to the server
        /// </summary>
        /// <param name="message">message that is sent</param>
        /// <param name="lenght">amount of connected users</param>
        /// <param name="kayttajatIP">list of connected IPs</param>
        /// <param name="palvelin">socket that is used to send the message</param>
        /// <param name="senderRemote">sender of the message</param>
        public static void SendMessage(string message, int lenght, EndPoint[] kayttajatIP, Socket palvelin, EndPoint senderRemote)
        {

            Console.WriteLine(message);
            if (message.Contains(';'))
            {

                byte[] viestii = System.Text.Encoding.ASCII.GetBytes(message);
                for (int i = 0; i < lenght; i++)
                {
                    palvelin.SendTo(viestii, kayttajatIP[i]);
                }
            }
            else
            {
                string virhe = "viesti oli väärässä moudossa";
                byte[] viestii = System.Text.Encoding.ASCII.GetBytes(virhe);
                palvelin.SendTo(viestii, senderRemote);
            }
        }
    }


}
