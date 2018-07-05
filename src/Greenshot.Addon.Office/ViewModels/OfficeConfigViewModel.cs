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

using System.Collections.Generic;
using System.Reactive.Disposables;
using Dapplo.CaliburnMicro.Configuration;
using Dapplo.CaliburnMicro.Extensions;
using Greenshot.Addon.Office.OfficeInterop;
using Greenshot.Addons;
using Greenshot.Addons.Core;
using Greenshot.Addons.Extensions;

namespace Greenshot.Addon.Office.ViewModels
{
    public sealed class OfficeConfigViewModel : SimpleConfigScreen
    {
        /// <summary>
        ///     Here all disposables are registered, so we can clean the up
        /// </summary>
        private CompositeDisposable _disposables;

        public IOfficeConfiguration OfficeConfiguration { get; }
        public IOfficeLanguage OfficeLanguage { get; }
        public IGreenshotLanguage GreenshotLanguage { get; }

        public OfficeConfigViewModel(
            IOfficeConfiguration officeConfiguration,
            IOfficeLanguage officeLanguage,
            IGreenshotLanguage greenshotLanguage)
        {
            OfficeConfiguration = officeConfiguration;
            OfficeLanguage = officeLanguage;
            GreenshotLanguage = greenshotLanguage;
        }

        public override void Initialize(IConfig config)
        {
            // Prepare disposables
            _disposables?.Dispose();
            _disposables = new CompositeDisposable();

            // Place this under the Ui parent
            ParentId = nameof(ConfigIds.Destinations);

            // Make sure Commit/Rollback is called on the IUiConfiguration
            config.Register(OfficeConfiguration);

            // automatically update the DisplayName
            var officeLanguageBinding = OfficeLanguage.CreateDisplayNameBinding(this, nameof(IOfficeLanguage.SettingsTitle));

            // Make sure the greenshotLanguageBinding is disposed when this is no longer active
            _disposables.Add(officeLanguageBinding);

            base.Initialize(config);
        }

        protected override void OnDeactivate(bool close)
        {
            _disposables.Dispose();
            base.OnDeactivate(close);
        }

        public PPSlideLayout SelectedSlideLayout
        {
            get => OfficeConfiguration.PowerpointSlideLayout;
            set
            {
                OfficeConfiguration.PowerpointSlideLayout = value;
                NotifyOfPropertyChange();
            }
        }

        public IDictionary<PPSlideLayout, string> SlideLayouts => GreenshotLanguage.TranslationValuesForEnum<PPSlideLayout>();
    }
}
