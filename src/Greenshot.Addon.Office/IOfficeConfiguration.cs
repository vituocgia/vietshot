﻿#region Greenshot GNU General License

// Greenshot - a free and open source screenshot tool
// Copyright (C) 2007-2018 Thomas Braun, Jens Klingen, Robin Krom
// 
// For more information see: http://getgreenshot.org/
// The Greenshot project is hosted on GitHub https://github.com/greenshot/greenshot
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General License as published by
// the Free Software Foundation, either version 1 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General License for more details.
// 
// You should have received a copy of the GNU General License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

#endregion

#region Usings

using System.ComponentModel;
using Dapplo.Ini;
using Dapplo.InterfaceImpl.Extensions;
using Greenshot.Addon.Office.OfficeInterop;

#endregion

namespace Greenshot.Addon.Office
{
    /// <summary>
    ///     Office configuration
    /// </summary>
    [IniSection("Office")]
	[Description("Greenshot Office configuration")]
	public interface IOfficeConfiguration : IIniSection, ITransactionalProperties, INotifyPropertyChanged
	{
		[Description("Default type for emails. (Text, Html)")]
		[DefaultValue(EmailFormats.Html)]
		EmailFormats OutlookEmailFormat { get; set; }

		[Description("Email subject pattern, works like the OutputFileFilenamePattern")]
		[DefaultValue("${title}")]
		string EmailSubjectPattern { get; set; }

		[Description("Default value for the to in emails that are created")]
		[DefaultValue("")]
		string EmailTo { get; set; }

		[Description("Default value for the CC in emails that are created")]
		[DefaultValue("")]
		string EmailCC { get; set; }

		[Description("Default value for the BCC in emails that are created")]
		[DefaultValue("")]
		string EmailBCC { get; set; }

		[Description("For Outlook: Allow export in meeting items")]
		[DefaultValue(false)]
		bool OutlookAllowExportInMeetings { get; set; }

		[Description("For Word: Lock the aspect ratio of the image")]
		[DefaultValue(true)]
		bool WordLockAspectRatio { get; set; }

		[Description("For Powerpoint: Lock the aspect ratio of the image")]
		[DefaultValue(true)]
		bool PowerpointLockAspectRatio { get; set; }

		[Description("For Powerpoint: Slide layout, changing this to a wrong value will fallback on ppLayoutBlank!!")]
		[DefaultValue(PPSlideLayout.ppLayoutPictureWithCaption)]
		PPSlideLayout PowerpointSlideLayout { get; set; }
	}
}