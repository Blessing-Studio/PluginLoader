using System.Reflection;
using System.Resources;
using System.Linq.Expressions;
using System.Runtime.Loader;
using BlessingStudio.PluginLoader.Attribute;
using BlessingStudio.PluginLoader.Interfaces;
using BlessingStudio.PluginLoader.Utils;
using BlessingStudio.PluginLoader.Events;

namespace BlessingStudio.PluginLoader
{
    /// <summary>
    /// 插件加载器
    /// </summary>
    public static class PluginLoader
    {
        /// <summary>
        /// 插件默认路径
        /// </summary>
        public static string PluginPath = StringUtil.GetSubPath(Environment.CurrentDirectory, "Plugins");
        /// <summary>
        /// 全局插件类
        /// </summary>
        public static List<IPlugin> Plugins = new List<IPlugin>();
        public static Dictionary<IPlugin, Assembly> Assemblys = new Dictionary<IPlugin, Assembly>();
        /// <summary>
        /// 获取插件信息
        /// </summary>
        /// <param name="Plugin">插件实例</param>
        /// <returns>插件信息</returns>
        public static PluginInfo? GetPluginInfo(IPlugin Plugin)
        {
            Type type = Plugin.GetType();
            Attribute? attribute = Attribute.GetCustomAttribute(type, typeof(PluginAttribute));
            PluginAttribute handler;
            if (attribute != null)
            {
                handler = (PluginAttribute)attribute;
            }
            else { return null; }
            if (handler != null)
            {
                PluginInfo info = new PluginInfo(type);
                info.Name = handler.Name;
                info.Description = handler.Description;
                info.Version = handler.Version;
                info.Guid = handler.Guid;
                info.Path = type.Assembly.Location;
                info.Icon = handler.Icon;
                info.Author = handler.Author;
                return info;
            }
            return null;

        }
        /// <summary>
        /// 加载插件
        /// </summary>
        /// <param name="Path">
        /// 插件文件路径
        /// </param>
        public static void Load(string Path)
        {
            CollectibleAssemblyLoadContext context = new();
            foreach (FileInfo file in new FileInfo(Path)!.Directory!.GetFiles())
            {
                context.LoadFromAssemblyPath(file.FullName);
            }
            Assembly assembly = context.LoadFromAssemblyPath(Path);
            Type? mainType = null;
            foreach (Type type in assembly.GetTypes())
            {
                if (type.GetCustomAttribute<PluginAttribute>() != null)
                {
                    mainType = type;
                    break;
                }
            }
            if (mainType != null)
            {
                IPlugin? plugin = Activator.CreateInstance(mainType) as IPlugin;
                if (plugin != null)
                {
                    Assemblys[plugin] = assembly;
                    Plugins.Add(plugin);
                    Event.CallEvent(new PluginLoadEvent() { PluginInfo = plugin.GetPluginInfo() });
                    plugin.OnLoad();
                    foreach (Type type in assembly.GetTypes())
                    {
                        if (type.GetCustomAttribute<ListenerAttribute>() != null)
                        {
                            IListener listener = (IListener)Activator.CreateInstance(type)!;
                            listener.Register();
                        }
                    }
                }
                else
                {
                    context.Unload();
                }
            }
            else
            {
                context.Unload();
            }
        }
        /// <summary>
        /// 通关插件名获取插件实例
        /// </summary>
        /// <param name="pluginName">插件名</param>
        /// <returns>插件实例</returns>
        public static IPlugin? GetPlugin(string pluginName)
        {
            foreach (IPlugin plugin in Plugins)
            {
                if (plugin.GetPluginInfo().Name == pluginName)
                {
                    return plugin;
                }
            }
            return null;
        }
        /// <summary>
        /// 获取所有加载的插件
        /// </summary>
        /// <returns>加载的插件</returns>
        public static IPlugin[] GetPlugins()
        {
            return Plugins.ToArray();
        }
        /// <summary>
        /// 卸载插件
        /// </summary>
        /// <param name="plugin">插件实例</param>
        public static void UnLoad(IPlugin plugin)
        {
            if (GetPlugins().Contains(plugin))
            {
                Assembly assembly = Assemblys[plugin];
                AssemblyLoadContext context = AssemblyLoadContext.GetLoadContext(assembly)!;
                Event.CallEvent(new PluginUnLoadEvent() { PluginInfo = plugin.GetPluginInfo() });
                Event.Listeners.RemoveAll((listener) => { return listener.PluginInfo.Guid == plugin.GetPluginInfo().Guid; });
                plugin.OnUnload();
                context.Unload();
            }
        }
        /// <summary>
        /// 加载插件文件夹中所有插件
        /// </summary>
        public static void LoadAllPlugins()
        {
            DirectoryInfo dir = new DirectoryInfo(PluginPath);
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                Load(file.FullName);
            }
        }
        /// <summary>
        /// 通过启用配置文件自动加载插件
        /// </summary>
        public static void LoadAll()
        {
            ConfigManager configManager = new ConfigManager(StringUtil.GetSubPath(PluginPath, "Plugins.json"));
            DirectoryInfo dir = new DirectoryInfo(PluginPath);
            foreach (DirectoryInfo PluginDir in dir.GetDirectories())
            {
                if (File.Exists(StringUtil.GetSubPath(PluginDir.FullName, "Plugin.dll")))
                {
                    bool isEnable = true;
                    try
                    {
                        isEnable = configManager.GetBool(StringUtil.GetSubPath(PluginDir.FullName, "Plugin.dll"));
                    }
                    catch { }
                    if (isEnable)
                    {
                        Load(StringUtil.GetSubPath(PluginDir.FullName, PluginDir.Name + ".dll"));
                    }
                }
                else if (File.Exists(StringUtil.GetSubPath(PluginDir.FullName, PluginDir.Name + ".dll")))
                {
                    bool isEnable = true;
                    try
                    {
                        isEnable = configManager.GetBool(StringUtil.GetSubPath(PluginDir.FullName, PluginDir.Name + ".dll"));
                    }
                    catch { }
                    if (isEnable)
                    {
                        Load(StringUtil.GetSubPath(PluginDir.FullName, PluginDir.Name + ".dll"));
                    }
                }
            }
        }
        /// <summary>
        /// 禁用插件
        /// </summary>
        /// <param name="PluginGuid">插件Guid</param>
        public static void SetDisable(string PluginGuid)
        {
            ConfigManager configManager = new ConfigManager(StringUtil.GetSubPath(PluginPath, "Plugins.json"));
            configManager.SetBool(PluginGuid, false);
            configManager.SaveConfig();
        }
        /// <summary>
        /// 启用插件
        /// </summary>
        /// <param name="PluginGuid">插件Guid</param>
        public static void SetEnable(string PluginGuid)
        {
            ConfigManager configManager = new ConfigManager(StringUtil.GetSubPath(PluginPath, "Plugins.json"));
            configManager.SetBool(PluginGuid, false);
            configManager.SaveConfig();
        }
        /// <summary>
        /// 禁用插件
        /// </summary>
        /// <param name="plugin">插件类</param>
        public static void SetDisable(IPlugin plugin)
        {
            ConfigManager configManager = new ConfigManager(StringUtil.GetSubPath(PluginPath, "Plugins.json"));
            configManager.SetBool(plugin.GetPluginInfo().Guid, false);
            configManager.SaveConfig();
        }
        /// <summary>
        /// 启用插件
        /// </summary>
        /// <param name="plugin">插件类</param>
        public static void SetEnable(IPlugin plugin)
        {
            ConfigManager configManager = new ConfigManager(StringUtil.GetSubPath(PluginPath, "Plugins.json"));
            configManager.SetBool(plugin.GetPluginInfo().Guid, false);
            configManager.SaveConfig();
        }
        /// <summary>
        /// 卸载所有插件
        /// </summary>
        public static void UnloadAll()
        {
            foreach (IPlugin plugin in Plugins.ToArray())
            {
                UnLoad(plugin);
            }
        }
        /// <summary>
        /// 启用所有已加载插件
        /// </summary>
        public static void EnableAll()
        {
            foreach (IPlugin plugin in Plugins)
            {
                plugin.OnEnable();
            }
        }
        /// <summary>
        /// 禁用所有已加载插件
        /// </summary>
        public static void DisableAll()
        {
            foreach (IPlugin plugin in Plugins)
            {
                plugin.OnDisable();
            }
        }
    }

}
