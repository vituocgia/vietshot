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
using Greenshot.Addons.Core;
using Greenshot.Addons.Extensions;
using Greenshot.Addons.ViewModels;

namespace Greenshot.Addon.Flickr.ViewModels
{
    public sealed class FlickrConfigViewModel : SimpleConfigScreen
    {
        /// <summary>
        ///     Here all disposables are registered, so we can clean the up
        /// </summary>
        private CompositeDisposable _disposables;

        public IFlickrConfiguration FlickrConfiguration { get; }
        
        public IFlickrLanguage FlickrLanguage { get; }
        
        public FileConfigPartViewModel FileConfigPartViewModel { get; }

        public FlickrConfigViewModel(
            IFlickrConfiguration flickrConfiguration,
            IFlickrLanguage flickrLanguage,
            FileConfigPartViewModel fileConfigPartViewModel)
        {
            FlickrConfiguration = flickrConfiguration;
            FlickrLanguage = flickrLanguage;
            FileConfigPartViewModel = fileConfigPartViewModel;
        }

        public override void Initialize(IConfig config)
        {
            FileConfigPartViewModel.DestinationFileConfiguration = FlickrConfiguration;
            // Prepare disposables
            _disposables?.Dispose();

            // Place this under the Ui parent
            ParentId = nameof(ConfigIds.Destinations);

            // Make sure Commit/Rollback is called on the IUiConfiguration
            config.Register(FlickrConfiguration);

            // automatically update the DisplayName
            _disposables = new CompositeDisposable
            {
                FlickrLanguage.CreateDisplayNameBinding(this, nameof(IFlickrLanguage.SettingsTitle))
            };
            
            base.Initialize(config);
        }

        protected override void OnDeactivate(bool close)
        {
            _disposables.Dispose();
            base.OnDeactivate(close);
        }

        public SafetyLevel SelectedSafetyLevel
        {
            get => FlickrConfiguration.SafetyLevel;
            set
            {
                FlickrConfiguration.SafetyLevel = value;
                NotifyOfPropertyChange();
            }
        }

        public IDictionary<SafetyLevel, string> SafetyLevels => FlickrLanguage.TranslationValuesForEnum<SafetyLevel>();
    }
}
