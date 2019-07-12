﻿using McMaster.Extensions.CommandLineUtils;
using System;

namespace Neo.DebugAdapter
{
    class Program
    {
        static void Main(string[] args) => CommandLineApplication.Execute<Program>(args);

        private void OnExecute(CommandLineApplication app, IConsole console)
        {
            NeoDebugAdapter adapter = new NeoDebugAdapter(Console.OpenStandardInput(), Console.OpenStandardOutput());
            adapter.Protocol.LogMessage += (sender, _args) => System.Diagnostics.Debug.WriteLine(_args.Message);
            adapter.Run();
        }
    }
}
