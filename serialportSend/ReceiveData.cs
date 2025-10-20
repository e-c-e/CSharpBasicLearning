using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ProgressBar;

namespace serialportSend
{
    internal class ReceiveData
    {
        RichTextBox richTextBox;
        SerialPort serialPort;
        public ReceiveData(RichTextBox RichTextBox)
        {
            richTextBox = RichTextBox;
            serialPort = new SerialPort()
            {
                PortName = "COM4",
                BaudRate = 115200,
                StopBits = StopBits.One,
                Parity = Parity.None,
                DataBits = 8,
            };

            serialPort.DataReceived += SerialPort_DataReceived;
        }

        /// <summary>
        /// 开启串口
        /// </summary>
        public bool Start()
        {
            parseThread = new Thread(parseData);
            parseThread.IsBackground = true;
            parseThread.Start();

            if (!serialPort.IsOpen)
            {
                serialPort.Open();
                serialPort.DiscardInBuffer();
                return true;
            }
            else
            {
                return false;
            }
        }

        bool running = true;
        ConcurrentQueue<byte[]> AllReceivedByteGroup = new ConcurrentQueue<byte[]>();
        private Thread parseThread;
        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort serialPort = (SerialPort)sender;
            int byteToread = serialPort.BytesToRead;
            byte[] buffer = new byte[byteToread];
            serialPort.Read(buffer, 0, byteToread);

            AllReceivedByteGroup.Enqueue(buffer);

            //string temp = Encoding.GetEncoding("GB2312").GetString(buffer);
            //if (richTextBox.InvokeRequired)
            //{
            //    richTextBox.BeginInvoke(new Action(() =>
            //    {
            //        richTextBox.AppendText(DateTime.Now.ToString() + temp + "\n");
            //    }));
            //}
        }

        private List<byte> buffer = new List<byte>();
        public void parseData()
        {
            while (running)
            {
                if (AllReceivedByteGroup.TryDequeue(out byte[] ByteGroup))
                {
                    lock (buffer) // 此处锁的是全局缓存，防止多线程冲突
                    {
                        buffer.AddRange(ByteGroup);
                        Parse_buffer(); // 尝试解析帧
                    }
                }
                else
                {
                    Task.Delay(5).Wait();// 无数据可读时，稍作等待，避免死循环占用CPU
                }
            }
        }

        public void Parse_buffer()
        {
            for (int i = 0; i < buffer.Count(); i++)
            {
                if (buffer.Count() - i >= 187)
                {
                    if (buffer[i] == (byte)'\r' && buffer[i + 1] == (byte)'\n' && buffer[i + 186] == 0x37)
                    {
                        List<byte> listTemp = new List<byte>();
                        listTemp.AddRange(buffer.GetRange(i, i + 186));
                        buffer.RemoveRange(i, i + 186);

                        string temp = Encoding.GetEncoding("GB2312").GetString(listTemp.ToArray());
                        if (richTextBox.InvokeRequired)
                        {
                            richTextBox.BeginInvoke(new Action(() =>
                            {
                                richTextBox.AppendText(DateTime.Now.ToString() + temp + "\n");
                            }));
                        }
                    }
                }
                else
                {
                    break;
                }
            }
        }
    }
}
