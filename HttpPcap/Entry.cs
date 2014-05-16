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

namespace Amber.Kit.HttpPcap
{


    public class Entry
    {
        int rid;
        long pktcount;
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



        private Dictionary<string, List<byte>> dic_senddata = new Dictionary<string, List<byte>>();
        private Dictionary<string, httpsession> dic_ack_httpsession = new Dictionary<string, httpsession>();

       
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
            if (iswhiteport(desport))
            {
                isok = true;
                if (s[47] == 0x18) qpscount++;
            }
            else if (iswhiteport(srcport))
            {

                isok = true;
            }
            if (!isok) return;



            byte[] t = new byte[p.caplen - 14];
            Array.Copy(s, 14, t, 0, t.Length);
            pktcache.Enqueue(t);
        }



        private void ParcePkt_Cache(byte[] byteData, int nReceived)
        {


            IPHeader ipHeader = new IPHeader(byteData, nReceived);


            switch (ipHeader.ProtocolType)
            {
                case Protocol.TCP:
                    TCPHeader tcpHeader = new TCPHeader(ipHeader.Data, ipHeader.MessageLength);//Length of the data field          
                    int headlen = ipHeader.HeaderLength + tcpHeader.HeaderLength;

                    if (iswhiteport(tcpHeader.DestinationPort))
                    {
                        pktcount++;
                        if (headlen >= nReceived) return;
                        if (tcpHeader.Flags == 0x18)
                            BuildPacket(true, tcpHeader.AcknowledgementNumber, tcpHeader.SequenceNumber, byteData, headlen, nReceived);
                        else if (tcpHeader.Flags == 0x10)
                        {
                            byte[] dataArray = new byte[nReceived - headlen];
                            Array.Copy(byteData, headlen, dataArray, 0, dataArray.Length);

                            if (dic_senddata.ContainsKey(tcpHeader.AcknowledgementNumber))
                                dic_senddata[tcpHeader.AcknowledgementNumber].AddRange(dataArray);
                            else
                            {
                                List<byte> listtmp = new List<byte>();
                                listtmp.AddRange(dataArray);
                                dic_senddata.Add(tcpHeader.AcknowledgementNumber, listtmp);

                            }
                        }
                    }
                    else if (iswhiteport(tcpHeader.SourcePort))
                    {
                        pktcount++;
                        if (headlen >= nReceived) return;
                        BuildPacket(false, tcpHeader.AcknowledgementNumber, tcpHeader.SequenceNumber, byteData, headlen, nReceived);

                    }
                    break;




            }


        }





        private void OnReceive(IAsyncResult ar)
        {
            try
            {
                int nReceived = mainSocket.EndReceive(ar);

                byte[] byteData = ar.AsyncState as byte[];
                //Analyze the bytes received...

                ParcePkt_Cache(byteData, nReceived);
                byteData = new byte[4096];
                if (bContinueCapturing)
                {
                    //    Thread.Sleep(1);
                    byteData = new byte[4096];

                    //Another call to BeginReceive so that we continue to receive the incoming
                    //packets
                    mainSocket.BeginReceive(byteData, 0, byteData.Length, SocketFlags.None,
                        new AsyncCallback(OnReceive), byteData);
                }
            }

            catch (Exception ex)
            {
                Logger.error(ex.Message);
            }
        }


        private bool iswhiteport(int port)
        {

            for (int i = 0; i < ports.Length; i++)
            {
                if (port == ports[i])
                    return true;
            }
            return false;
        }

        private void ParserCacheThread()
        {
            while (bContinueCapturing)
            {
                while (pktcache.Count > 0)
                {

                    byte[] t = pktcache.Dequeue();
                    ParcePkt_Cache(t, t.Length);
                    //Application.DoEvents();
                    //   Thread.Sleep(10);

                }

                System.Threading.Thread.Sleep(1000);

            }
        }
        private Queue<byte[]> pktcache = new Queue<byte[]>();

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
                if (iswhiteport(desport))
                {
                   
                    isok = true;
                    if (buf[33] == 0x18) qpscount++;
                }
                else if (iswhiteport(srcport))
                {
                 
                    isok = true;
                }
                if (!isok) continue;

                byte[] t = new byte[size];
                Array.Copy(buf, 0, t, 0, t.Length);

