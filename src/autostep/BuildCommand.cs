﻿using AutoStep.Extensions;
using AutoStep.Projects;
using AutoStep.Projects.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace AutoStep.CommandLine
{
    public class BuildCommand : BuildOperationCommand<BuildOperationArgs>
    {
        public BuildCommand() 
            : base("build", "Build interactions and tests.")
        {
        }

        public override async Task<int> Execute(BuildOperationArgs args, ILoggerFactory logFactory, CancellationToken cancelToken)
        {
            var success = false;
            var logger = logFactory.CreateLogger<BuildCommand>();

            try
            {
                var projectConfig = GetConfiguration(args);

                using var extensions = await LoadExtensionsAsync(args, logFactory, projectConfig, cancelToken);

                success = await CreateAndBuildProject(args, projectConfig, logFactory, extensions, cancelToken);
            }
            catch (ProjectConfigurationException configEx)
            {
                LogConfigurationError(logger, configEx);
            }

            return success ? 0 : 1;
        }

        private async Task<bool> CreateAndBuildProject(BuildOperationArgs args, IConfiguration projectConfig, ILoggerFactory logFactory, IExtensionSet extensions, CancellationToken cancelToken)
        {
            var project = CreateProject(args, projectConfig, extensions);

            return await BuildAndWriteResultsAsync(args, project, logFactory, cancelToken);
        }
    }
}
