﻿using System.IO;
using Shell32;

namespace SynclessUI.Helper
{
    public static class FileHelper
    {
        private const string SYNCLESS = ".syncless";
        private const string ARCHIVE = "_synclessArchive";
        private const string CONFLICT = "_synclessConflict";

        public static bool IsFile(string path)
        {
            try
            {
                var fi = new FileInfo(path);
                if (fi.Exists)
                    return true;

                return false;
            }
            catch
            {
                return false;
            }
        }

        // Credits of http://www.saunalahti.fi/janij/blog/2006-12.html

        public static string GetShortcutTargetFile(string shortcutFilename)
        {
            string extension = Path.GetExtension(shortcutFilename);

            if (extension.ToLower() == ".lnk")
            {
                string pathOnly = Path.GetDirectoryName(shortcutFilename);
                string filenameOnly = Path.GetFileName(shortcutFilename);

                Shell shell = new ShellClass();
                Folder folder = shell.NameSpace(pathOnly);
                FolderItem folderItem = folder.ParseName(filenameOnly);
                if (folderItem != null)
                {
                    var link =
                        (ShellLinkObject) folderItem.GetLink;
                    return link.Path;
                }
            }

            return null; // not found
        }

        public static bool IsSynclessFolder(string path)
        {
            string[] tokens = path.Split(new char[] { '\\' });
            foreach (string token in tokens)
            {
                if (token.ToLower().Equals(SYNCLESS) || token.ToLower().Equals(ARCHIVE) || token.ToLower().Equals(CONFLICT))
                {
                    return true;
                }
            }
            return false;
        }
    }
}