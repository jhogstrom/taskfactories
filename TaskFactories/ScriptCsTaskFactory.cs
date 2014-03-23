/* Written by Jesper Hogstrom */

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Build.Framework;

namespace MsBuild.ScriptCs
{
    public class ScriptCsTaskFactory : ITaskFactory
    {
        private IDictionary<string, TaskPropertyInfo> _parameterGroup;
        private string _taskBody;

        public bool Initialize(string taskName, IDictionary<string, TaskPropertyInfo> parameterGroup, string taskBody, IBuildEngine taskFactoryLoggingHost)
        {
            _taskBody = taskBody;
            _parameterGroup = parameterGroup;
            return true;
        }

        public TaskPropertyInfo[] GetTaskParameters()
        {
            return _parameterGroup.Values.ToArray();
        }

        public ITask CreateTask(IBuildEngine taskFactoryLoggingHost)
        {
            return new ScriptCsTask(_taskBody);
        }

        public void CleanupTask(ITask task)
        {
            // Intentionally left empty
        }

        public string FactoryName
        {
            get { return GetType().Name; }
        }

        public Type TaskType
        {
            get { return GetType(); }
        }
    }
}