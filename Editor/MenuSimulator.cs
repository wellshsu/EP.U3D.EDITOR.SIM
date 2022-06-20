//---------------------------------------------------------------------//
//                    GNU GENERAL PUBLIC LICENSE                       //
//                       Version 2, June 1991                          //
//                                                                     //
// Copyright (C) Wells Hsu, wellshsu@outlook.com, All rights reserved. //
// Everyone is permitted to copy and distribute verbatim copies        //
// of this license document, but changing it is not allowed.           //
//                  SEE LICENSE.md FOR MORE DETAILS.                   //
//---------------------------------------------------------------------//
using UnityEditor;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using System;
using EP.U3D.EDITOR.BASE;
using Preferences = EP.U3D.LIBRARY.BASE.Preferences;
using System.Reflection;

namespace EP.U3D.EDITOR.SIM
{
    public static class MenuSimulator
    {
        public const string RPC_IP = "127.0.0.1";
        public const int RPC_PORT = 9532;
        public const int RPC_HEAD_LENGTH = 12;

        public static Process Process;
        public static Thread Thread;

        static MenuSimulator()
        {
            EditorEvtcat.OnWillExitEvent += () =>
            {
                new Thread(() =>
                        {
                            StopSimulators();
                        }).Start();
            };
        }

        [MenuItem(Constants.MENU_SIMULATOR_RUN_1)]
        public static void Run1()
        {
            if (Preferences.Instance.SWidth > Preferences.Instance.SHeight)
            {
                RunSimulators(1, 1);
            }
            else
            {
                RunSimulators(1, 4);
            }
        }

        [MenuItem(Constants.MENU_SIMULATOR_RUN_2)]
        public static void Run2()
        {
            if (Preferences.Instance.SWidth > Preferences.Instance.SHeight)
            {
                RunSimulators(2, 1);
            }
            else
            {
                RunSimulators(2, 4);
            }
        }

        [MenuItem(Constants.MENU_SIMULATOR_RUN_3)]
        public static void Run3()
        {
            if (Preferences.Instance.SWidth > Preferences.Instance.SHeight)
            {
                RunSimulators(3, 2);
            }
            else
            {
                RunSimulators(3, 4);
            }
        }

        [MenuItem(Constants.MENU_SIMULATOR_RUN_4)]
        public static void Run4()
        {
            if (Preferences.Instance.SWidth > Preferences.Instance.SHeight)
            {
                RunSimulators(4, 2);
            }
            else
            {
                RunSimulators(4, 4);
            }
        }

        [MenuItem(Constants.MENU_SIMULATOR_STOP)]
        public static void Stop()
        {
            Helper.OnlyWindows(() =>
            {
                if (Thread != null && Thread.IsAlive)
                {
                    Thread.Abort();
                }
                Thread = new Thread(() =>
                {
                    StopSimulators();
                });
                Thread.Start();
            });
        }

        public static void RunSimulators(int count, int columnLimit)
        {
            Helper.OnlyWindows(() =>
           {
               var pkg = Helper.FindPackage(Assembly.GetExecutingAssembly());
               if (File.Exists(Constants.SIMULATOR_EXE) == false)
               {
                   EditorUtility.DisplayDialog("Warning", "Simulator doesn't exist.", "OK");
                   return;
               }
               else
               {
                   if (Thread != null && Thread.IsAlive)
                   {
                       Thread.Abort();
                   }
                   Thread = new Thread(() =>
                     {
                         StopSimulators();

                         Process = new Process();
                         Process.StartInfo.FileName = pkg.resolvedPath + "/Editor/Libs/arrange.exe";
                         Process.StartInfo.Arguments = string.Format("{0} {1} {2} {3} {4}", Constants.SIMULATOR_EXE, count, columnLimit, Preferences.Instance.SHeight, Preferences.Instance.SWidth);
                         Process.StartInfo.WorkingDirectory = Path.GetDirectoryName(Process.StartInfo.FileName);
                         Process.Start();
                     });
                   Thread.Start();
               }
           });
        }

        public static void StopSimulators()
        {
            SendEvtToDeamon(1001);
        }

        public static void SendEvtToDeamon(int id)
        {
            try
            {
                IPAddress[] addrs = Dns.GetHostAddresses(RPC_IP);
                if (addrs.Length == 0) throw new Exception("none addr for " + RPC_IP);
                IPAddress addr = addrs[0];
                IPEndPoint rep = new IPEndPoint(addr, RPC_PORT);
                Socket socket = new Socket(addr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                socket.Connect(rep);

                MemoryStream ms = new MemoryStream(RPC_HEAD_LENGTH);
                BinaryWriter bw = new BinaryWriter(ms);
                bw.Write(RPC_PORT);// 消息标识
                bw.Write(RPC_HEAD_LENGTH);// 消息长度
                bw.Write(id); // 消息ID
                bw.Write(0);// 消息内容长度
                byte[] buffer = ms.ToArray();
                socket.Send(buffer);
                socket.Receive(buffer);
            }
            catch { }
        }
    }
}
