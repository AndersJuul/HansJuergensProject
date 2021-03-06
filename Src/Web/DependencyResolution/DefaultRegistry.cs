// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DefaultRegistry.cs" company="Web Advanced">
// Copyright 2012 Web Advanced (www.webadvanced.com)
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0

// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using HansJuergenWeb.WebHJ.Controllers;
using HansJuergenWeb.WebHJ.Validators;

namespace HansJuergenWeb.WebHJ.DependencyResolution {
    using StructureMap.Configuration.DSL;
    using StructureMap.Graph;
    using EasyNetQ;
    using StructureMap;

    public class DefaultRegistry : Registry {
        #region Constructors and Destructors

        public DefaultRegistry()
        {
            var appSettings = new AppSettings();
            var bus = RabbitHutch.CreateBus(appSettings.EasyNetQConfig);

            Scan(
                scan => {
                    scan.TheCallingAssembly();
                    scan.WithDefaultConventions();
					scan.With(new ControllerConvention());
                    scan.AddAllTypesOf(typeof(IValidateUpload))
                        .NameBy(type => type.Name.ToLower());
                });
            For<IBus>().Use(bus);
            For<IAppSettings>().Use(appSettings);
        }

        #endregion
    }
}