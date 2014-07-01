using System;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var localEP = new IPEndPoint(IPAddress.Any, 1900);
            var multicastEP = new IPEndPoint(IPAddress.Parse("239.255.255.250"), 1900);
            var udpsocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            udpsocket.Bind(localEP);
            udpsocket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, new MulticastOption(multicastEP.Address, IPAddress.Any));
            
            string searchstr = "M-SEARCH * HTTP/1.1\r\n" +
                               "HOST: 239.255.255.250:1900\r\n" +
                               "MAN:\"ssdp:discover\"\r\n" +
                               "MX:1\r\n" +
                               "ST:urn:schemas-sony-com:service:ScalarWebAPI:1\r\n";

            udpsocket.SendTo(Encoding.UTF8.GetBytes(searchstr), SocketFlags.None, multicastEP);

            byte[] recbuff = new byte[1024];
            int recbytes = 0;
            bool loop = true;
            while (loop)
            {
                if (udpsocket.Available > 0)
                {
                    recbytes = udpsocket.Receive(recbuff, SocketFlags.None);
                    if (recbytes > 0)
                    {
                        string str = Encoding.UTF8.GetString(recbuff, 0, recbytes).ToString();
                        if (MessageBox.Show(str, "抜ける？", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                        {
                            loop = false;
                            udpsocket.Close();
                        }
                    }
                }
            }

        }

        private void button2_Click(object sender, EventArgs e)
        {
            Task t = new Task(DoPost);
            t.Start();
        }

        static  async void DoPost()
        {
            string jsonparams = "{\"method\": \"actTakePicture\"," +
                                "\"params\": []," +
                                "\"id\": 1," +
                                "\"version\": \"1.0\"}";

            string url = "http://10.0.0.1:10000/sony/camera";

            var httpclient = new HttpClient();
            var jsoncontent = new StringContent(jsonparams, Encoding.UTF8, "application/json");

            httpclient.MaxResponseContentBufferSize = int.MaxValue;
            
            var response = await httpclient.PostAsync(url, jsoncontent);
            String text = await response.Content.ReadAsStringAsync();
        }


        private void button3_Click(object sender, EventArgs e)
        {
            Task t = new Task(DoPost2);
            t.Start();
        }

        static async void DoPost2()
        {
            string jsonparams = "{\"method\": \"getEvent\"," +
                                "\"params\": [true]," +
                                "\"id\": 1," +
                                "\"version\": \"1.0\"}";

            string url = "http://10.0.0.1:10000/sony/camera";



            while (true)
            {
                var httpclient = new HttpClient();
                var jsoncontent = new StringContent(jsonparams, Encoding.UTF8, "application/json");

                httpclient.MaxResponseContentBufferSize = int.MaxValue;
                var response = await httpclient.PostAsync(url, jsoncontent);

                String text = await response.Content.ReadAsStringAsync();

                httpclient.Dispose();
                httpclient = null;
                jsoncontent.Dispose();
                jsoncontent = null;
            }

        }
    }
}
