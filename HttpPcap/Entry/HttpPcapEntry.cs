using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amber.Kit.HttpPcap.WinPcap;
using Amber.Kit.HttpPcap.HttpBusiness;
using Amber.Kit.HttpPcap.RawSocket;
using Amber.Kit.HttpPcap.Common;

namespace Amber.Kit.HttpPcap
{
    /// <summary>
    /// HttpPcap抓包的入口功能类,可以在这里启动和停止抓包,同时也需要在这里设置事件回调. <para/>
    /// 本程序有多种工作方式,请参考<see cref="Amber.Kit.HttpPcap.HttpPcapConfig.pcapMode">HttpPcapConfig.pcapMode</see>.<para/>
    /// </summary>
    public class HttpPcapEntry
    {
        private HttpPcapConfig httpPcapConfig { get; set; }
        private HttpBusinessPoller httpBusinessPoller { get; set; }
        private AsyncObjectBase packetPoller { get; set; }
        private RawSocketPacketPoller rawSocketPoller { get; set; }
        private bool alreadyStarted { get; set; }

        /// <summary>
        /// onHttpRequestEvent事件的代理.<para/>
        /// </summary>
        /// <param name="sender">
        /// 本类的对象.
        /// </param>
        /// <param name="args">
        /// 包含了回调内容的事件参数.
        /// </param>
        public delegate void HttpRequestEventHandler(object sender, HttpRequest args);

        /// <summary>
        /// 如果需要监听单个的HTTP请求包,请在这里设置事件监听,就算没有对应返回的请求包也会在这里给出.<para/>
        /// </summary>
        public event HttpRequestEventHandler onHttpRequestEvent = delegate { };


        /// <summary>
        /// onHttpResponseEvent事件的代理.<para/>
        /// </summary>
        /// <param name="sender">
        /// 本类的对象.
        /// </param>
        /// <param name="args">
        /// 包含了回调内容的事件参数.
        /// </param>
        public delegate void HttpResponseEventHandler(object sender, HttpResponse args);

        /// <summary>
        /// 如果需要监听单个的HTTP回应包,请在这里设置事件监听.<para/>
        /// </summary>
        public event HttpResponseEventHandler onHttpResponseEvent = delegate { };

        /// <summary>
        /// onHttpTransactionEvent事件的代理.<para/>
        /// </summary>
        /// <param name="sender">
        /// 本类的对象.
        /// </param>
        /// <param name="args">
        /// 包含了回调内容的事件参数.
        /// </param>
        public delegate void HttpTransactionEventHandler(object sender, HttpTransaction args);

        /// <summary>
        /// 如果需要监听一个包含完整请求和回应的HTTP事务,请在这里设置事件监听,不完整的事务将不会给出.<para/>
        /// </summary>
        public event HttpTransactionEventHandler onHttpTransactionEvent = delegate { };

        /// <summary>
        /// onHttpPcapErrorEvent事件的代理.<para/>
        /// </summary>
        /// <param name="sender">
        /// 本类的对象.
        /// </param>
        /// <param name="args">
        /// 包含了回调内容的事件参数.
        /// </param>
        public delegate void HttpPcapErrorEventHandler(object sender, HttpPcapError args);

        /// <summary>
        /// 如果需要监听所有错误,请在这里设置事件监听,一般情况下,错误将不会以异常抛出.<para/>
        /// </summary>
        public event HttpPcapErrorEventHandler onHttpPcapErrorEvent = delegate { };

        /// <summary>
        /// 在这里实例化HttpPcap抓包的入口功能类,实例化时必须提供一个配置对象.<para/>
        /// 如果配置格式有误将抛出异常. <para/>
        /// </summary>
        /// <param name="httpPcapConfig">
        /// 配置对象.
        /// </param>
        public HttpPcapEntry(HttpPcapConfig httpPcapConfig)
        {
            httpPcapConfig.validate();
            this.httpPcapConfig = httpPcapConfig;
            httpBusinessPoller = new HttpBusinessPoller(httpPcapConfig);
            httpBusinessPoller.onRequest = this.onRequest;
            httpBusinessPoller.onResponse = this.onResponse;
            httpBusinessPoller.onTransaction = this.onTransaction;
            httpBusinessPoller.onError = this.onError;

            switch(httpPcapConfig.pcapMode)
            {
                case "winpcap":
                {
                    packetPoller = new PcapPacketPoller(httpPcapConfig.pcapIpAddress, this.onPacket);
                    break;
                }
                case "rawsocket":
                {
                    packetPoller = new RawSocketPacketPoller(httpPcapConfig.pcapIpAddress, this.onPacket);
                    break;
                }
            }
            packetPoller.onError = this.onError;

            alreadyStarted = false;
        }

        /// <summary>
        /// 启动Http抓包.<para/>
        /// </summary>
        /// <remarks>
        /// 启动Http抓包将会使用额外的两个线程.
        /// </remarks>
        public void start()
        {
            if(!alreadyStarted)
            {
                alreadyStarted = true;
                httpBusinessPoller.start();
                packetPoller.start();
            }
        }

        /// <summary>
        /// 停止Http抓包.<para/>
        /// </summary>
        public void stop()
        {
            httpBusinessPoller.stop();
            packetPoller.stop();
            alreadyStarted = false;   
        }

        /// <summary>
        /// 程序并不清楚是否每一个HTTP请求都能得到回应.<para/>
        /// 由于时间紧迫,现在每一个请求都会一直等待回应.因此随着运行时间变长,未回应的请求会持续堆积在内存中.<para/>
        /// 本方法是一个临时的解决方案,清除所有正在等待回应的HTTP请求.<para/>
        /// 清除后如果回应真的到来,将被忽略,也不会在onHttpPcapResponseEvent中给出.<para/>
        /// 本块功能需要更多的时间设计完善.<para/>
        /// </summary>
        public void clearUnresponsedRequest()
        {
            httpBusinessPoller.ignoreUnresponsedRequest();
        }

        private void onPacket(Descriptor descriptor)
        {
            // null as rawsocket close
            if (descriptor == null) return;
            if (descriptor.des[9] != 0x06) return;
            int srcport = BytesHelper.bytes2ushort(descriptor.des, 20, true);
            int desport = BytesHelper.bytes2ushort(descriptor.des, 22, true);
            if (srcport == desport) return;
            if (httpPcapConfig.isRequest(srcport, desport) ||
                httpPcapConfig.isResponse(srcport, desport))
            {
                byte[] ipPacket = new byte[descriptor.desLength];
                Array.Copy(descriptor.des, 0, ipPacket, 0, ipPacket.Length);
                httpBusinessPoller.postRequest(new DescriptorReference(ipPacket, ipPacket.Length));
            }
        }

        private void onRequest(HttpRequest httpRequest)
        {
            onHttpRequestEvent(this, httpRequest);
        }

        private void onResponse(HttpResponse httpResponse)
        {
            onHttpResponseEvent(this, httpResponse);
        }

        private void onTransaction(HttpTransaction httpTransaction)
        {
            onHttpTransactionEvent(this, httpTransaction);
        }

        private void onError(string message)
        {
            stop();
            onHttpPcapErrorEvent(this, new HttpPcapError(message));
        }
    }
}
