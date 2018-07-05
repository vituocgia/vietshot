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
using Greenshot.Addon.LegacyEditor.Drawing;
using Greenshot.Addons.Interfaces.Drawing;

#endregion

namespace Greenshot.Addon.LegacyEditor.Memento
{
	/// <summary>
	///     The DeleteElementMemento makes it possible to undo deleting an element
	/// </summary>
	public class DeleteElementMemento : IMemento
	{
		private readonly Surface surface;
		private IDrawableContainer drawableContainer;

		public DeleteElementMemento(Surface surface, IDrawableContainer drawableContainer)
		{
			this.surface = surface;
			this.drawableContainer = drawableContainer;
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		public bool Merge(IMemento otherMemento)
		{
			return false;
		}

		public IMemento Restore()
		{
			// Before
			drawableContainer.Invalidate();

			var oldState = new AddElementMemento(surface, drawableContainer);
			surface.AddElement(drawableContainer, false);
			// The container has a selected flag which represents the state at the moment it was deleted.
			if (drawableContainer.Selected)
			{
				surface.SelectElement(drawableContainer);
			}

			// After
			drawableContainer.Invalidate();
			return oldState;
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (drawableContainer != null)
				{
					drawableContainer.Dispose();
					drawableContainer = null;
				}
			}
		}
	}
}