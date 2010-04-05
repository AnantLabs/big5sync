﻿using System;
using System.IO;
using System.Xml;
using Syncless.CompareAndSync.Enum;
using Syncless.CompareAndSync.XMLWriteObject;

namespace Syncless.CompareAndSync.Seamless
{
    public class SeamlessXMLHelper
    {
        private static long dateTime = DateTime.Now.Ticks;

        #region Main Method

        public static void UpdateXML(BaseXMLWriteObject xmlWriteList)
        {
            if (xmlWriteList is XMLWriteFolderObject)
                HandleFolder(xmlWriteList);
            else
                HandleFile(xmlWriteList);
        }

        #endregion

        #region File Operations

        private static void HandleFile(BaseXMLWriteObject xmlWriteList)
        {
            switch (xmlWriteList.ChangeType)
            {
                case MetaChangeType.New:
                    CreateFile((XMLWriteFileObject)xmlWriteList);
                    break;
                case MetaChangeType.Delete:
                    DeleteFile((XMLWriteFileObject)xmlWriteList);
                    break;
                case MetaChangeType.Rename:
                    RenameFile((XMLWriteFileObject)xmlWriteList);
                    break;
                case MetaChangeType.Update:
                    UpdateFile((XMLWriteFileObject)xmlWriteList);
                    break;
            }
        }

        private static void CreateFile(XMLWriteFileObject xmlWriteObj)
        {
            XmlDocument xmlDoc = new XmlDocument();
            string xmlFilePath = Path.Combine(xmlWriteObj.FullPath, CommonXMLConstants.MetadataPath);
            CommonMethods.CreateFileIfNotExist(xmlWriteObj.FullPath);
            CommonMethods.LoadXML(ref xmlDoc, xmlFilePath);

            CommonMethods.DoFileCleanUp(xmlDoc, xmlWriteObj.Name);
            XmlText nameText = xmlDoc.CreateTextNode(xmlWriteObj.Name);
            XmlText hashText = xmlDoc.CreateTextNode(xmlWriteObj.Hash);
            XmlText sizeText = xmlDoc.CreateTextNode(xmlWriteObj.Size.ToString());
            XmlText createdTimeText = xmlDoc.CreateTextNode(xmlWriteObj.CreationTime.ToString());
            XmlText lastModifiedText = xmlDoc.CreateTextNode(xmlWriteObj.LastModified.ToString());

            XmlElement nameElement = xmlDoc.CreateElement(CommonXMLConstants.NodeName);
            XmlElement hashElement = xmlDoc.CreateElement(CommonXMLConstants.NodeHash);
            XmlElement sizeElement = xmlDoc.CreateElement(CommonXMLConstants.NodeSize);
            XmlElement createdTimeElement = xmlDoc.CreateElement(CommonXMLConstants.NodeLastCreated);
            XmlElement lastModifiedElement = xmlDoc.CreateElement(CommonXMLConstants.NodeLastModified);
            XmlElement fileElement = xmlDoc.CreateElement(CommonXMLConstants.NodeFile);

            nameElement.AppendChild(nameText);
            hashElement.AppendChild(hashText);
            sizeElement.AppendChild(sizeText);
            createdTimeElement.AppendChild(createdTimeText);
            lastModifiedElement.AppendChild(lastModifiedText);

            fileElement.AppendChild(nameElement);
            fileElement.AppendChild(sizeElement);
            fileElement.AppendChild(hashElement);
            fileElement.AppendChild(lastModifiedElement);
            fileElement.AppendChild(createdTimeElement);

            XmlNode rootNode = xmlDoc.SelectSingleNode(CommonXMLConstants.XPathExpr);
            rootNode.AppendChild(fileElement);
            CommonMethods.SaveXML(ref xmlDoc, xmlFilePath);
            DeleteFileToDoByName(xmlWriteObj);
        }

