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
using Dapplo.CaliburnMicro.Translations;
using Dapplo.Ini;
using Dapplo.Language;
using Greenshot.Addon.Tfs.ViewModels;
using Greenshot.Addons.Components;

namespace Greenshot.Addon.Tfs
{
    /// <inheritdoc />
    public class TfsAddonModule : AddonModule
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder
                .Register(context => IniConfig.Current.Get<ITfsConfiguration>())
                .As<ITfsConfiguration>()
                .SingleInstance();

            builder
                .Register(context => LanguageLoader.Current.Get<ITfsLanguage>())
                .As<ITfsLanguage>()
                .SingleInstance();


            builder
                .RegisterType<TfsDestination>()
                .As<IDestination>()
                .SingleInstance();
            builder
                .RegisterType<TfsConfigViewModel>()
                .As<IConfigScreen>()
                .SingleInstance();
            builder
                .RegisterType<TfsClient>()
                .AsSelf()
                .SingleInstance();

            base.Load(builder);
        }
    }
}