                pktcache.Enqueue(t);

            }



        }

        string localip = string.Empty;
        int[] ports;
        //IPAddress ipwhite = null;

        Regex rhost = new Regex(@"\bhost:.(\S*)", RegexOptions.IgnoreCase);
        private string Gethost(string http)
        {

            Match m = rhost.Match(http);
            return m.Groups[1].Value;

        }
        System.IO.StreamWriter sw = System.IO.File.AppendText(System.DateTime.Now.ToString("yyyyMMdd_") + "log.txt");
        private void log(string msg)
        {

            sw.WriteLine(System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss\t") + msg);
            sw.Flush();
        }
        private Dictionary<string, string> dic_ack_seq = new Dictionary<string, string>();

        private List<string> list_ack = new List<string>();

        /// <summary>
        /// 组包
        /// </summary>
        /// <param name="isout"></param>
        /// <param name="ack"></param>
        /// <param name="seq"></param>
        /// <param name="PacketData"></param>
        /// <param name="start"></param>
        /// <param name="reclen"></param>
        /// 
        private void BuildPacket(bool isout, string ack, string seq, byte[] PacketData, int start, int reclen)
        {

            try
            {


                
                

                if (reclen <= start) return;
                byte[] dataArray = new byte[reclen - start];
                Array.Copy(PacketData, start, dataArray, 0, dataArray.Length);
                if (isout)//如果是请求包
                {


                    httpsession osesion = new httpsession();
                    osesion.id = rid;
                    osesion.senddtime = DateTime.Now;
                    osesion.ack = ack;
                    if (dic_senddata.ContainsKey(ack))
                    {
                        osesion.sendraw = dic_senddata[ack];
                    }
                    else
                    {
                        osesion.sendraw = new List<byte>();
                    }

                    osesion.sendraw.AddRange(dataArray);


                    string http_str = System.Text.Encoding.ASCII.GetString(osesion.sendraw.ToArray());
                    string host = Gethost(http_str);

                    if (filtedomain == "" || host.IndexOf(filtedomain) >= 0)
                    {


                        ////  this.dataGridView1.Rows.Add(vr.rid, vr.seq, vr.method, vr.url,http_str);
                        //else
                        int fltag = http_str.IndexOf("\r\n");
                        if (fltag > 0)
                        {
                            string fline = http_str.Substring(0, fltag);
                            int fblacktag = fline.IndexOf(" ");
                            if (fblacktag > 0)
                            {
                                osesion.method = fline.Substring(0, fline.IndexOf(" "));
                                int urllen = fline.LastIndexOf(" ") - fblacktag - 1;
                                if (urllen > 0)
                                    osesion.url = String.Format("http://{0}{1}", host, fline.Substring(fblacktag + 1, urllen));
                            }
                        }
                        if (!this.dic_ack_httpsession.ContainsKey(osesion.ack))
                        {
                            this.dic_ack_httpsession.Add(osesion.ack, osesion);
                            //     this.dic_rid_ack.Add(osesion.id, ack);
                            this.list_ack.Add(ack);
                        }
                        rid++;
                        Myrecvie( osesion );
                         
                        debugmsg(string.Format("[{0}]  创建 {1}", seq, osesion.url));
                        //    }                     

                    }

                }
                else//如果是返回数据包
                {

                    if (dic_ack_httpsession.ContainsKey(seq))//如果第一次匹配
                    {
                        //    log(ack + ":" + seq + " 第一次返回匹配，添加映射");
                        debugmsg(string.Format("[{0}]  开始接受 {1}：{2}", seq, ack, seq));
                        httpsession osession = dic_ack_httpsession[seq];
                        if (osession.responseraw == null) osession.responseraw = new List<byte>();
                        osession.responseraw.AddRange(dataArray);
                        osession.responoversetime = DateTime.Now;

                        string headb = System.Text.Encoding.ASCII.GetString(dataArray);
                        int flinetag = headb.IndexOf("\r\n");
                        if (flinetag > 0)
                        {
                            headb = headb.Substring(0, flinetag);
                            string[] p3 = headb.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            if (p3.Length >= 2)
                            {
                                osession.statucode = int.Parse(p3[1]);

                                myupdatestatehandle( 
                                osession.id, osession.statucode, (osession.responoversetime - osession.senddtime).TotalMilliseconds);

                                log(osession.method + "\t" + osession.url +"\t"+osession.statucode); ;
                            }
                        }
                        dic_ack_httpsession[seq] = osession;
                        if (!dic_ack_seq.ContainsKey(ack))
                        {
                            dic_ack_seq.Add(ack, seq);
                        }
                        //    if (osession.id<=40)

                    }//后面的数据包
                    else
                    {
                        if (dic_ack_seq.ContainsKey(ack))
                        {

                            httpsession osession = dic_ack_httpsession[dic_ack_seq[ack]];

                            osession.responseraw.AddRange(dataArray);
                            dic_ack_httpsession[dic_ack_seq[ack]] = osession;
                            debugmsg(string.Format("[{0}]  继续接受 {1}：{2}  总长度 {3} ", dic_ack_seq[ack], ack, seq, osession.responseraw.Count));
                        }
                        else
                        {
                            debugmsg(string.Format("[未识别]  接受 {1}：{2}", 0, ack, seq));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                debugmsg(string.Format("[异常]  {0}", ex.ToString()));
                log(ex.ToString());
            }
        }



        string filtedomain;
        Thread main_raw;
        Thread main_pcap;
        public void button1_Click(bool isStart, string domain, string port, string localaddress)
        {

            try
            {
                filtedomain = domain;
                rid = 0;
                pktcount = 0;

                if (isStart)
                {

                    dic_ack_seq.Clear();
                    dic_ack_httpsession.Clear();
                    string[] portstring = port.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    ports = new int[portstring.Length];
                    for (int i = 0; i < portstring.Length; i++)
                    {
                        ports[i] = Convert.ToInt32(portstring[i]);
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

                    Thread ppcthread = new Thread(new ThreadStart(ParserCacheThread));
                    ppcthread.IsBackground = true;
                    ppcthread.Start();
                }
                else
                {

                    if (poller != null) poller.stop();
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