        private static void UpdateFile(XMLWriteFileObject xmlWriteObj)
        {
            XmlDocument xmlDoc = new XmlDocument();
            string xmlFilePath = Path.Combine(xmlWriteObj.FullPath, CommonXMLConstants.MetadataPath);
            CommonMethods.CreateFileIfNotExist(xmlWriteObj.FullPath);
            CommonMethods.LoadXML(ref xmlDoc, xmlFilePath);

            XmlNode node = xmlDoc.SelectSingleNode(CommonXMLConstants.XPathExpr + CommonXMLConstants.XPathFile + "[name=" + CommonMethods.ParseXPathString(xmlWriteObj.Name) + "]");
            if (node == null)
            {
                CommonMethods.SaveXML(ref xmlDoc, xmlFilePath);
                CreateFile(xmlWriteObj);
                return;
            }

            XmlNodeList childNodeList = node.ChildNodes;
            for (int i = 0; i < childNodeList.Count; i++)
            {
                XmlNode nodes = childNodeList[i];

                switch (nodes.Name)
                {
                    case CommonXMLConstants.NodeSize:
                        nodes.InnerText = xmlWriteObj.Size.ToString();
                        break;
                    case CommonXMLConstants.NodeHash:
                        nodes.InnerText = xmlWriteObj.Hash;
                        break;
                    case CommonXMLConstants.NodeName:
                        nodes.InnerText = xmlWriteObj.Name;
                        break;
                    case CommonXMLConstants.NodeLastModified:
                        nodes.InnerText = xmlWriteObj.LastModified.ToString();
                        break;
                    case CommonXMLConstants.NodeLastCreated:
                        nodes.InnerText = xmlWriteObj.CreationTime.ToString();
                        break;
                }
            }

            CommonMethods.SaveXML(ref xmlDoc, xmlFilePath);
            DeleteFileToDoByName(xmlWriteObj);
        }


        private static void RenameFile(XMLWriteFileObject xmlWriteObj)
        {
            XmlDocument xmlDoc = new XmlDocument();
            XmlNode tempNode = null;
            string xmlFilePath = Path.Combine(xmlWriteObj.FullPath, CommonXMLConstants.MetadataPath);
            CommonMethods.CreateFileIfNotExist(xmlWriteObj.FullPath);
            CommonMethods.LoadXML(ref xmlDoc, xmlFilePath);

            XmlNode node = xmlDoc.SelectSingleNode(CommonXMLConstants.XPathExpr + CommonXMLConstants.XPathFile + "[name=" + CommonMethods.ParseXPathString(xmlWriteObj.Name) + "]");
            if (node == null)
                return;
            tempNode = node.Clone();
            node.FirstChild.InnerText = xmlWriteObj.NewName;
            CommonMethods.SaveXML(ref xmlDoc, xmlFilePath);
            GenerateFileToDo(xmlWriteObj, tempNode);
        }

        private static void DeleteFile(XMLWriteFileObject xmlWriteObj)
        {
            XmlDocument xmlDoc = new XmlDocument();
            XmlNode tempNode = null;
            string xmlFilePath = Path.Combine(xmlWriteObj.FullPath, CommonXMLConstants.MetadataPath);
            if (File.Exists(xmlFilePath))
            {
                CommonMethods.LoadXML(ref xmlDoc, xmlFilePath);
                XmlNode node = xmlDoc.SelectSingleNode(CommonXMLConstants.XPathExpr + CommonXMLConstants.XPathFile + "[name=" + CommonMethods.ParseXPathString(xmlWriteObj.Name) + "]");
                if (node == null)
                    return;
                tempNode = node.Clone();
                node.ParentNode.RemoveChild(node);
                CommonMethods.SaveXML(ref xmlDoc, xmlFilePath);
            }

            GenerateFileToDo(xmlWriteObj, tempNode);
        }

        #endregion

        #region Folder Operations

        private static void HandleFolder(BaseXMLWriteObject xmlWriteObj)
        {
            switch (xmlWriteObj.ChangeType)
            {
                case MetaChangeType.New:
                    CreateFolder((XMLWriteFolderObject)xmlWriteObj);
                    break;
                case MetaChangeType.Rename:
                    RenameFolder((XMLWriteFolderObject)xmlWriteObj);
                    break;
                case MetaChangeType.Delete:
                    DeleteFolder((XMLWriteFolderObject)xmlWriteObj);
                    break;
            }
        }

        private static void CreateFolder(XMLWriteFolderObject xmlWriteObj)
        {
            XmlDocument xmlDoc = new XmlDocument();
            string xmlFilePath = Path.Combine(xmlWriteObj.FullPath, CommonXMLConstants.MetadataPath);
            CommonMethods.CreateFileIfNotExist(xmlWriteObj.FullPath);
            CommonMethods.LoadXML(ref xmlDoc, xmlFilePath);

            CommonMethods.DoFolderCleanUp(xmlDoc, xmlWriteObj.Name);
            XmlText nameText = xmlDoc.CreateTextNode(xmlWriteObj.Name);
            XmlElement nameOfFolder = xmlDoc.CreateElement(CommonXMLConstants.NodeName);
            XmlElement folder = xmlDoc.CreateElement(CommonXMLConstants.NodeFolder);
            nameOfFolder.AppendChild(nameText);
            folder.AppendChild(nameOfFolder);

            XmlNode rootNode = xmlDoc.SelectSingleNode(CommonXMLConstants.XPathExpr);
            rootNode.AppendChild(folder);
            CommonMethods.SaveXML(ref xmlDoc, xmlFilePath);
            DeleteFolderToDoByName(xmlWriteObj);
        }

