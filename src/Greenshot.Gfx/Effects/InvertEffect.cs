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
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

#endregion

namespace Greenshot.Gfx.Effects
{
	/// <summary>
	///     InvertEffect
	/// </summary>
	public class InvertEffect : IEffect
	{
		public Bitmap Apply(Bitmap sourceBitmap, Matrix matrix)
		{
			return CreateNegative(sourceBitmap);
		}

	    /// <summary>
	    ///     Return negative of Bitmap
	    /// </summary>
	    /// <param name="sourceBitmap">Bitmap to create a negative off</param>
	    /// <returns>Negative bitmap</returns>
	    public static Bitmap CreateNegative(Bitmap sourceBitmap)
	    {
	        var clone = sourceBitmap.CloneBitmap();
	        var invertMatrix = new ColorMatrix(new[]
	        {
	            new float[] {-1, 0, 0, 0, 0},
	            new float[] {0, -1, 0, 0, 0},
	            new float[] {0, 0, -1, 0, 0},
	            new float[] {0, 0, 0, 1, 0},
	            new float[] {1, 1, 1, 1, 1}
	        });
	        clone.ApplyColorMatrix(invertMatrix);
	        return clone;
	    }

    }
}