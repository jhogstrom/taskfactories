/* Written by Jesper Hogstrom */

using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace MsBuild.ScriptCs
{
    public class ScriptCsTask : Task, IGeneratedTask
    {
        private readonly StringBuilder _commandLinePattern;
        private readonly Dictionary<string, object> _parameters = new Dictionary<string, object>();

        public ScriptCsTask(string commandLine)
        {
            _commandLinePattern = new StringBuilder(commandLine);
        }

        public override bool Execute()
        {
            StartScriptCs(ScriptCsArguments());

            return true;
        }

        public void SetPropertyValue(TaskPropertyInfo property, object value)
        {
            _parameters.Add(property.Name, value);
        }

        public object GetPropertyValue(TaskPropertyInfo property)
        {
            var propertyName = property.Name;
            Log.LogMessage(MessageImportance.Low, "Requesting property '{0}'", propertyName);

            if (_parameters.ContainsKey(propertyName))
            {
                return _parameters[propertyName];
            }

            Log.LogError("Property '{0}' not found", propertyName);
            return string.Empty;
        }

        private void StartScriptCs(string commandLine)
        {
            Log.LogMessage(MessageImportance.Low, "Calling ScriptCs with: [[{0}]]", commandLine);
            var p = new ProcessStartInfo(@"scriptcs.exe", commandLine)
            {
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,                
                CreateNoWindow = true
            };
            var process = Process.Start(p);
            process.OutputDataReceived += (sender, args) => Display(args.Data);
            process.ErrorDataReceived += (sender, args) => DisplayError(args.Data);

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            process.WaitForExit();
        }

        private void DisplayError(string data)
        {
            if (data != null)
            {
                Log.LogError(data);
            }
        }

        private string ScriptCsArguments()
        {
            var commandLine = BuildCommandLine();
            string scriptCommandLine;
            if (commandLine.Contains("--"))
            {
                scriptCommandLine = commandLine;
            }
            else
            {
                var cmdLine = commandLine.Split(new[] {' '}, 2);
                scriptCommandLine = cmdLine[0];
                if (cmdLine.Length == 2)
                {
                    scriptCommandLine += string.Format(" -- {0}", cmdLine[1]);
                }
            }
            return scriptCommandLine;
        }

        private void Display(string data)
        {
            if (data != null)
            {
                Log.LogMessage(MessageImportance.Normal, data);
            }
        }

        private string BuildCommandLine()
        {
            foreach (var parameter in _parameters)
            {
                var name = string.Format("{{{0}}}", parameter.Key);
                var value = parameter.Value.ToString();
                _commandLinePattern.Replace(name, value);
            }

            return _commandLinePattern.ToString();
        }
    }
}
