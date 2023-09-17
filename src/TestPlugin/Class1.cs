using BlessingStudio.PluginLoader.Attributes;
using BlessingStudio.PluginLoader.Interfaces;

namespace TestPlugin
{
    [Plugin("TestPlugin", "测试插件", "1.0.0", "{A93D168A-C410-48C3-92E8-86375503DBD3}", "Ddggdd135")]
    public class TestPlugin : IPlugin
    {
        public void OnDisable()
        {
            Test.var1 = "hello";
        }

        public bool OnEnable()
        {
            Test.var1 = "hello world";
            return true;
        }

        public void OnLoad()
        {
            Console.WriteLine("测试插件已加载");
        }

        public void OnUnload()
        {

        }
    }
}