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

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
using Dapplo.Log;
using Dapplo.Windows.Common.Extensions;
using Dapplo.Windows.Common.Structs;
using Dapplo.Windows.Dpi;
using Dapplo.Windows.Dpi.Forms;
using Dapplo.Windows.Extensions;
using Greenshot.Addons.Components;
using Greenshot.Addons.Extensions;
using Greenshot.Addons.Interfaces;
using Greenshot.Gfx;

#endregion

namespace Greenshot.Addons.Core
{
    /// <summary>
    ///     Description of AbstractDestination.
    /// </summary>
    public abstract class AbstractDestination : IDestination
    {
        private static readonly LogSource Log = new LogSource();

        protected IGreenshotLanguage GreenshotLanguage { get; }
        protected ICoreConfiguration CoreConfiguration { get; }

        protected AbstractDestination(ICoreConfiguration coreConfiguration, IGreenshotLanguage greenshotLanguage)
        {
            CoreConfiguration = coreConfiguration;
            GreenshotLanguage = greenshotLanguage;
            Designation = GetType().GetDesignation();
        }

        /// <inheritdoc />
        public virtual string Designation { get; }

        /// <inheritdoc />
        public abstract string Description { get; }

        /// <inheritdoc />
        public virtual Bitmap DisplayIcon { get; set; }

        /// <inheritdoc />
        public virtual BitmapSource DisplayIconWpf => DisplayIcon?.ToBitmapSource() ?? GetDisplayIcon(DpiHandler.DefaultScreenDpi).ToBitmapSource();

        public virtual Bitmap GetDisplayIcon(double dpi)
        {
            return DisplayIcon;
        }

        public virtual bool HasDisplayIcon => true;

        public virtual Keys EditorShortcutKeys => Keys.None;

        /// <summary>
        /// Give a destination a preparation possibility before showing the toolstrip items
        /// </summary>
        /// <param name="destinationToolStripMenuItem">ToolStripMenuItem</param>
        /// <returns>Task</returns>
        protected virtual Task PrepareDynamicDestinations(ToolStripMenuItem destinationToolStripMenuItem)
        {
            return Task.FromResult(true);
        }

        /// <summary>
        /// Get the Dynamic destinations
        /// </summary>
        /// <returns>IEnumerable of IDestination</returns>
        public virtual IEnumerable<IDestination> DynamicDestinations()
        {
            return Enumerable.Empty<IDestination>();
        }

