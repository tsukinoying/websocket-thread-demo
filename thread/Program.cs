using Newtonsoft.Json.Linq;
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
        public const int MAX_Thread = 2;
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
            string tablename = Guid.NewGuid().ToString("N");
    
            for (int i = 0; i < 4; i++)
            {
                ct_array[i] = new Thread(new ParameterizedThreadStart(childThread));
                ct_array[i].IsBackground = true;
                string[] info = new string[4];
                info[0] = i.ToString();
                info[1] = tablename;
                ct_array[i].Start(info);
            }
            

          
        }

        public static void childThread(object data)
        {
            string[] info = data as string[];
            
            string position = info[0];
            string table = info[1];
            string user = info[1] + "_"+info[0];
            string v;
            Console.WriteLine(user);
            WebSocket ws = new WebSocket("ws://172.20.206.171:9502");
            ws.Connect();
            ws.Send("{\"action\":\"1000\",\"message\":{\"user\":\""+user+"\"}}");
            
           // 
            //{ "action":"1001","message":{ "user":"1","table":"1","position":"1","go":"0"} }
            //{ "action":"1002","message":{ "user":"4","table":"1","go":"2","position":"4"} }
            ws.OnMessage += (sender, e) =>
            {
                
                JObject acto = JObject.Parse(e.Data);
                if (acto["notice"].ToString() == "1000")
                {
                    //服务器登陆成功
                    ws.Send("{\"action\":\"1001\",\"message\":{ \"user\":\"" + user + "\",\"table\":\"" + table + "\",\"position\":\"" + position + "\",\"go\":\"1\"} }");
                    
                }

                if (acto["notice"].ToString() == "1001")
                {

                    ws.Send("{\"action\":\"1002\",\"message\":{ \"user\":\"" + user + "\",\"table\":\"" + table + "\",\"position\":\"" + position + "\",\"go\":\"2\"} }");
                   
                }
                if (acto["notice"].ToString() == "1003")
                {
                    JObject o = new JObject();
                    o["table"] = table;

                    ws.Send(actionStr("1003", o));
                   
                }
                //收到骰子
                if (acto["notice"].ToString() == "1004")
                {

                    JArray a = JArray.Parse(acto["message"]["dice"].ToString());
                    //{"notice":"1004","message":{"dice":[4,5]}}
                    
                    JObject o = new JObject();
                    o["user"] = user;

                    ws.Send(actionStr("1004", o));
                    
                }
                //摸初始13张
                if (acto["notice"].ToString() == "1005")
                {
                    JArray a = JArray.Parse(acto["message"]["my"].ToString());
                    
                    //通知一下服务器准备就绪
                    JObject o = new JObject();
                    JArray nil = new JArray();
                    o["chi"] = nil;
                    o["peng"] = nil;
                    o["gang"] = nil;
                    o["hu"] = "0";
                    o["pass"] = "1";
                    ws.Send(actionStr("1007", o));
                    
                }
                //摸一张牌
                if (acto["notice"].ToString() == "1006")
                {
                    JObject o = new JObject();
                    
                    if(position=="3")
                    {
                        
                        o["hu"] = "1";
                        ws.Send(actionStr("1010", o));
                    }
                    else
                    {
                        o["user"] = user;
                        o["position"] = position;
                        o["pai"] = "11";
                        ws.Send(actionStr("1006", o));
                    }
                    
                }
                //对别人打出的牌进行操作
                if (acto["notice"].ToString() == "1008")
                {
                    v = acto["message"]["v"].ToString();

                }
                //操作
                if (acto["notice"].ToString() == "1009")
                {
                    JObject jo = JObject.Parse(acto["message"].ToString());
                    if (acto["message"]["move"].ToString() == position)
                    {
                        JObject o = new JObject();
                        o["user"] = user;
                        o["gettype"] = "0";
                        ws.Send(actionStr("1005", o));

                    }
                }
                Console.WriteLine(user + ": server says: " + e.Data);
            };
            
            //while (true)
            //{
            //    ws.Send("BALUS" + datastr);
            //    Thread.Sleep(1000);
            //}
        }
        static string actionStr(string action, JObject o)
        {
            JObject op = new JObject();
            op["action"] = action;
            op["message"] = o;
            return op.ToString();

        }

    }
}
