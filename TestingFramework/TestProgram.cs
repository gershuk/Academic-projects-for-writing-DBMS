namespace TestingFramework
{
    class TestProgram
    {
        static void Main()
        {
            var engine = new Engine();
            engine.Run();
            engine.Dispose();
        }
    }
}
