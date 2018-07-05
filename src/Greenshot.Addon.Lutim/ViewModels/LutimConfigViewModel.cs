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

using System.Reactive.Disposables;
using Dapplo.CaliburnMicro.Configuration;
using Dapplo.CaliburnMicro.Extensions;
using Greenshot.Addons.Core;
using Greenshot.Addons.ViewModels;

namespace Greenshot.Addon.Lutim.ViewModels
{
    /// <summary>
    /// The ViewModel for the Lutim configuration
    /// </summary>
    public sealed class LutimConfigViewModel : SimpleConfigScreen
    {
        /// <summary>
        ///     Here all disposables are registered, so we can clean the up
        /// </summary>
        private CompositeDisposable _disposables;

        public ILutimConfiguration LutimConfiguration { get; }
        public ILutimLanguage LutimLanguage { get; }
        public FileConfigPartViewModel FileConfigPartViewModel { get; }

        public LutimConfigViewModel(
            ILutimConfiguration lutimConfiguration,
            ILutimLanguage lutimLanguage,
            FileConfigPartViewModel fileConfigPartViewModel)
        {
            LutimConfiguration = lutimConfiguration;
            LutimLanguage = lutimLanguage;
            FileConfigPartViewModel = fileConfigPartViewModel;
        }

        public override void Initialize(IConfig config)
        {
            FileConfigPartViewModel.DestinationFileConfiguration = LutimConfiguration;

            // Prepare disposables
            _disposables?.Dispose();

            // Place this under the Ui parent
            ParentId = nameof(ConfigIds.Destinations);

            // Make sure Commit/Rollback is called on the IUiConfiguration
            config.Register(LutimConfiguration);

            // automatically update the DisplayName
            _disposables = new CompositeDisposable
            {
                LutimLanguage.CreateDisplayNameBinding(this, nameof(ILutimLanguage.SettingsTitle))
            };

            base.Initialize(config);
        }

        protected override void OnDeactivate(bool close)
        {
            _disposables.Dispose();
            base.OnDeactivate(close);
        }
    }
}
