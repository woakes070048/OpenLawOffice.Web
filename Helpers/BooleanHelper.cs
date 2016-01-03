// -----------------------------------------------------------------------
// <copyright file="TimeSpanHelper.cs" company="Nodine Legal, LLC">
// Licensed to Nodine Legal, LLC under one
// or more contributor license agreements.  See the NOTICE file
// distributed with this work for additional information
// regarding copyright ownership.  Nodine Legal, LLC licenses this file
// to you under the Apache License, Version 2.0 (the
// "License"); you may not use this file except in compliance
// with the License.  You may obtain a copy of the License at
//
//  http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing,
// software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
// KIND, either express or implied.  See the License for the
// specific language governing permissions and limitations
// under the License.
// </copyright>
// -----------------------------------------------------------------------

namespace OpenLawOffice.Web.Helpers
{
    using System;
    using System.Web;

    public class BooleanHelper
    {
        public static IHtmlString YesNo(bool var)
        {
            if (var)
                return new HtmlString("<span style=\"color: green;\">Yes</span>");
            else
                return new HtmlString("<span style=\"color: red;\">No</span>");
        }

        public static IHtmlString YesNo(object obj)
        {
            if (obj.GetType() != typeof(bool))
                throw new InvalidCastException("Cannot cast " + obj.GetType().FullName + " to " + typeof(bool).FullName);

            return YesNo((bool)obj);
        }
    }
}