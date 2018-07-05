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
using Caliburn.Micro;
using Dapplo.CaliburnMicro.Configuration;
using Dapplo.CaliburnMicro.Extensions;
using Greenshot.Addons;
using Greenshot.Addons.Core;
using Greenshot.Addons.ViewModels;

namespace Greenshot.Addon.ExternalCommand.ViewModels
{
    /// <summary>
    /// Configuration for the external commands
    /// </summary>
    public sealed class ExternalCommandConfigViewModel : SimpleConfigScreen
    {
        /// <summary>
        ///     Here all disposables are registered, so we can clean the up
        /// </summary>
        private CompositeDisposable _disposables;

        public IExternalCommandConfiguration ExternalCommandConfiguration { get; }

        public IExternalCommandLanguage ExternalCommandLanguage { get; }

        public IGreenshotLanguage GreenshotLanguage { get; }

        public FileConfigPartViewModel FileConfigPartViewModel { get; private set; }
        public ExternalCommandMasterViewModel ExternalCommandMasterViewModel { get; }

        public ExternalCommandConfigViewModel(
            IExternalCommandConfiguration externalCommandConfiguration,
            IExternalCommandLanguage externalCommandLanguage,
            IGreenshotLanguage greenshotLanguage,
            FileConfigPartViewModel fileConfigPartViewModel,
            ExternalCommandMasterViewModel externalCommandMasterViewModel)
        {
            ExternalCommandConfiguration = externalCommandConfiguration;
            ExternalCommandLanguage = externalCommandLanguage;
            GreenshotLanguage = greenshotLanguage;
            FileConfigPartViewModel = fileConfigPartViewModel;
            ExternalCommandMasterViewModel = externalCommandMasterViewModel;
        }

        public override void Initialize(IConfig config)
        {
            FileConfigPartViewModel.DestinationFileConfiguration = ExternalCommandConfiguration;
            // Prepare disposables
            _disposables?.Dispose();
            _disposables = new CompositeDisposable();

            // Place this under the Ui parent
            ParentId = nameof(ConfigIds.Destinations);

            // Make sure Commit/Rollback is called on the IUiConfiguration
            config.Register(ExternalCommandConfiguration);

            // automatically update the DisplayName
            var boxLanguageBinding = ExternalCommandLanguage.CreateDisplayNameBinding(this, nameof(IExternalCommandLanguage.SettingsTitle));

            // Make sure the greenshotLanguageBinding is disposed when this is no longer active
            _disposables.Add(boxLanguageBinding);

            base.Initialize(config);
        }

        protected override void OnActivate()
        {
            base.OnActivate();
            ExternalCommandMasterViewModel.ActivateWith(this);
        }

        protected override void OnDeactivate(bool close)
        {
            _disposables.Dispose();
            base.OnDeactivate(close);
        }

        public override void Commit()
        {
            ExternalCommandMasterViewModel.Store();
            base.Commit();
        }
    }
}
