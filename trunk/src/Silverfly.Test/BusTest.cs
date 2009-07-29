#region License
//
// Copyright 2009 Nicholas Hadfield
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
//
#endregion
using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Silverfly
{
    /// <summary>
    /// Summary description for BusTest
    /// </summary>
    [TestClass]
    public class BusTest
    {
        [TestMethod]
        public void CanPublishSubscribe()
        {
            bool isDog = false;
            bool isCat = false;
            bool isFish = false;

            IBus bus = new Bus();
            bus.Subscribe<Mammal>((o, n) =>
            {
                Console.WriteLine(n.Description);

                if (n is Dog)
                {
                    isDog = true;
                }
                else if (n is Cat)
                {
                    isCat = true;
                }
                else if (n is Fish)
                {
                    isFish = true;
                }
            }
            );

            bus.Publish(null, new Dog());
            bus.Publish(null, new Cat());
            bus.Publish(null, new Fish());

            Assert.IsTrue(isDog, "Dog should have been received.");
            Assert.IsTrue(isCat, "Cat should have been received.");
            Assert.IsFalse(isFish, "Fish should not have been received.");
        }

        [TestMethod]
        [ExpectedException(typeof(SilverflyException), "Attempting to Subscribe from within a Publish will result in a deadlock")]
        public void CannotSubscribeFromWithinPublish()
        {
            IBus bus = new Bus();

            bus.Subscribe<Dog>((o1, n1) =>
            {
                Console.WriteLine(n1.Description);
                bus.Subscribe<Cat>((o2, n2) => Console.WriteLine(n2.Description));
            }
            );

            bus.Publish(null, new Dog());
            bus.Publish(null, new Cat());
        }

        [TestMethod]
        [ExpectedException(typeof(SilverflyException), "Attempting to Unsubscribe from within a Publish will result in a deadlock")]
        public void CannotUnsubscribeFromWithinPublish()
        {
            IBus bus = new Bus();

            bus.Subscribe<Dog>((o1, n1) =>
            {
                Console.WriteLine(n1.Description);
                bus.Unsubscribe((object o, Dog n) => { });
            }
            );

            bus.Publish(null, new Dog());
            bus.Publish(null, new Cat());
        }

        [TestMethod]
        public void CanUnsubscibe()
        {
            bool isDog = false;

            IBus bus = new Bus();

            Action<object, Dog> handler = (o, n) =>
            {
                Console.WriteLine(n.Description);
                if (n is Dog)
                {
                    isDog = true;
                }
            };

            bus.Subscribe<Dog>(handler);
            bus.Unsubscribe<Dog>(handler);
            bus.Publish(null, new Dog());

            Assert.IsFalse(isDog, "Dog should not have been received.");
        }

        #region Model

        public class Animal
        {
            public virtual string Description { get { return "Animal"; } }
        }

        public class Mammal : Animal
        {
            public override string Description { get { return "Mammal"; } }
        }

        public class Dog : Mammal
        {
            public override string Description { get { return "Dog"; } }
        }

        public class Cat : Mammal
        {
            public override string Description { get { return "Cat"; } }
        }

        public class Fish : Animal
        {
            public override string Description { get { return "Fish"; } }
        }

        #endregion
    }
}
