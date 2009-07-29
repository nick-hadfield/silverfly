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
// limitations under the License.
//
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Silverfly
{
    /// <summary>
    /// A reusable implementation of <see cref="IDisposable"/>.
    /// </summary>
    public class Disposable : IDisposable
    {
        private Action _term;

        /// <summary>
        /// Initialises a new instance of the <see cref="Disposable"/> class.
        /// </summary>
        /// <param name="init">A lambda to be called during construction.</param>
        /// <param name="term">A lambda to be called during disposal.</param>
        public Disposable(Action init, Action term)
        {
            _term = term;
            init();
        }

        #region IDisposable Members

        /// <summary>
        /// Called on disposal.
        /// </summary>
        public void Dispose()
        {
            _term();
        }

        #endregion
    }
}