        private static void RenameFolder(XMLWriteFolderObject xmlWriteObj)
        {
            XmlDocument xmlDoc = new XmlDocument();
            string xmlPath = Path.Combine(xmlWriteObj.FullPath, CommonXMLConstants.MetadataPath);
            CommonMethods.CreateFileIfNotExist(xmlWriteObj.FullPath);
            CommonMethods.LoadXML(ref xmlDoc, xmlPath);

            XmlNode node = xmlDoc.SelectSingleNode(CommonXMLConstants.XPathExpr + CommonXMLConstants.XPathFolder + "[name=" + CommonMethods.ParseXPathString(xmlWriteObj.Name) + "]");
            if (node == null)
                return;
            node.FirstChild.InnerText = xmlWriteObj.NewName;
            CommonMethods.SaveXML(ref xmlDoc, xmlPath);

            XmlDocument subFolderXmlDoc = new XmlDocument();
            string subFolder = Path.Combine(xmlWriteObj.FullPath, xmlWriteObj.NewName);
            string subFolderXmlPath = Path.Combine(subFolder, CommonXMLConstants.MetadataPath);
            CommonMethods.CreateFileIfNotExist(subFolder);
            CommonMethods.LoadXML(ref subFolderXmlDoc, subFolderXmlPath);

            XmlNode subFolderNode = subFolderXmlDoc.SelectSingleNode(CommonXMLConstants.XPathExpr + "/name");
            if (subFolderNode == null)
                return;
            subFolderNode.InnerText = xmlWriteObj.NewName;
            CommonMethods.SaveXML(ref subFolderXmlDoc, subFolderXmlPath);
            GenerateFolderToDo(xmlWriteObj);
        }

        private static void DeleteFolder(XMLWriteFolderObject xmlWriteObj)
        {
            string xmlFilePath = Path.Combine(xmlWriteObj.FullPath, CommonXMLConstants.MetadataPath);
            XmlDocument xmlDoc = new XmlDocument();
            if (File.Exists(xmlFilePath))
            {
                CommonMethods.LoadXML(ref xmlDoc, xmlFilePath);
                XmlNode node = xmlDoc.SelectSingleNode(CommonXMLConstants.XPathExpr + CommonXMLConstants.XPathFolder + "[name=" + CommonMethods.ParseXPathString(xmlWriteObj.Name) + "]");
                if (node == null)
                    return;
                node.ParentNode.RemoveChild(node);
                CommonMethods.SaveXML(ref xmlDoc, xmlFilePath);
            }

            GenerateFolderToDo(xmlWriteObj);
        }

        #endregion

        #region ToDo Operations

        private static void GenerateFileToDo(XMLWriteFileObject xmlWriteObj, XmlNode deletedNode)
        {
            if (deletedNode == null)
                return;
            string fullPath = xmlWriteObj.FullPath;
            XmlDocument xmlTodoDoc = new XmlDocument();
            string todoPath = Path.Combine(fullPath, CommonXMLConstants.TodoPath);
            CommonMethods.CreateToDoFile(fullPath);
            CommonMethods.LoadXML(ref xmlTodoDoc, todoPath);
            AppendActionFileToDo(xmlTodoDoc, xmlWriteObj, CommonXMLConstants.ActionDeleted, deletedNode);
            CommonMethods.SaveXML(ref xmlTodoDoc, todoPath);
        }

        private static void GenerateFolderToDo(XMLWriteFolderObject xmlWriteObj)
        {
            string parentPath = xmlWriteObj.FullPath;
            if (!Directory.Exists(parentPath))
                return;

            XmlDocument xmlTodoDoc = new XmlDocument();
            string todoPath = Path.Combine(parentPath, CommonXMLConstants.TodoPath);
            CommonMethods.CreateToDoFile(parentPath);
            CommonMethods.LoadXML(ref xmlTodoDoc, todoPath);
            AppendActionFolderToDo(xmlTodoDoc, xmlWriteObj, CommonXMLConstants.ActionDeleted);
            CommonMethods.SaveXML(ref xmlTodoDoc, todoPath);
        }

