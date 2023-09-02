

public static class Test
{
    public static string var1 = "hello";
    public static void Main(string[] args)
    {
        wonderlab.PluginLoader.PluginLoader.LoadAll();
        wonderlab.PluginLoader.PluginLoader.EnableAll();
        Console.WriteLine(var1);
    }
}