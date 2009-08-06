using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;

namespace Silverfly.Test
{
    /// <summary>
    /// Summary description for ExtensionsTest
    /// </summary>
    [TestClass]
    public class ExtensionsTest
    {
        bool isCompleted = true;

        [TestMethod]
        public void CanMarshallGenericEventHandler()
        {
            Action<object, EventArgs> handler = ((Action<object, EventArgs>)OnCompletion).Marshall();
            var thread = new Thread(
                new ThreadStart(() =>
                {
                    handler(this, new EventArgs());
                }
                )
            );
            thread.Start();
            thread.Join();
            Assert.IsTrue(isCompleted, "Event was not handled.");
        }

        public void OnCompletion(object sender, EventArgs ea)
        {
            isCompleted = true;
            Console.WriteLine("Completed");
        }
    }
}
