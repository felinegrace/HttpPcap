using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amber.Kit.HttpPcap.Common;

namespace Amber.Kit.HttpPcap
{
    /// <summary>
    /// <see cref = "Amber.Kit.HttpPcap.HttpPcapEntry">HttpPcapEntry</see>的配置,
    /// <see cref = "Amber.Kit.HttpPcap.HttpPcapEntry">HttpPcapEntry</see>实例化时必须提供一个配置.<para/>
    /// 由于时间紧迫,有些配置暂时是必须的.<para/>
    /// </summary>
    public class HttpPcapConfig
    {
        /// <summary>
        /// HttpPcap的工作方式. <para/>
        /// 此项配置是必须的. <para/>
        /// </summary>
        /// <value>
        /// 可以将模式设置为winpcap或rawsocket
        /// </value> 
        /// <example>
        /// <code> 
        /// pcapMode = "winpcap";
        /// pcapMode = "rawsocket";
        /// </code>
        /// </example>
        /// <remarks>
        /// winpcap:<para/>
        /// 此模式需要安装<see href="http://www.winpcap.org/install/default.htm">WinPcap</see>. <para/>
        /// 开启此模式将使程序工作在链路层(第二层),以获得更多的功能和更快的速度.<para/>
        /// 由于时间紧迫,此模式并没有调整到性能最优,还有许多可以优化的空间.<para/>
        /// 由于时间紧迫,本程序假设链路层仅有一层以太网封装,如果有其他链路层协议如PPPOE,则会失效,在这类情况下请使用rawsocket模式.<para/>
        /// <para/>
        /// rawsocket: <para/>
        /// 此模式需要管理员权限. <para/>
        /// 开启此模式将使程序工作在网络层(第三层). <para/>
        /// 此模式下逻辑更为简单,且不需要安装其他的驱动程序. <para/>
        /// 部分网卡带有自动计算校验和的功能,会导致无法在本模式下抓包,通过本地连接-属性-配置-高级-Checksum Offload里关闭发送时的自动校验,即可解决问题,但对全局网络性能有一定的影响.<para/>
        /// </remarks>
        public string pcapMode { get; set; }

        /// <summary>
        /// HttpPcap监听的网络地址.<para/>
        /// 由于时间紧迫,目前只能监听一个地址.<para/>
        /// 只能监听IPV4地址. <para/>
        /// </summary>
        /// <value>
        /// 地址需要设置一个IPV4地址字符串,即为点号间隔的四组0-255的数字.
        /// </value> 
        /// <example>
        /// <code> 
        /// pcapIpAddress = "192.168.1.31";
        /// </code>
        /// </example>
        public string pcapIpAddress { get; set; }

        /// <summary>
        /// HttpPcap过滤的远端域名或地址,使得程序只关注本机与此域名之间发生的HTTP会话.<para/>
        /// 由于时间紧迫,目前只能设置一个过滤地址.<para/>
        /// </summary>
        /// <value>
        /// 如需设置过滤,地址是一个IPV4地址的字符串,或者是域名字符串. <para/>
        /// 如不需要设置任何过滤,则将此项设置为空字符串(注意:不是null对象). <para/>
        /// 默认为不设置过滤.<para/>
        /// </value> 
        /// <example>
        /// <code> 
        /// remoteDomainFilter = "www.baidu.com";
        /// </code>
        /// </example>
        /// <remarks>
        /// 过滤的域名是完全匹配的,如设置为 www.baidu.com 时, news.baidu.com 上发生的HTTP会话会被忽略.
        /// </remarks>
        public string remoteDomainFilter { get; set; }

        /// <summary>
        /// HttpPcap过滤的服务端端口,使得程序只关注该端口上的HTTP会话.<para/>
        /// 由于时间紧迫,目前只能指定好HTTP的服务端与客户端,并只在客户端捕获请求,只在服务端捕获回应.<para/>
        /// 此项配置是必须的.<para/>
        /// </summary>
        /// <value>
        /// 端口是一个整数的队列. <para/>
        /// 请至少添加一个服务端端口. <para/>
        /// </value> 
        /// <example>
        /// 以json方式作示范,在C#中请使用List相关方法.
        /// <code> 
        /// serverPortsFilter = [80];
        /// </code>
        /// </example>
        /// <remarks>
        /// 服务端不一定就是远端,如本机为HTTP服务器时,本机为服务端.
        /// </remarks>
        public List<int> serverPortsFilter { get; set; }

        /// <summary>
        /// HttpPcap过滤的客户端端口,使得程序只关注该端口上的HTTP会话.<para/>
        /// 由于时间紧迫,目前只能指定好HTTP的服务端与客户端,并只在客户端捕获请求,只在服务端捕获回应.<para/>
        /// </summary>
        /// <value>
        /// 如不需要设置任何过滤,则将此队列置为空队列.(注意:不是null对象). <para/>
        /// 默认为不设置过滤. <para/>
        /// </value> 
        /// <example>
        /// 以json方式作示范,在C#中请使用List相关方法.
        /// <code> 
        /// clientPortsFilter = [19872];
        /// </code>
        /// </example>
        /// <remarks>
        /// 客户端不一定就是本程序运行端,如本机为HTTP服务器时,远端为客户端.
        /// </remarks>
        public List<int> clientPortsFilter { get; set; }

        /// <summary>
        /// 在此构造HttpPcapConfig, 构造后请逐个使用属性设置来配置. <para/>
        /// </summary>
        public HttpPcapConfig()
        {
            pcapMode = string.Empty;
            pcapIpAddress = string.Empty;
            remoteDomainFilter = string.Empty;
            serverPortsFilter = new List<int>();
            clientPortsFilter = new List<int>();
        }


        internal bool isRequest(int sourcePort, int destPort)
        {
            bool isClient = false;
            bool isServer = false;
            if (clientPortsFilter.Count == 0)
                isClient = true;
            if (clientPortsFilter.Contains<int>(sourcePort))
                isClient = true;
            if (serverPortsFilter.Count == 0)
                isServer = true;
            if (serverPortsFilter.Contains<int>(destPort))
                isServer = true;
            return isClient && isServer;
        }

        internal bool isResponse(int sourcePort, int destPort)
        {
            bool isClient = false;
            bool isServer = false;
            if (clientPortsFilter.Count == 0)
                isClient = true;
            if (clientPortsFilter.Contains<int>(destPort))
                isClient = true;
            if (serverPortsFilter.Count == 0)
                isServer = true;
            if (serverPortsFilter.Contains<int>(sourcePort))
                isServer = true;
            return isClient && isServer;
        }

        internal void validate()
        {
            
            if (pcapMode == string.Empty)
            {
                throw new PcapException("must specify a pcapMode.");
            }
            if ( !( pcapMode.Equals("winpcap") || pcapMode.Equals("rawsocket")) )
            {
                throw new PcapException("pcapMode must be winpcap or rawsocket.");
            }
            if (pcapIpAddress == string.Empty)
            {
                throw new PcapException("must specify an IpAddress.");
            }
            if (serverPortsFilter.Count == 0)
            {
                throw new PcapException("must specify one or more server ports.");
            }
            foreach(int clientPort in clientPortsFilter)
            {
                if(serverPortsFilter.Contains(clientPort))
                {
                    throw new PcapException("client ports must not be the same as server ports.");
                }
            }
        }
    }
}
