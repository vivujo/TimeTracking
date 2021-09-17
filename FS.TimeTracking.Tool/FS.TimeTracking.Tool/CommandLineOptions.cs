﻿using FS.TimeTracking.Shared.Models.Configuration;
using Mono.Options;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace FS.TimeTracking.Tool
{
    /// <summary>
    /// Command line options
    /// </summary>
    public class CommandLineOptions
    {
        /// <summary>
        /// Import Kimai V1 database.
        /// </summary>
        public bool ImportKimaiV1 { get; set; }

        /// <summary>
        /// The connection string of the source database.
        /// </summary>
        public string SourceConnectionString { get; set; }

        /// <summary>
        /// The type of source database.
        /// </summary>
        /// <autogeneratedoc />
        public DatabaseType SourceDatabaseType { get; set; }

        /// <summary>
        /// The prefix for table names in source database.
        /// </summary>
        public string SourceTablePrefix { get; set; }

        /// <summary>
        /// The connection string of the destination database.
        /// </summary>
        public string DestinationConnectionString { get; set; }

        /// <summary>
        /// The type of destination database.
        /// </summary>
        public DatabaseType DestinationDatabaseType { get; set; }

        /// <summary>
        /// Truncate database before import.
        /// </summary>
        public bool TruncateBeforeImport { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="CommandLineOptions"/> is parsed successfully.
        /// </summary>
        public bool Parsed { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandLineOptions"/> class.
        /// </summary>
        /// <param name="args">The command line arguments.</param>
        public CommandLineOptions(IEnumerable<string> args)
        {
            var showHelp = false;

            var optionSet = new OptionSet {
                { "import-kimai-v1", "Import Kimai V1 database", x => ImportKimaiV1 = x != null },
                { "source-connection-string=", "The connection string of the source database", x => SourceConnectionString = x },
                { "source-database-type=", "The type of source database", (DatabaseType x) => SourceDatabaseType = x },
                { "source-table-prefix=", "The prefix for table names in source database", x => SourceTablePrefix = x },
                { "destination-connection-string=", "The connection string of the destination database", x => DestinationConnectionString = x },
                { "destination-database-type=", "The type of destination database", (DatabaseType x) => DestinationDatabaseType = x },
                { "truncate-before-import", "Truncate database before import", x => TruncateBeforeImport = x != null },
                { "h|help", "Show this message and exit", x => showHelp = x != null },
            };

            try
            {
                optionSet.Parse(args);
                if (showHelp)
                    ShowHelp(optionSet);
                else
                    Parsed = true;
            }
            catch (OptionException ex)
            {
                // show some app description message
                Console.WriteLine($"Usage: {Assembly.GetExecutingAssembly().GetName()} [OPTIONS]+");
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine();

                // output the options
                Console.WriteLine("Options:");
                ShowHelp(optionSet);
            }
        }

        private static void ShowHelp(OptionSet optionSet)
            => optionSet.WriteOptionDescriptions(Console.Out);
    }
}
