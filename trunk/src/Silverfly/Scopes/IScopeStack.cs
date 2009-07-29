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
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Silverfly.Scopes
{
    /// <summary>
    /// A stack of scopes/contexts.
    /// </summary>
    /// <typeparam name="SCOPE">The type of scopes/contexts to be stored on the stack.</typeparam>
    public interface IScopeStack<SCOPE>
    {
        /// <summary>
        /// Gets the inner most scope.
        /// </summary>
        SCOPE Peek();

        /// <summary>
        /// Adds a scope to the scope stack.
        /// </summary>
        /// <param name="scope"></param>
        void Push(SCOPE scope);

        /// <summary>
        /// Pops a scope from the scope stack.
        /// </summary>
        /// <returns></returns>
        SCOPE Pop();
    }
}
