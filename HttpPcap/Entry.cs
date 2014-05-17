using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Net.Sockets;
using System.Threading;

using Cabinet.Utility;
using System.Text.RegularExpressions;
using System.Net;
using Amber.Kit.HttpPcap.WinPcap;
using Amber.Kit.HttpPcap.CommonObject;
using Amber.Kit.HttpPcap.HttpBusiness;

namespace Amber.Kit.HttpPcap
{


    public class Entry
    {
        
        HttpBusinessPoller httpbusiness;
        private delegate void RecApacket(httpsession osession);
        private delegate void updatemsg(string msg);
        private updatemsg myupdatemsg;
        private RecApacket Myrecvie;

        private Socket mainSocket;                          //The socket which captures all incoming packets

        private bool bContinueCapturing = false;            //A flag to check if packets are to be captured or not
        const int SIO_RCVALL = unchecked((int)0x98000001);
        private delegate void updae_state_del(int id, int code, double times);
        private updae_state_del myupdatestatehandle;

        System.IO.StreamWriter debugsw = System.IO.File.AppendText(System.DateTime.Now.ToString("yyyyMMdd") + "_debug_.txt");


        private int maxqps = 1;
        private int qpscount = 0;
        private long totalcount = 0;



       
        #region 更新UI
        private void update_statecode_instance(int id, int code, double times)
        {
            Logger.debug("update id = {0}, code = {1}, times= {2}",
                id, code, times);
        }

        private void updatemsginstance(string msg)
        {
            Logger.debug("message = {0}", msg);
        }

        private void debugmsg(string msg)
        {
            Logger.debug("debug = {0}", msg);

        }
        private void UpdateRecPacket(httpsession osession)
        {
            Logger.debug("packet {0},{1},{2},{3},{4},{5}",
                osession.id, osession.ack, osession.method, 
                osession.url, Encoding.ASCII.GetString(osession.sendraw.ToArray()), osession.statucode);
        }

        List<PcapNetworkInterface> devlist { get; set; }
        private void Getsetting()//加载网卡设置
        {
            try
            {
                PcapNetworkInterfacePool pool = new PcapNetworkInterfacePool();
                pool.findAllInterfaces();
                devlist = pool.interfaceList;
            }
            catch (Exception ex)
            {
                Logger.error("不能获取网卡，请检查是否安装winpcap且已管理员身份运行！: {0}",ex.Message);
            }

        }

        #endregion
        public void InitUiParms()
        {
            if (CommonConfig.isusepcap)
            {
                Getsetting();
            }
            else
            {
                string strIP = null;
                IPHostEntry HosyEntry = Dns.GetHostEntry((Dns.GetHostName()));
                if (HosyEntry.AddressList.Length > 0)
                {
                    foreach (IPAddress ip in HosyEntry.AddressList)
                    {
                        if (ip.IsIPv6LinkLocal || ip.IsIPv6Multicast || ip.IsIPv6SiteLocal || ip.IsIPv6SiteLocal)
                            continue;
                        strIP = ip.ToString();
                    }
                }
            }


            Myrecvie = new RecApacket(UpdateRecPacket);
            myupdatemsg = new updatemsg(updatemsginstance);
            myupdatestatehandle = new updae_state_del(update_statecode_instance);


        }

        WinPcap.PcapNetworkInterface usedev = null;
        PcapPacketPoller poller = null;
        private void MonitorData_Pcap()
        {
            poller = new PcapPacketPoller(this.ReceivePacket_pcap);
            poller.networkInterfaceName = usedev.name;
            poller.onError = this.onError;
            poller.start();
        }
        private void onError(string message)
        {
            Logger.error(message);
        }

        private void ReceivePacket_pcap(PcapPacketHeader p, byte[] s)
        {

      
            if (s[23] != 0x06) return;
            if (s[12] != 0x08 && s[13] != 0x00) return;

            if (p.caplen <= 60) return;
            int offset = 34;
            int srcport = PkFunction.Get2Bytes(s, ref offset, 0);
            int desport = PkFunction.Get2Bytes(s, ref offset, 0);

            bool isok = false;
            if (CommonConfig.iswhiteport(desport))
            {
                isok = true;
                if (s[47] == 0x18) qpscount++;
            }
            else if (CommonConfig.iswhiteport(srcport))
            {

                isok = true;
            }
            if (!isok) return;



            byte[] t = new byte[p.caplen - 14];
            Array.Copy(s, 14, t, 0, t.Length);


            httpbusiness.postRequest(new DescriptorReference(t, t.Length));
        }




