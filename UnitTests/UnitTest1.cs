using SharpMacroPlayer.Bindings;
using SharpMacroPlayer.Macros;
using SharpMacroPlayer.Macros.MacroElements;

namespace SharpMacroPlayer.UnitTests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod("—оздание и сохранение тестового макроса")]
        public void CreateMacroAndSaveTest()
        {
            Macro testMacro = new();
            testMacro.MacroElements.Add(new WaitTimeMacroElement(100));
            MacroLoaderSaver.SaveMacros(testMacro, "testMacro.json");
            bool isFileThere = MacroLoaderSaver.GetAllMacros().Any(s => s == "testMacro.json");
            MacroLoaderSaver.DeleteMacro("testMacro.json");
            Assert.IsTrue(isFileThere);
        }
        [TestMethod("—оздание и сохранение тестового назначени€ клавиши")]
        public void CreateBindingAndSaveTest()
        {
            BindingContainer testBindings = new();
            BindingLoaderSaver.SaveBindings(testBindings, "testBinding.json");
            bool isFileThere = BindingLoaderSaver.GetAllBindings().Any(s => s == "testBinding.json");
            BindingLoaderSaver.DeleteBinding("testBinding.json");
            Assert.IsTrue(isFileThere);
        }
    }
}