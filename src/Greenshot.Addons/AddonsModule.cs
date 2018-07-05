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
using Dapplo.Ini;
using Dapplo.Language;
using Greenshot.Addons.Components;
using Greenshot.Addons.Controls;
using Greenshot.Addons.Core;
using Greenshot.Addons.ViewModels;

namespace Greenshot.Addons
{
    /// <inheritdoc />
    public class AddonsModule : AddonModule
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder
                .Register(context => IniConfig.Current.Get<IUiConfiguration>())
                .As<IUiConfiguration>()
                .SingleInstance();

            builder
                .Register(context => IniConfig.Current.Get<ICoreConfiguration>())
                .As<ICoreConfiguration>()
                .SingleInstance();

            builder
                .Register(context => LanguageLoader.Current.Get<IGreenshotLanguage>())
                .As<IGreenshotLanguage>()
                .SingleInstance();

            builder
                .Register(context => IniConfig.Current.Get<INetworkConfiguration>())
                .As<INetworkConfiguration>()
                .SingleInstance();

            builder
                .RegisterType<FileConfigPartViewModel>()
                .AsSelf();
            builder
                .RegisterType<DestinationHolder>()
                .AsSelf();

            builder
                .RegisterType<PleaseWaitForm>()
                .AsSelf();

            builder.RegisterType<SetupConfig>()
                .As<IStartable>()
                .SingleInstance();

            base.Load(builder);
        }

        /// <inheritdoc />
        private class SetupConfig : IStartable
        {
            public void Start()
            {
                // Register the after load, so it's called when the configuration is created
                IniConfig.Current.AfterLoad<ICoreConfiguration>(coreConfiguration => coreConfiguration.AfterLoad());
            }
        }
    }
}
