using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Console.Plugins;

namespace Console
{
    public static class Interface
    {
        #region Variables
        
        public static Controller Controller = Controller.Instance;
        public static List<Plugin> Plugins { get; } = PoolNew<List<Plugin>>.Get();
        
        #endregion
        
        /// <summary>
        /// Calls a specified hook on every plugin
        /// </summary>
        /// <param name="name">Hook name</param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static object Call(string name, params object[] args)
        {
            try
            {
                object result = null;
                var results = PoolNew<List<HookResult>>.Get();
                var conflicts = PoolNew<List<HookResult>>.Get();

                for (var i = 0; i < Plugins.Count; i++)
                {
                    var plugin = Plugins[i];
                    var currentResult = new HookResult(plugin, plugin.IsLoaded ? plugin.Call(name, args) : null);
                    results.Add(currentResult);
                    if (currentResult.Result != null)
                        result = currentResult.Result;
                }

                for (var i = 0; i < Plugins.Count - 1; i++)
                {
                    for (var j = i + 1; j < Plugins.Count; j++)
                    {
                        var result1 = results[i];
                        var result2 = results[j];

                        if (result1.Result == null || result2.Result == null || result1.Result.Equals(result2.Result))
                            continue;
                        
                        conflicts.Add(result1);
                        conflicts.Add(result2);
                    }
                }

                if (conflicts.Count <= 0) return result;

                Log.Warning($"Calling hook '{name}' resulted in a conflict between the following plugins: {string.Join(", ", conflicts.Distinct())}");
                return null;
            }
            catch (Exception e)
            {
                Log.Exception(e);
            }

            return null;
        }

        /// <summary>
        /// Calls a specified hook on every plugin
        /// </summary>
        /// <param name="name">Hook name</param>
        /// <param name="args"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T Call<T>(string name, params object[] args)
        {
            var result = Call(name, args);
            return result == null ? default(T) : (T) Convert.ChangeType(result, typeof(T));
        }

        /// <summary>
        /// Calls a specified hook on every plugin
        /// </summary>
        /// <param name="name">Hook name</param>
        /// <param name="args"></param>
        public static void CallHook(string name, params object[] args) => Call(name, args);

        internal static void LoadAssembly(string path)
        {
            try
            {
                var assembly = Assembly.Load(File.ReadAllBytes(path));
                var type = assembly.GetType("Console.Plugins." + Path.GetFileNameWithoutExtension(path));
                
                Plugin.CreatePlugin(type, path);
            }
            catch (Exception e)
            {
                Log.Exception(e);
            }
        }

        internal static void UnloadAssembly(string path)
        {
            var pluginsCount = Plugins.Count;
            var plugin = (Plugin) null;
            for (var i = 0; i < pluginsCount; i++)
            {
                if (Plugins[i].Path == path)
                    plugin = Plugins[i];
            }

            if (plugin == null)
                return;

            Unload(plugin.Name);
            Plugins.Remove(plugin);
        }

        public static void Load(string name)
        {
            var plugin = Plugins.Find(x => x.Name == name);
            if (plugin == null) return;
            
            plugin.IsLoaded = true;
            Log.Info($"Loaded plugin {plugin.Title} by {plugin.Author} v{plugin.Version}");
        }

        public static void Unload(string name)
        {
            var plugin = Plugins.Find(x => x.Name == name);
            if (plugin == null) return;
            
            plugin.IsLoaded = false;
            Log.Info($"Unloaded plugin {plugin.Title} by {plugin.Author} v{plugin.Version}");
        }
    }
}