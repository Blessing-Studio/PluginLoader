public static class Test
{
    public static string var1 = "hello";
    public static void Main(string[] args)
    {
        BlessingStudio.PluginLoader.PluginLoader.LoadAll();
        BlessingStudio.PluginLoader.PluginLoader.EnableAll();
        Console.WriteLine(var1);
    }
}