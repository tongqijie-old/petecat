﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using Petecat.Threading.Process;
using System;
using System.Collections.Generic;
using System.IO;

namespace Petecat.Test.Threading.Process
{
    [TestClass]
    public class ProcessObjectTest
    {
        [TestMethod]
        public void ExecuteTest()
        {
            string branchName = "project_13400";

            string localLocation = @"d:\git\shoppingservice";

            string compareTool = @"E:\Tools\BeyondCompare\BCompare.exe";

            var changeFiles = new List<ChangeFile>();

            using (var reader = new ProcessObject("git")
                .Add("diff").Add("--name-status").Add("master.." + branchName)
                .ReadStream(localLocation))
            {
                string line = null;
                while ((line = reader.ReadLine()) != null)
                {
                    var fields = line.Split('\t');
                    if (fields.Length == 2)
                    {
                        changeFiles.Add(new ChangeFile() { Action = fields[0].Trim(), Path = fields[1].Trim() });
                    }
                }
            }

            var commits = new List<Commit>();

            using (var reader = new ProcessObject("git")
                .Add("reflog").Add("show")
                .ReadStream(localLocation))
            {
                string line = null;
                while ((line = reader.ReadLine()) != null)
                {
                    commits.Add(new Commit() { Id = line.Trim().Substring(0, 7), Text = line.Trim() });
                }
            }

            if (commits.Count <= 1)
            {
                return;
            }

            var initialVersion = commits.FindLast(x => x.Text.ToUpper().Contains(branchName.ToUpper()));
            var latestVersion = commits[0];

            foreach (var changeFile in changeFiles)
            {
                var initialContent = new ProcessObject("git")
                    .Add("show").Add(initialVersion.Id + ":" + changeFile.Path)
                    .ReadString(localLocation);

                changeFile.InitialFullPath = Path.Combine("init", changeFile.Path.Replace('/', '\\'));
                WriteFile(initialContent, changeFile.InitialFullPath);

                var latestContent = new ProcessObject("git")
                    .Add("show").Add(latestVersion.Id + ":" + changeFile.Path)
                    .ReadString(localLocation);

                changeFile.LatestFullPath = Path.Combine("latest", changeFile.Path.Replace('/', '\\'));
                WriteFile(latestContent, changeFile.LatestFullPath);

                new ProcessObject(compareTool)
                    .Add(changeFile.InitialFullPath).Add(changeFile.LatestFullPath)
                    .Execute();
            }
        }

        private void WriteFile(string content, string path)
        {
            var folder = path.Substring(0, path.LastIndexOf('\\'));
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            using (var outputStream = new StreamWriter(path))
            {
                outputStream.Write(content);
            }
        }

        class ChangeFile
        {
            public string Action { get; set; }

            public string Path { get; set; }

            public string InitialFullPath { get; set; }

            public string LatestFullPath { get; set; }

        }

        class Commit
        {
            public string Id { get; set; }

            public string Text { get; set; }
        }
    }
}
