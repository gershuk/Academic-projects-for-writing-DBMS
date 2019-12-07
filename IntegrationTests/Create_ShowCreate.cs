using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IntegrationTests
{
    [TestClass]
    public class Create_ShowCreate
    {
        [TestMethod]
        public void TestCreateCommandSynax ()
        {
            var cl = new TestClient();
            var test1 = cl.SendQuery("sdfvsdfvga");
            var test2 = cl.SendQuery("sdfvsdfvga\nsdbsdzvzsrfva");
            var test3 = cl.SendQuery("sdfvsdfvga\n\n\n\n\n\"");
            var test4 = cl.SendQuery("sdfvsdfvgasdfvasdvcasDC");
            var test5 = cl.SendQuery("sdfvsdfvgaSSFDBAFVA AV ASDFV ARVF ASDVC ASDFV ");
        }
    }
}
