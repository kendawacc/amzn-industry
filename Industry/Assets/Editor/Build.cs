
using System;
using System.IO;
using System.Text;

using UnityEditor.SceneManagement;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

using UnityEngine;

public class Build : IPreprocessBuildWithReport, IPostprocessBuildWithReport
{
    private Backend Backend;
    public int callbackOrder
    { get { return 0; } }

    public void OnPreprocessBuild(
    BuildReport
    BuildReport)
    { }

    public void OnPostprocessBuild(
    BuildReport
    BuildReport)
    {
        string Path
        = BuildReport
        .summary
        .outputPath;

#if Web
        {
            Array Array
            = new string[]
            {
                "TemplateData",
                "index.html"
            };

            {
                foreach (string String
                in Array)
                {
                    StringBuilder StringBuilder
                    = new StringBuilder();

                    StringBuilder
                    .Append(Path);
                    StringBuilder
                    .Append("/");
                    StringBuilder
                    .Append(String);

                    string _String
                    = StringBuilder
                    .ToString();

                    bool Boolean
                    = Directory
                    .Exists(_String);

                    switch (Boolean)
                    {
                        case true:
                            {
                                Directory
                                .Delete
                                (_String
                                , true);
                            }
                            break;
                    }
                }
            }

            {
                foreach (string String
                in Array)
                {
                    StringBuilder StringBuilder
                    = new StringBuilder();

                    StringBuilder
                    .Append(Path);
                    StringBuilder
                    .Append("/");
                    StringBuilder
                    .Append(String);

                    string _String
                    = StringBuilder
                    .ToString();

                    bool Boolean
                    = File
                    .Exists(_String);

                    switch (Boolean)
                    {
                        case true:
                            {
                                File
                                .Delete
                                (_String);
                            }
                            break;
                    }
                }
            }
        }

        {
            StringBuilder StringBuilder
            = new StringBuilder();

            StringBuilder
            .Append(Path);
            StringBuilder
            .Append("/Build");

            string String
            = StringBuilder
            .ToString();

#if WebAuth
            Path
            += "/auth";
#elif WebAccessibility
            Path
            += "/accessibility";
#elif WebLaborAssignment
            Path
            += "/accessibility"
            + "/operations"
            + "/manage"
            + "/laborassignment";
#elif WebDwelling
            Path
            += "/accessibility"
            + "/operations"
            + "/inventory"
            + "/dwelling";
#elif WebFunction
            Path
            += "/accessibility"
            + "/operations"
            + "/inventory"
            + "/function";
#endif
            DirectoryInfo DirectoryInfo 
            = new DirectoryInfo
            (Path);

            DirectoryInfo
            .Create();

            DirectoryInfo
            .Delete(true);

            Directory
            .Move(
            String,
            Path);
        }

#elif OSes || Firmware
        FileInfo FileInfo
        = new FileInfo(Path);

        DirectoryInfo DirectoryInfo
        = FileInfo
        .Directory;

        string String
        = DirectoryInfo.FullName;

        StringBuilder StringBuilder
        = new StringBuilder(String);

#if OSes
        StringBuilder
        .Append("/OSes");
#elif Firmware
        StringBuilder
        .Append("/Firmware");
#endif

        String
        = StringBuilder
        .ToString();

        File
        .Move(
        FileInfo.FullName,
        String);

        {
            StringBuilder
            = new StringBuilder();

            StringBuilder
            .Append(Application.dataPath);
            StringBuilder
            .Append("/Editor");
            StringBuilder
            .Append("/.Build");

            StringBuilder
            .Append("/System");
            StringBuilder
            .Append("/indus");

#if OSes
            StringBuilder
            .Append("/oses");
#elif Firmware
            StringBuilder
            .Append("/firmware");
#endif

            Path
            = StringBuilder
            .ToString();

            bool Boolean
            = Directory
            .Exists(Path);

            switch (Boolean)
            {
                case true:
                    {
                        Directory
                        .Delete(
                        Path, true);
                    }
                    break;
            }

            Directory
            .CreateDirectory(Path);

            foreach (string Dir
            in Directory
            .GetDirectories(
            DirectoryInfo.FullName))
            {
                string Name
                = System.IO.Path
                .GetFileName(Dir);

                string SubDir
                = System.IO.Path.Combine(
                Path, Name);

                Directory.Move(
                Dir, SubDir);
            }

            foreach (string File
            in Directory
            .GetFiles(DirectoryInfo.FullName))
            {
                string Name
                = System.IO.Path
                .GetFileName(File);

                string Dir
                = System.IO.Path.Combine(
                Path, Name);

                System.IO.File.Move(
                File, Dir);
            }
        }

        {
            Path
            = DirectoryInfo
            .FullName;

            Directory
            .Delete(Path);
        }
#endif
    }
}

// Copyright © 2025 KenDaw.