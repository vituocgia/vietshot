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

using System.Drawing;
using System.Windows.Forms;
using Greenshot.Addons;
using Greenshot.Addons.Components;
using Greenshot.Addons.Core;
using Greenshot.Addons.Interfaces;

#endregion

namespace Greenshot.Destinations
{
    /// <summary>
    ///     Description of FileWithDialog.
    /// </summary>
    [Destination("FileDialog", DestinationOrder.FileDialog)]

    public class FileWithDialogDestination : AbstractDestination
	{
	    private readonly IGreenshotLanguage _greenshotLanguage;

	    public FileWithDialogDestination(ICoreConfiguration coreConfiguration,
	        IGreenshotLanguage greenshotLanguage
	    ) : base(coreConfiguration, greenshotLanguage)
	    {
	        _greenshotLanguage = greenshotLanguage;
	    }

        public override string Description => _greenshotLanguage.SettingsDestinationFileas;

	    public override Keys EditorShortcutKeys => Keys.Control | Keys.Shift | Keys.S;

	    public override Bitmap DisplayIcon => GreenshotResources.GetBitmap("Save.Image");

	    protected override ExportInformation ExportCapture(bool manuallyInitiated, ISurface surface, ICaptureDetails captureDetails)
		{
			var exportInformation = new ExportInformation(Designation, Description);
			// Bug #2918756 don't overwrite path if SaveWithDialog returns null!
			var savedTo = ImageOutput.SaveWithDialog(surface, captureDetails);
			if (savedTo != null)
			{
				exportInformation.ExportMade = true;
				exportInformation.Filepath = savedTo;
				captureDetails.Filename = savedTo;
			    CoreConfiguration.OutputFileAsFullpath = savedTo;
			}
			ProcessExport(exportInformation, surface);
			return exportInformation;
		}
	}
}