        byte[] byteData = new byte[4096];
        private void MonitorData_raw()//监控线程
        {



            while (bContinueCapturing)
            {


                // byteData.Initialize();
                byte[] buf = new byte[4096 * 2];
                int size = mainSocket.Receive(buf, 0, buf.Length, SocketFlags.None);


                //   if (size <= 40) continue;
                if (buf[9] != 0x06) continue;
                int offset = 20;
                int srcport = PkFunction.Get2Bytes(buf, ref offset, 0);
                int desport = PkFunction.Get2Bytes(buf, ref offset, 0);

                bool isok = false;
                if (CommonConfig.iswhiteport(desport))
                {
                   
                    isok = true;
                    if (buf[33] == 0x18) qpscount++;
                }
                else if (CommonConfig.iswhiteport(srcport))
                {
                 
                    isok = true;
                }
                if (!isok) continue;

                byte[] t = new byte[size];
                Array.Copy(buf, 0, t, 0, t.Length);

                httpbusiness.postRequest(new DescriptorReference(t, t.Length));

            }



        }

        string localip = string.Empty;
        
        

        System.IO.StreamWriter sw = System.IO.File.AppendText(System.DateTime.Now.ToString("yyyyMMdd_") + "log.txt");
        private void log(string msg)
        {

            sw.WriteLine(System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss\t") + msg);
            sw.Flush();
        }
  



        
        Thread main_raw;
        Thread main_pcap;
        public void button1_Click(bool isStart, string domain, string port, string localaddress)
        {

            try
            {
                CommonConfig.filtedomain = domain;
                
                

                if (isStart)
                {

                    string[] portstring = port.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    CommonConfig.ports = new int[portstring.Length];
                    for (int i = 0; i < portstring.Length; i++)
                    {
                        CommonConfig.ports[i] = Convert.ToInt32(portstring[i]);
                    }

                    if (!CommonConfig.isusepcap)
                    {
                        localip = localaddress;
                        bContinueCapturing = true;
                        mainSocket = new Socket(AddressFamily.InterNetwork, SocketType.Raw, ProtocolType.IP);
                        mainSocket.Bind(new IPEndPoint(IPAddress.Parse(localip), 0));
                        mainSocket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.HeaderIncluded, true);
                        byte[] IN = new byte[4] { 1, 0, 0, 0 };
                        byte[] OUT = new byte[4];
                        mainSocket.IOControl(SIO_RCVALL, IN, OUT);
                        main_raw = new Thread(new ThreadStart(MonitorData_raw));
                        main_raw.Priority = ThreadPriority.Highest;
                        main_raw.Start();
                    }
                    else
                    {

                        usedev = devlist.ElementAt<WinPcap.PcapNetworkInterface>(CommonConfig.interfaceSelected);
                        bContinueCapturing = true;
                        main_pcap = new Thread(new ThreadStart(MonitorData_Pcap));
                        main_pcap.Priority = ThreadPriority.Highest;
                        main_pcap.Start();
                    }
                    httpbusiness = new HttpBusinessPoller();
                    httpbusiness.Myrecvie = this.UpdateRecPacket;
                    httpbusiness.start();
                }
                else
                {

                    if (poller != null) poller.stop();
                    if (httpbusiness != null) httpbusiness.stop();
                    bContinueCapturing = false;
                    Thread.Sleep(1100);


                }
            }
            catch (Exception ex)
            {
                Logger.error(ex.Message);
            }

        }



        private bool isimage(string head)
        {
            if (head.ToLower().IndexOf("image") > 0)

                return true;

            return false;
        }
        private string getencompress(string head)
        {
            if (head.ToLower().IndexOf("gzip") > 0)

                return "gzip";




            if (head.ToLower().IndexOf("deflate") > 0)

                return "deflate";

            return "";
        }
        Regex regexencode=new Regex("charset=([\\w|-]+)",RegexOptions.IgnoreCase);
        private string  getencfromhead(string head)
        {
          Match mc=  regexencode.Match(head);
            if (mc.Success)
            {
                  return  mc.Groups[1].Value;
            }
           return string.Empty;
        }


        public Encoding GetResponseEncoding(HttpWebResponse websp)
        {
            Encoding encoding = Encoding.Default;
            if (websp.CharacterSet != null)
            {
                try
                {
                    encoding = Encoding.GetEncoding(websp.CharacterSet);
                }
                catch
                {
                }
            }
            else
            {
                if (websp.Headers["Content-Type"] != null)
                {
                    if (websp.Headers["Content-Type"].IndexOf("charset") >= 0)
                    {

                        string[] a = websp.Headers["Content-Type"].Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

                        for (int i = 0; i < a.Length; i++)
                        {
                            if (a[i].IndexOf("charset") >= 0)
                            {
                                encoding = Encoding.GetEncoding(a[i].Substring(a[i].IndexOf("charset") + 8));
                                break;
                            }
                        }
                    }
                }
            }
            return encoding;
        }
 

    

    }
}