        /// <summary>
        /// Give a destination the possibility to clean up after showing the toolstrip item
        /// </summary>
        /// <param name="destinationToolStripMenuItem">ToolStripMenuItem</param>
        /// <returns>Task</returns>
        protected virtual Task AfterDynamicDestinations(ToolStripMenuItem destinationToolStripMenuItem)
        {
            return Task.FromResult(true);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        /// <inheritdoc />
        public virtual bool IsDynamic => false;

        /// <inheritdoc />
        public virtual bool UseDynamicsOnly => false;

        /// <inheritdoc />
        public virtual bool IsLinkable => false;

        /// <inheritdoc />
        public virtual bool IsActive => true;

        protected virtual ExportInformation ExportCapture(bool manuallyInitiated, ISurface surface, ICaptureDetails captureDetails)
        {
            return null;
        }

        /// <summary>
        /// This is the Async version of the export Capture, and by default it calls the ExportCapture.
        /// </summary>
        /// <param name="manuallyInitiated"></param>
        /// <param name="surface"></param>
        /// <param name="captureDetails"></param>
        /// <returns></returns>
        public virtual Task<ExportInformation> ExportCaptureAsync(bool manuallyInitiated, ISurface surface, ICaptureDetails captureDetails)
        {
            return Task.FromResult(ExportCapture(manuallyInitiated, surface, captureDetails));
        }

        /// <summary>
        ///     Return a menu item
        /// </summary>
        /// <param name="addDynamics">bool is dynamic entries need to be added</param>
        /// <param name="menu">ContextMenuStrip</param>
        /// <param name="destinationClickHandler">EventHandler</param>
        /// <param name="bitmapScaleHandler">BitmapScaleHandler</param>
        /// <returns>ToolStripMenuItem</returns>
        public virtual ToolStripMenuItem GetMenuItem(bool addDynamics, ContextMenuStrip menu, EventHandler destinationClickHandler, BitmapScaleHandler<IDestination> bitmapScaleHandler)
        {
            var basisMenuItem = new ToolStripMenuItem(Description)
            {
                Tag = this,
                Text = Description
            };

            bitmapScaleHandler.AddTarget(basisMenuItem, this);

            AddTagEvents(basisMenuItem, menu, Description);
            basisMenuItem.Click -= destinationClickHandler;
            basisMenuItem.Click += destinationClickHandler;

            if (IsDynamic && addDynamics)
            {
                basisMenuItem.DropDownOpening += async (sender, args) =>
                {
                    if (basisMenuItem.DropDownItems.Count != 0)
                    {
                        return;
                    }

                    // Give the destination a chance to prepare for the destinations
                    await PrepareDynamicDestinations(basisMenuItem).ConfigureAwait(true);

                    var subDestinations = new List<IDestination>();
                    // Fixing Bug #3536968 by catching the COMException (every exception) and not displaying the "subDestinations"
                    try
                    {
                        subDestinations.AddRange(DynamicDestinations());
                    }
                    catch (Exception ex)
                    {
                        Log.Error().WriteLine("Skipping {0}, due to the following error: {1}", Description, ex.Message);
                    }
                    await AfterDynamicDestinations(basisMenuItem).ConfigureAwait(true);
                    if (subDestinations.Count <= 0)
                    {
                        return;
                    }

                    if (UseDynamicsOnly && subDestinations.Count == 1)
                    {
                        basisMenuItem.Tag = subDestinations[0];
                        basisMenuItem.Text = subDestinations[0].Description;
                        basisMenuItem.Click -= destinationClickHandler;
                        basisMenuItem.Click += destinationClickHandler;
                    }
                    else
                    {
                        foreach (var subDestination in subDestinations)
                        {
                            var destinationMenuItem = new ToolStripMenuItem(subDestination.Description)
                            {
                                Tag = subDestination,
                            };
                            bitmapScaleHandler.AddTarget(destinationMenuItem, subDestination);

                            destinationMenuItem.Click += destinationClickHandler;
                            AddTagEvents(destinationMenuItem, menu, subDestination.Description);
                            basisMenuItem.DropDownItems.Add(destinationMenuItem);
                        }
                        basisMenuItem.ShowDropDown();
                    }
                };
            }

            return basisMenuItem;
        }

        protected virtual void Dispose(bool disposing)
        {
            //if (disposing) {}
        }

        /// <summary>
        ///     A small helper method to perform some default destination actions, like inform the surface of the export
        /// </summary>
        /// <param name="exportInformation"></param>
        /// <param name="surface"></param>
        public void ProcessExport(ExportInformation exportInformation, ISurface surface)
        {
            if (exportInformation != null && exportInformation.ExportMade)
            {
                if (!string.IsNullOrEmpty(exportInformation.Uri))
                {
                    surface.UploadUrl = exportInformation.Uri;
                    surface.SendMessageEvent(this, SurfaceMessageTyp.UploadedUri, string.Format(GreenshotLanguage.ExportedTo, exportInformation.DestinationDescription));
                }
                else if (!string.IsNullOrEmpty(exportInformation.Filepath))
                {
                    surface.LastSaveFullPath = exportInformation.Filepath;
                    surface.SendMessageEvent(this, SurfaceMessageTyp.FileSaved, string.Format(GreenshotLanguage.ExportedTo, exportInformation.DestinationDescription));
                }
                else
                {
                    surface.SendMessageEvent(this, SurfaceMessageTyp.Info, string.Format(GreenshotLanguage.ExportedTo, exportInformation.DestinationDescription));
                }
                surface.Modified = false;
            }
            else if (!string.IsNullOrEmpty(exportInformation?.ErrorMessage))
            {
                surface.SendMessageEvent(this, SurfaceMessageTyp.Error,
                    string.Format(GreenshotLanguage.ExportedTo, exportInformation.DestinationDescription) + " " + exportInformation.ErrorMessage);
            }
        }

        public override string ToString()
        {
            return Description;
        }

        /// <summary>
        ///     Helper method to add events which set the tag, this way we can see why there might be a close.
        /// </summary>
        /// <param name="menuItem">Item to add the events to</param>
        /// <param name="menu">Menu to set the tag</param>
        /// <param name="tagValue">Value for the tag</param>
        private void AddTagEvents(ToolStripMenuItem menuItem, ContextMenuStrip menu, string tagValue)
        {
            if (menuItem == null || menu == null)
            {
                return;
            }

            menuItem.MouseDown += (sender, args) =>
            {
                Log.Debug().WriteLine("Setting tag to '{0}'", tagValue);
                menu.Tag = tagValue;
            };
            menuItem.MouseUp += (sender, args) =>
            {
                Log.Debug().WriteLine("Deleting tag");
                menu.Tag = null;
            };
        }

        /// <summary>
        ///     This method will create and show the destination picker menu
        /// </summary>
        /// <param name="addDynamics">Boolean if the dynamic values also need to be added</param>
        /// <param name="surface">The surface which can be exported</param>
        /// <param name="captureDetails">Details for the surface</param>
        /// <param name="destinations">The list of destinations to show</param>
        /// <returns></returns>
        protected ExportInformation ShowPickerMenu(bool addDynamics, ISurface surface, ICaptureDetails captureDetails, IEnumerable<IDestination> destinations)
        {
            var menu = new ContextMenuStrip
            {
                Tag = null,
                TopLevel = true
            };
            var dpiHandler = menu.AttachDpiHandler();
            var bitmapScaleHandler = BitmapScaleHandler.Create<IDestination>(
                dpiHandler,
                (destination, dpi) => destination.GetDisplayIcon(dpi),
                (bitmap, d) => bitmap.ScaleIconForDisplaying(d));

            dpiHandler.OnDpiChanged.Subscribe(dpiChangeInfo =>
            {
                menu.ImageScalingSize = DpiHandler.ScaleWithDpi(CoreConfiguration.IconSize, dpiChangeInfo.NewDpi);
            });

            // Generate an empty ExportInformation object, for when nothing was selected.
            var exportInformation = new ExportInformation("", GreenshotLanguage.SettingsDestinationPicker);
            menu.Closing += (source, eventArgs) =>
            {
                Log.Debug().WriteLine("Close reason: {0}", eventArgs.CloseReason);
                switch (eventArgs.CloseReason)
                {
                    case ToolStripDropDownCloseReason.AppFocusChange:
                        if (menu.Tag == null)
                        {
                            // Do not allow the close if the tag is not set, this means the user clicked somewhere else.
                            eventArgs.Cancel = true;
                        }
                        else
                        {
                            Log.Debug().WriteLine("Letting the menu 'close' as the tag is set to '{0}'", menu.Tag);
                        }
                        break;
                    case ToolStripDropDownCloseReason.ItemClicked:
                    case ToolStripDropDownCloseReason.CloseCalled:
                        // The ContextMenuStrip can be "closed" for these reasons.
                        break;
                    case ToolStripDropDownCloseReason.Keyboard:
                        // Dispose as the close is clicked
                        if (!captureDetails.HasDestination("Editor"))
                        {
                            surface.Dispose();
                            surface = null;
                        }
                        break;
                    default:
                        eventArgs.Cancel = true;
                        break;
                }
            };
            menu.MouseEnter += (sender, args) =>
            {
                // in case the menu has been unfocused, focus again so that dropdown menus will still open on mouseenter
                if (!menu.ContainsFocus)
                {
                    menu.Focus();
                }
            };
            foreach (var destination in destinations)
            {
                // Fix foreach loop variable for the delegate
                var item = destination.GetMenuItem(addDynamics, menu,
                    async (sender, e) =>
                    {
                        var toolStripMenuItem = sender as ToolStripMenuItem;
                        var clickedDestination = (IDestination) toolStripMenuItem?.Tag;
                        if (clickedDestination == null)
                        {
                            return;
                        }
                        menu.Tag = clickedDestination.Designation;
                        // Export
                        exportInformation = await clickedDestination.ExportCaptureAsync(true, surface, captureDetails).ConfigureAwait(true);
                        if (exportInformation != null && exportInformation.ExportMade)
                        {
                            Log.Info().WriteLine("Export to {0} success, closing menu", exportInformation.DestinationDescription);
                            // close menu
                            menu.Close();
                            menu.Dispose();
                            // Cleanup surface, only if there is no editor in the destinations and we didn't export to the editor
                            if (!captureDetails.HasDestination("Editor") && !"Editor".Equals(clickedDestination.Designation))
                            {
                                surface.Dispose();
                                surface = null;
                            }
                        }
                        else
                        {
                            Log.Info().WriteLine("Export cancelled or failed, showing menu again");

                            // Make sure a click besides the menu don't close it.
                            menu.Tag = null;

                            // This prevents the problem that the context menu shows in the task-bar
                            ShowMenuAtCursor(menu);
                        }
                    } , bitmapScaleHandler
                );
                if (item != null)
                {
                    menu.Items.Add(item);
                }
            }
            // Close
            menu.Items.Add(new ToolStripSeparator());
            var closeItem = new ToolStripMenuItem(GreenshotLanguage.ContextmenuExit)
            {
                Image = GreenshotResources.GetBitmap("Close.Image")
            };
            closeItem.Click += (sender, args) =>
            {
                // This menu entry is the close itself, we can dispose the surface
                menu.Close();
                menu.Dispose();
                // Only dispose if there is a destination which keeps the capture
                if (captureDetails.HasDestination("Editor"))
                {
                    return;
                }

                surface.Dispose();
                surface = null;
            };
            menu.Items.Add(closeItem);

            ShowMenuAtCursor(menu);
            return exportInformation;
        }

        /// <summary>
        ///     This method will show the supplied context menu at the mouse cursor, also makes sure it has focus and it's not
        ///     visible in the taskbar.
        /// </summary>
        /// <param name="menu"></param>
        private static void ShowMenuAtCursor(ContextMenuStrip menu)
        {
            // find a suitable location
            var location = Cursor.Position;
            var menuRectangle = new NativeRect(location, menu.Size).Intersect(WindowCapture.GetScreenBounds());
            if (menuRectangle.Height < menu.Height)
            {
                location.Offset(-40, -(menuRectangle.Height - menu.Height));
            }
            else
            {
                location.Offset(-40, -10);
            }
            // This prevents the problem that the context menu shows in the task-bar
            // TODO: Get a handle
            // User32Api.SetForegroundWindow(PluginUtils.Host.NotifyIcon.ContextMenuStrip.Handle);
            menu.Show(location);
            menu.Focus();

            // Wait for the menu to close
            while (true)
            {
                if (menu.Visible)
                {
                    Application.DoEvents();
                    Thread.Sleep(200);
                }
                else
                {
                    //menu.Dispose();
                    break;
                }
            }
        }
    }
}