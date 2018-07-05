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

#region Usings

using System.ComponentModel;
using Dapplo.Ini;
using Dapplo.InterfaceImpl.Extensions;
using Greenshot.Addons.Core;

#endregion

namespace Greenshot.Addon.Jira
{
	/// <summary>
	///     Description of JiraConfiguration.
	/// </summary>
	[IniSection("Jira")]
    [Description("Greenshot Jira Plugin configuration")]
	public interface IJiraConfiguration : IIniSection, IDestinationFileConfiguration, INotifyPropertyChanged, ITransactionalProperties
	{
		[Description("Base url to Jira system, without anything else")]
		[DefaultValue("https://jira")]
		string Url { get; set; }

		[Description("Session timeout in minutes")]
		[DefaultValue(30)]
		int Timeout { get; set; }

	    [Description("The maximum number of entries to show in the list")]
	    [DefaultValue(20)]
        int MaxEntries { get; set; }
	}
}