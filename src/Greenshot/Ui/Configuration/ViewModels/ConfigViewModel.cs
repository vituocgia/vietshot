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

using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Windows;
using System.Windows.Controls;
using Dapplo.CaliburnMicro;
using Dapplo.CaliburnMicro.Configuration;
using Dapplo.CaliburnMicro.Extensions;
using Greenshot.Addons;
using Greenshot.Configuration;
using MahApps.Metro.IconPacks;

namespace Greenshot.Ui.Configuration.ViewModels
{
    /// <summary>
    ///     The settings view model is, well... for the settings :)
    ///     It is a conductor where only one item is active.
    /// </summary>
    public sealed class ConfigViewModel : Config<IConfigScreen>, IHaveIcon
    {
        /// <summary>
        ///     Here all disposables are registered, so we can clean the up
        /// </summary>
        private CompositeDisposable _disposables;

        /// <summary>
        ///     Get all settings controls, these are the items that are displayed.
        /// </summary>
        public override IEnumerable<Lazy<IConfigScreen>> ConfigScreens { get; set; }

        /// <summary>
        /// The translations
        /// </summary>
        public IGreenshotLanguage GreenshotLanguage { get; }

        /// <summary>
        /// Is used from View
        /// </summary>
        public IConfigTranslations ConfigTranslations { get; }

        public ConfigViewModel(
            IEnumerable<Lazy<IConfigScreen>> configScreens,
            IGreenshotLanguage greenshotLanguage,
            IConfigTranslations configTranslations)
        {
            ConfigScreens = configScreens;
            GreenshotLanguage = greenshotLanguage;
            ConfigTranslations = configTranslations;

            // automatically update the DisplayName
            GreenshotLanguage.CreateDisplayNameBinding(this, nameof(IGreenshotLanguage.SettingsTitle));

            // TODO: Check if we need to set the current language (this should update all registered OnPropertyChanged anyway, so it can run in the background
            //var lang = demoConfiguration.Language;
            //Task.Run(async () => await LanguageLoader.Current.ChangeLanguageAsync(lang).ConfigureAwait(false));
        }

        /// <summary>
        ///     Set the default config icon for the view
        /// </summary>
        public Control Icon => new PackIconMaterial
        {
            Kind = PackIconMaterialKind.Settings,
            Margin = new Thickness(10)
        };

        protected override void OnActivate()
        {
            // Prepare disposables
            _disposables?.Dispose();
            // automatically update the DisplayName
            _disposables = new CompositeDisposable
            {
                GreenshotLanguage.CreateDisplayNameBinding(this, nameof(IGreenshotLanguage.SettingsTitle))
            };

            base.OnActivate();
        }

        protected override void OnDeactivate(bool close)
        {
            base.OnDeactivate(close);
            _disposables.Dispose();
        }
    }
}
