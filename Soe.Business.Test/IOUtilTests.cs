using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SoftOne.Soe.Util.Tests
{
    [TestClass()]
    public class IOUtilTests
    {
        [TestMethod()]
        public void FileNameSafeTest()
        {
            var test =  IOUtil.FileNameSafe(@"asdö\\lkölkasd32423/");
            var test2 = IOUtil.FileNameSafe("///");
            Assert.IsTrue(test != null && test2 != null);
        }
    }
}