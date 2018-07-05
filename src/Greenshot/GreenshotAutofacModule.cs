﻿#region Greenshot GNU General Public License

// Greenshot - a free and open source screenshot tool
// Copyright (C) 2007-2018 Thomas Braun, Jens Klingen, Robin Krom
// 
// For more information see: http://getgreenshot.org/
// The Greenshot project is hosted on GitHub https://github.com/greenshot/greenshot
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 1 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

#endregion

using Autofac;
using Dapplo.Addons;
using Dapplo.CaliburnMicro.Configuration;
using Dapplo.CaliburnMicro.Security;
using Dapplo.Ini;
using Dapplo.Language;
using Greenshot.Addons.Components;
using Greenshot.Components;
using Greenshot.Configuration;
using Greenshot.Forms;
using Greenshot.Helpers;
using Greenshot.Ui.Configuration.ViewModels;
using Greenshot.Ui.Misc.ViewModels;

namespace Greenshot
{
    /// <inheritdoc />
    public class GreenshotAutofacModule : AddonModule
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder
                .Register(context => IniConfig.Current.Get<IMetroConfiguration>())
                .As<IMetroConfiguration>()
                .SingleInstance();

            builder
                .Register(context => LanguageLoader.Current.Get<IConfigTranslations>())
                .As<IConfigTranslations>()
                .SingleInstance();

            builder
                .RegisterType<ConfigViewModel>()
                .AsSelf();

            builder
                .RegisterType<AuthenticationProvider>()
                .As<IAuthenticationProvider>()
                .AsSelf()
                .SingleInstance();
            builder
                .RegisterType<MainForm>()
                .AsSelf()
                .SingleInstance();

            builder
                .RegisterType<PrintHelper>()
                .AsSelf();

            builder
                .RegisterType<PrintOptionsDialog>()
                .AsSelf();

            builder
                .RegisterType<LanguageDialog>()
                .AsSelf();

            builder
                .RegisterType<AboutForm>()
                .AsSelf();

            // Configuration
            builder
                .RegisterAssemblyTypes(ThisAssembly)
                .AssignableTo<IConfigScreen>()
                .As<IConfigScreen>()
                .SingleInstance();

            // Startup and Shutdown
            builder
                .RegisterAssemblyTypes(ThisAssembly)
                .AssignableTo<IService>()
                .As<IService>()
                .AsSelf()
                .AsImplementedInterfaces()
                .SingleInstance();
            
            builder
                .RegisterType<ErrorViewModel>()
                .AsSelf();

            builder
                .RegisterType<WindowHandle>()
                .AsSelf()
                .SingleInstance();

            // Destinations
            builder
                .RegisterAssemblyTypes(ThisAssembly)
                .AssignableTo<IDestination>()
                .As<IDestination>()
                .SingleInstance();

            base.Load(builder);
        }
    }
}