        private static void AppendActionFileToDo(XmlDocument xmlDoc, XMLWriteFileObject xmlWriteObj, string changeType, XmlNode node)
        {
            string hash = string.Empty;
            string lastModified = string.Empty;
            XmlNodeList nodeList = node.ChildNodes;
            for (int i = 0; i < nodeList.Count; i++)
            {
                XmlNode childNode = nodeList[i];
                switch (childNode.Name)
                {
                    case CommonXMLConstants.NodeHash:
                        hash = childNode.InnerText;
                        break;
                    case CommonXMLConstants.NodeLastModified:
                        lastModified = childNode.InnerText;
                        break;
                }
            }

            XmlText hashText = xmlDoc.CreateTextNode(hash);
            XmlText actionText = xmlDoc.CreateTextNode(changeType);
            XmlText lastModifiedText = xmlDoc.CreateTextNode(lastModified);
            XmlText nameText = xmlDoc.CreateTextNode(xmlWriteObj.Name);
            XmlText lastUpdatedText = xmlDoc.CreateTextNode(dateTime.ToString());

            XmlElement fileElement = xmlDoc.CreateElement(CommonXMLConstants.NodeFile);
            XmlElement nameElement = xmlDoc.CreateElement(CommonXMLConstants.NodeName);
            XmlElement hashElement = xmlDoc.CreateElement(CommonXMLConstants.NodeHash);
            XmlElement actionElement = xmlDoc.CreateElement(CommonXMLConstants.NodeAction);
            XmlElement lastModifiedElement = xmlDoc.CreateElement(CommonXMLConstants.NodeLastModified);
            XmlElement lastUpdatedElement = xmlDoc.CreateElement(CommonXMLConstants.NodeLastUpdated);

            hashElement.AppendChild(hashText);
            actionElement.AppendChild(actionText);
            lastModifiedElement.AppendChild(lastModifiedText);
            lastUpdatedElement.AppendChild(lastUpdatedText);
            nameElement.AppendChild(nameText);

            fileElement.AppendChild(nameElement);
            fileElement.AppendChild(actionElement);
            fileElement.AppendChild(hashElement);
            fileElement.AppendChild(lastModifiedElement);
            fileElement.AppendChild(lastUpdatedElement);

            XmlNode rootNode = xmlDoc.SelectSingleNode(CommonXMLConstants.XPathLastKnownState);
            rootNode.AppendChild(fileElement);
        }

        private static void DeleteFileToDoByName(XMLWriteFileObject xmlWriteObj)
        {
            string todoXmlPath = Path.Combine(xmlWriteObj.FullPath, CommonXMLConstants.TodoPath);
            if (!File.Exists(todoXmlPath))
                return;

            XmlDocument todoXmlDoc = new XmlDocument();
            CommonMethods.LoadXML(ref todoXmlDoc, todoXmlPath);
            XmlNode fileNode = todoXmlDoc.SelectSingleNode(CommonXMLConstants.XPathLastKnownState + CommonXMLConstants.XPathFile + "[name=" + CommonMethods.ParseXPathString(xmlWriteObj.Name) + "]");
            if (fileNode != null)
                fileNode.ParentNode.RemoveChild(fileNode);
            CommonMethods.SaveXML(ref todoXmlDoc, todoXmlPath);
        }

        private static void AppendActionFolderToDo(XmlDocument xmlDoc, XMLWriteFolderObject folder, string changeType)
        {
            XmlText nameText = xmlDoc.CreateTextNode(folder.Name);
            XmlText action = xmlDoc.CreateTextNode(changeType);
            XmlText lastUpdatedText = xmlDoc.CreateTextNode(dateTime.ToString());

            XmlElement folderElement = xmlDoc.CreateElement(CommonXMLConstants.NodeFolder);
            XmlElement nameElement = xmlDoc.CreateElement(CommonXMLConstants.NodeName);
            XmlElement actionElement = xmlDoc.CreateElement(CommonXMLConstants.NodeAction);
            XmlElement lastUpdatedElement = xmlDoc.CreateElement(CommonXMLConstants.NodeLastUpdated);

            nameElement.AppendChild(nameText);
            actionElement.AppendChild(action);
            lastUpdatedElement.AppendChild(lastUpdatedText);

            folderElement.AppendChild(nameElement);
            folderElement.AppendChild(actionElement);
            folderElement.AppendChild(lastUpdatedElement);
            XmlNode rootNode = xmlDoc.SelectSingleNode(CommonXMLConstants.XPathLastKnownState);
            rootNode.AppendChild(folderElement);
        }

        private static void DeleteFolderToDoByName(XMLWriteFolderObject xmlWriteObj)
        {
            string todoXmlPath = Path.Combine(xmlWriteObj.FullPath, CommonXMLConstants.TodoPath);
            if (!File.Exists(todoXmlPath))
                return;

            XmlDocument todoXmlDoc = new XmlDocument();
            CommonMethods.LoadXML(ref todoXmlDoc, todoXmlPath);
            XmlNode folderNode = todoXmlDoc.SelectSingleNode(CommonXMLConstants.XPathLastKnownState + CommonXMLConstants.XPathFolder + "[name=" + CommonMethods.ParseXPathString(xmlWriteObj.Name) + "]");
            if (folderNode != null)
                folderNode.ParentNode.RemoveChild(folderNode);
            CommonMethods.SaveXML(ref todoXmlDoc, todoXmlPath);
        }

        #endregion

    }
}