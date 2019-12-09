using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IntegrationTests
{
    [TestClass]
    public class Create_ShowCreate
    {
        [TestMethod]
        public void TestTest()
        {
            var st = new StackTrace();
            var currentMethodName = st.GetFrame(0).GetMethod().Name;
            var results = new TestData(this.GetType().FullName, currentMethodName);

            var test = results.GetResult();
            Assert.AreEqual("test1", test);
            results.FixResult("test1");
            results.Save();
            test = results.GetResult();
            Assert.AreEqual("test2", test);
            results.FixResult("test2");
            results.Save();
        }

        [TestMethod]
        public void TestCreateCommandSynax ()
        {
            var server = new TestServer();
            var cl = new TestClient();
            var test1 = cl.SendQuery("sdfvsdfvga");
            var test2 = cl.SendQuery("sdfvsdfvga\nsdbsdzvzsrfva");
            var test3 = cl.SendQuery("sdfvsdfvga\n\n\n\n\n\"");
            var test4 = cl.SendQuery("sdfvsdfvgasdfvasdvcasDC");
            var test5 = cl.SendQuery("sdfvsdfvgaSSFDBAFVA AV ASDFV ARVF ASDVC ASDFV ");
        }
    }
}
