﻿using System;
using System.Collections.Generic;
using System.IO;

using DrakeScript;

namespace ShikigamiEngine
{
	public static class Script
	{
		public static Context Context {get; private set;} = new Context();
		public static List<Task> Tasks = new List<Task>();



		public static void Init()
		{
			Libs.Register();
		}

		public static void ResumeTasks()
		{
			for (var i = 0; i < Tasks.Count; i++)
			{
				var task = Tasks[i];
				if (task.NextResume == Engine.Time)
				{
					if (task.Coroutine.Status == CoroutineStatus.Yielded)
						task.Resume();
					if (task.Coroutine.Status == CoroutineStatus.Stopped)
					{
						Tasks.RemoveAt(i);
						i--;
					}
				}
			}
		}

		public static Task CreateTask(Entity parent, Function function)
		{
			var task = new Task(parent, function);
			task.Resume();
			if (task.Coroutine.Status == CoroutineStatus.Yielded)
				Tasks.Add(task);
			return task;
		}

		public static Value InvokeEntityEvent(Entity ent, string name, params Value[] args)
		{
			var funcT = Script.Context.GetGlobal(name);
			Function function;
			if (funcT.Type == DrakeScript.Value.ValueType.Function)
				function = funcT.Function;
			else
				return Value.Nil;
			return function.Invoke(new Dictionary<string, Value> {{"this", Value.Create(ent)}}, args);
		}

		public static Task InvokeEntityEventTask(Entity ent, string name, params Value[] args)
		{
			var funcT = Script.Context.GetGlobal(name);
			Function function;
			if (funcT.Type == DrakeScript.Value.ValueType.Function)
				function = funcT.Function;
			else
				return null;
			var task = new Task(ent, function);
			var ret = task.Resume(args);
			if (task.Coroutine.Status == CoroutineStatus.Yielded)
				Tasks.Add(task);
			return task;
		}

        public static Dictionary<string, Function> LoadedFileFunctions = new Dictionary<string, Function>();
        public static Function LoadFile(string path)
        {
            Function ret;
            path = Path.GetFullPath(Paths.Scripts + path);
            if (LoadedFileFunctions.TryGetValue(path, out ret))
                return ret;
            ret = Context.LoadFile(path);
            LoadedFileFunctions.Add(path, ret);
            return ret;
        }
        public static Value RunFile(string path)
        {
            return LoadFile(path).Invoke();
        }
    }
}

