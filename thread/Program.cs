using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WebSocketSharp;

namespace thread
{
    class Program
    {
        public const int MAX_Thread = 400;
        static void Main(string[] args)
        {
            Thread[] t_array = new Thread[MAX_Thread];
            for(int i=0;i< MAX_Thread; i++)
            {
                t_array[i] = new Thread(new ParameterizedThreadStart(TestMethod));
                t_array[i].IsBackground = true;
                t_array[i].Start(i.ToString());
            }
            //Thread t1 = new Thread(new ThreadStart(TestMethod));
            //Thread t2 = new Thread(new ParameterizedThreadStart(TestMethod));
            //t1.IsBackground = true;
            //t2.IsBackground = true;
            //t1.Start();
            //t2.Start("hello");
            Console.ReadKey();
        }
        public static void TestMethod()
        {
            while (true)
            {
                Console.WriteLine("不带参数的线程函数");
                Thread.Sleep(1000);
            }
        }

        public static void TestMethod(object data)
        {
            string datastr = data as string;

            Thread[] ct_array = new Thread[4];
            for (int i = 0; i < 4; i++)
            {
                ct_array[i] = new Thread(new ParameterizedThreadStart(childThread));
                ct_array[i].IsBackground = true;
                ct_array[i].Start(datastr+"_"+i.ToString());
            }
            

          
        }

        public static void childThread(object data)
        {
            string datastr = data as string;
            WebSocket ws = new WebSocket("ws://localhost:9501");


            ws.OnMessage += (sender, e) =>
            {
                Console.WriteLine("Laputa says: " + e.Data);
            };
            ws.Connect();
            while (true)
            {
                ws.Send("BALUS" + datastr);
                Thread.Sleep(1000);
            }
        }
            
    }
}
