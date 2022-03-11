﻿using FS.TimeTracking.Shared.Extensions;
using System;
using System.IO;

namespace FS.TimeTracking.Abstractions.Models.Configuration;

/// <summary>
/// Global application configuration
/// </summary>
public class TimeTrackingConfiguration
{
    /// <summary>
    /// The configuration section bind to.
    /// </summary>
    internal const string CONFIGURATION_SECTION = "TimeTracking";

    /// <summary>
    /// Pathname of the web user interface folder.
    /// </summary>
    internal const string WEB_UI_FOLDER = "webui";

    /// <summary>
    /// Pathname of the configuration folder.
    /// </summary>
    internal const string CONFIG_FOLDER = "config";

    /// <summary>
    /// Pathname of folder containing translation files.
    /// </summary>
    internal const string TRANSLATION_FOLDER = "translations";

    /// <summary>
    /// Name of the configuration file.
    /// </summary>
    /// <autogeneratedoc />
    internal const string CONFIG_BASE_NAME = "FS.TimeTracking.config";

    /// <summary>
    /// The nlog configuration file.
    /// </summary>
    /// <autogeneratedoc />
    internal const string NLOG_CONFIGURATION_FILE = CONFIG_BASE_NAME + ".nlog";

    /// <summary>
    /// Full pathname of the executable file.
    /// </summary>
    /// <autogeneratedoc />
    internal static readonly string ExecutablePath = AssemblyExtensions.GetProgramDirectory();

    /// <summary>
    /// Path to the content root.
    /// </summary>
    /// <autogeneratedoc />
    internal static readonly string PathToContentRoot = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development"
        ? Directory.GetCurrentDirectory()
        : ExecutablePath;

    /// <summary>
    /// Database specific configuration.
    /// </summary>
    public DatabaseConfiguration Database { get; set; } = new();
}