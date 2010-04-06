﻿using System;
using System.Collections.Generic;
using System.IO;
using Syncless.CompareAndSync.CompareObject;
using Syncless.Core;
using Syncless.Filters;
using Syncless.Logging;

namespace Syncless.CompareAndSync.Manual.Visitor
{
    public class BuilderVisitor : IVisitor
    {
        #region IVisitor Members

        private readonly List<Filter> _filter;
        private readonly FilterChain _filterChain;
        private readonly List<string> _typeConflicts;

        public BuilderVisitor(List<Filter> filter, List<string> typeConflicts)
        {
            _filter = filter;
            _filterChain = new FilterChain();
            _typeConflicts = typeConflicts;
        }

        public void Visit(FileCompareObject file, int numOfPaths) { /* Do nothing. */ }

        public void Visit(FolderCompareObject folder, int numOfPaths)
        {
            RootCompareObject root = folder as RootCompareObject;

            for (int index = 0; index < numOfPaths; index++)
            {
                string path = root == null ? Path.Combine(folder.GetSmartParentPath(index), folder.Name) : root.Paths[index];

                DirectoryInfo f = new DirectoryInfo(path);

                if (f.Exists)
                {
                    DirectoryInfo[] infos = f.GetDirectories();

                    foreach (DirectoryInfo info in infos)
                    {
                        if (_filterChain.ApplyFilter(_filter, info.FullName))
                        {
                            BaseCompareObject o = folder.GetChild(info.Name);
                            FolderCompareObject fco;
                            bool conflict = false;

                            if (o == null)
                                fco = new FolderCompareObject(info.Name, numOfPaths, folder);
                            else
                            {
                                try
                                {
                                    fco = (FolderCompareObject)o;
                                }
                                catch (InvalidCastException)
                                {
                                    _typeConflicts.Add(info.FullName);
                                    folder.RemoveChild(info.Name); //Remove file object
                                    fco = new FolderCompareObject(info.Name, numOfPaths, folder);
                                    conflict = true;
                                    ServiceLocator.GetLogger(ServiceLocator.USER_LOG).Write(new LogData(LogEventType.FSCHANGE_CONFLICT, "Conflicted file detected " + info.FullName));
                                }
                            }

                            fco.CreationTime[index] = info.CreationTime.Ticks;
                            fco.Exists[index] = true;

                            if (o == null || conflict)
                                folder.AddChild(fco);
                        }
                    }

                    FileInfo[] fileInfos = f.GetFiles();
                    foreach (FileInfo info in fileInfos)
                    {
                        if (_filterChain.ApplyFilter(_filter, info.FullName))
                        {
                            BaseCompareObject o = folder.GetChild(info.Name);
                            FileCompareObject fco = null;
                            bool conflict = false;

                            if (o == null)
                                fco = new FileCompareObject(info.Name, numOfPaths, folder);
                            else
                            {
                                try
                                {
                                    fco = (FileCompareObject)o;
                                }
                                catch (InvalidCastException)
                                {
                                    _typeConflicts.Add(info.FullName);
                                    conflict = true;
                                    ServiceLocator.GetLogger(ServiceLocator.USER_LOG).Write(new LogData(LogEventType.FSCHANGE_CONFLICT, "Conflicted file detected " + info.FullName));
                                }
                            }

                            if (!conflict)
                            {
                                fco.CreationTime[index] = info.CreationTime.Ticks;
                                fco.LastWriteTime[index] = info.LastWriteTime.Ticks;
                                fco.Length[index] = info.Length;
                                fco.Exists[index] = true;

                                if (o == null)
                                    folder.AddChild(fco);
                            }
                        }
                    }
                }
            }
        }

        public void Visit(RootCompareObject root)
        {
            Visit(root, root.Paths.Length);
        }

        #endregion
    }
}