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
using System.IO;
using System.Text;
using Dapplo.Ini;
using Dapplo.Log;
using Dapplo.Windows.Com;
using Greenshot.Addon.Office.OfficeInterop;
using mshtml;
using Microsoft.Win32;

#endregion

namespace Greenshot.Addon.Office.OfficeExport
{
	/// <summary>
	///     Outlook exporter has all the functionality to export to outlook
	/// </summary>
	public static class OutlookEmailExporter
	{
		// The signature key can be found at:
		// HKEY_CURRENT_USER\Software\Microsoft\Windows NT\CurrentVersion\Windows Messaging Subsystem\Profiles\<DefaultProfile>\9375CFF0413111d3B88A00104B2A6676\<xxxx> [New Signature]
		private const string ProfilesKey = @"Software\Microsoft\Windows NT\CurrentVersion\Windows Messaging Subsystem\Profiles\";
		private const string AccountKey = "9375CFF0413111d3B88A00104B2A6676";
		private const string NewSignatureValue = "New Signature";
		private const string DefaultProfileValue = "DefaultProfile";
		private static readonly LogSource Log = new LogSource();
		private static readonly IOfficeConfiguration OfficeConfig = IniConfig.Current.Get<IOfficeConfiguration>();
		private static readonly string SignaturePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"Microsoft\Signatures");
		private static Version _outlookVersion;
		private static string _currentUser;

		/// <summary>
		///     A method to retrieve all inspectors which can act as an export target
		/// </summary>
		/// <returns>List of strings with inspector captions (window title)</returns>
		public static IDictionary<string, OlObjectClass> RetrievePossibleTargets()
		{
			IDictionary<string, OlObjectClass> inspectorCaptions = new SortedDictionary<string, OlObjectClass>();
			try
			{
				using (var outlookApplication = GetOutlookApplication())
				{
					if (outlookApplication == null)
					{
						return inspectorCaptions;
					}

					if (_outlookVersion.Major >= (int) OfficeVersions.Office2013)
					{
						// Check inline "panel" for Outlook 2013
						using (var activeExplorer = outlookApplication.ActiveExplorer())
						{
							if (activeExplorer != null)
							{
								using (var inlineResponse = activeExplorer.ActiveInlineResponse)
								{
									if (CanExportToInspector(inlineResponse))
									{
										var currentItemClass = inlineResponse.Class;
										inspectorCaptions.Add(activeExplorer.Caption, currentItemClass);
									}
								}
							}
						}
					}

					using (var inspectors = outlookApplication.Inspectors)
					{
						if (inspectors != null && inspectors.Count > 0)
						{
							for (var i = 1; i <= inspectors.Count; i++)
							{
								using (var inspector = outlookApplication.Inspectors[i])
								{
									using (var currentItem = inspector.CurrentItem)
									{
									    if (!CanExportToInspector(currentItem))
									    {
									        continue;
									    }

									    var currentItemClass = currentItem.Class;
									    inspectorCaptions.Add(inspector.Caption, currentItemClass);
									}
								}
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				Log.Warn().WriteLine(ex, "Problem retrieving word destinations, ignoring: ");
			}
			return inspectorCaptions;
		}

		/// <summary>
		///     Return true if we can export to the supplied inspector
		/// </summary>
		/// <param name="currentItem">the Item to check</param>
		/// <returns></returns>
		private static bool CanExportToInspector(IItem currentItem)
		{
			try
			{
				if (currentItem != null)
				{
					var currentItemClass = currentItem.Class;
					if (OlObjectClass.olMail.Equals(currentItemClass))
					{
						var mailItem = (IMailItem) currentItem;
						Log.Debug().WriteLine("Mail sent: {0}", mailItem.Sent);
						if (!mailItem.Sent)
						{
							return true;
						}
					}
					else if (_outlookVersion.Major >= (int) OfficeVersions.Office2010 && OfficeConfig.OutlookAllowExportInMeetings && OlObjectClass.olAppointment.Equals(currentItemClass))
					{
						var appointmentItem = (IAppointmentItem) currentItem;
						if (string.IsNullOrEmpty(appointmentItem.Organizer) || _currentUser != null && _currentUser.Equals(appointmentItem.Organizer))
						{
							return true;
						}
						Log.Debug().WriteLine("Not exporting, as organizer is {0} and currentuser {1}", appointmentItem.Organizer, _currentUser);
					}
				}
			}
			catch (Exception ex)
			{
				Log.Warn().WriteLine("Couldn't process item due to: {0}", ex.Message);
			}
			return false;
		}


		/// <summary>
		///     Export the image stored in tmpFile to the Inspector with the caption
		/// </summary>
		/// <param name="inspectorCaption">Caption of the inspector</param>
		/// <param name="tmpFile">Path to image file</param>
		/// <param name="attachmentName">name of the attachment (used as the tooltip of the image)</param>
		/// <returns>true if it worked</returns>
		public static bool ExportToInspector(string inspectorCaption, string tmpFile, string attachmentName)
		{
			using (var outlookApplication = GetOrCreateOutlookApplication())
			{
				if (outlookApplication == null)
				{
					return false;
				}
				if (_outlookVersion.Major >= (int) OfficeVersions.Office2013)
				{
					// Check inline "panel" for Outlook 2013
					using (var activeExplorer = outlookApplication.ActiveExplorer())
					{
						if (activeExplorer == null)
						{
							return false;
						}
						var currentCaption = activeExplorer.Caption;
						if (currentCaption.StartsWith(inspectorCaption))
						{
							using (var inlineResponse = activeExplorer.ActiveInlineResponse)
							{
								using (var currentItem = activeExplorer.ActiveInlineResponse)
								{
									if (CanExportToInspector(inlineResponse))
									{
										try
										{
											return ExportToInspector(activeExplorer, currentItem, tmpFile, attachmentName);
										}
										catch (Exception exExport)
										{
											Log.Error().WriteLine(exExport, "Export to " + currentCaption + " failed.");
										}
									}
								}
							}
						}
					}
				}

				using (var inspectors = outlookApplication.Inspectors)
				{
					if (inspectors == null || inspectors.Count == 0)
					{
						return false;
					}
					Log.Debug().WriteLine("Got {0} inspectors to check", inspectors.Count);
					for (var i = 1; i <= inspectors.Count; i++)
					{
						using (var inspector = outlookApplication.Inspectors[i])
						{
							var currentCaption = inspector.Caption;
						    if (!currentCaption.StartsWith(inspectorCaption))
						    {
						        continue;
						    }

						    using (var currentItem = inspector.CurrentItem)
						    {
						        if (!CanExportToInspector(currentItem))
						        {
						            continue;
						        }

						        try
						        {
						            return ExportToInspector(inspector, currentItem, tmpFile, attachmentName);
						        }
						        catch (Exception exExport)
						        {
						            Log.Error().WriteLine(exExport, "Export to " + currentCaption + " failed.");
						        }
						    }
						}
					}
				}
			}
			return false;
		}

		/// <summary>
		///     Export the file to the supplied inspector
		/// </summary>
		/// <param name="inspectorOrExplorer">ICommonExplorer</param>
		/// <param name="currentItem">Item</param>
		/// <param name="tmpFile"></param>
		/// <param name="attachmentName"></param>
		/// <returns></returns>
		private static bool ExportToInspector(ICommonExplorer inspectorOrExplorer, IItem currentItem, string tmpFile, string attachmentName)
		{
			if (currentItem == null)
			{
				Log.Warn().WriteLine("No current item.");
				return false;
			}
			var itemClass = currentItem.Class;
			var isMail = OlObjectClass.olMail.Equals(itemClass);
			var isAppointment = OlObjectClass.olAppointment.Equals(itemClass);
			if (!isMail && !isAppointment)
			{
				Log.Warn().WriteLine("Item is no mail or appointment.");
				return false;
			}
			IMailItem mailItem = null;
			try
			{
				if (isMail)
				{
					mailItem = (IMailItem) currentItem;
					if (mailItem.Sent)
					{
						Log.Warn().WriteLine("Item already sent, can't export to {0}", currentItem.Subject);
						return false;
					}
				}

				// Make sure the inspector is activated, only this way the word editor is active!
				// This also ensures that the window is visible!
				inspectorOrExplorer.Activate();
				var isTextFormat = false;
				if (isMail)
				{
					isTextFormat = OlBodyFormat.olFormatPlain.Equals(mailItem.BodyFormat);
				}
				if (isAppointment || !isTextFormat)
				{
					// Check for wordmail, if so use the wordexporter
					// http://msdn.microsoft.com/en-us/library/dd492012%28v=office.12%29.aspx
					// Earlier versions of Outlook also supported an Inspector.HTMLEditor object property, but since Internet Explorer is no longer the rendering engine for HTML messages and posts, HTMLEditor is no longer supported.
					IWordDocument wordDocument = null;
					var explorer = inspectorOrExplorer as IExplorer;
					if (explorer != null)
					{
						wordDocument = explorer.ActiveInlineResponseWordEditor;
					}
					else
					{
						var inspector1 = inspectorOrExplorer as IInspector;
						if (inspector1 != null)
						{
							var inspector = inspector1;
							if (inspector.IsWordMail())
							{
								wordDocument = inspector.WordEditor;
							}
						}
					}
					if (wordDocument != null)
					{
						try
						{
							if (WordExporter.InsertIntoExistingDocument(wordDocument.Application, wordDocument, tmpFile, null, null))
							{
								Log.Info().WriteLine("Inserted into Wordmail");
								wordDocument.Dispose();
								return true;
							}
						}
						catch (Exception exportException)
						{
							Log.Error().WriteLine(exportException, "Error exporting to the word editor, trying to do it via another method");
						}
					}
					else if (isAppointment)
					{
						Log.Info().WriteLine("Can't export to an appointment if no word editor is used");
						return false;
					}
					else
					{
						Log.Info().WriteLine("Trying export for outlook < 2007.");
					}
				}
				// Only use mailitem as it should be filled!!
				Log.Info().WriteLine("Item '{0}' has format: {1}", mailItem?.Subject, mailItem?.BodyFormat);

				string contentId;
				if (_outlookVersion.Major >= (int) OfficeVersions.Office2007)
				{
					contentId = Guid.NewGuid().ToString();
				}
				else
				{
					Log.Info().WriteLine("Older Outlook (<2007) found, using filename as contentid.");
					contentId = Path.GetFileName(tmpFile);
				}

				// Use this to change the format, it will probably lose the current selection.
				//if (!OlBodyFormat.olFormatHTML.Equals(currentMail.BodyFormat)) {
				//	Log.Info().WriteLine("Changing format to HTML.");
				//	currentMail.BodyFormat = OlBodyFormat.olFormatHTML;
				//}

				var inlinePossible = false;
				if (inspectorOrExplorer is IInspector && OlBodyFormat.olFormatHTML.Equals(mailItem?.BodyFormat))
				{
					// if html we can try to inline it
					// The following might cause a security popup... can't ignore it.
					try
					{
						var document2 = (inspectorOrExplorer as IInspector).HTMLEditor as IHTMLDocument2;
						if (document2 != null)
						{
							var selection = document2.selection;
							if (selection != null)
							{
								IHTMLTxtRange range = selection.createRange();
								if (range != null)
								{
									// First paste, than attach (otherwise the range is wrong!)
									range.pasteHTML("<BR/><IMG border=0 hspace=0 alt=\"" + attachmentName + "\" align=baseline src=\"cid:" + contentId + "\"><BR/>");
									inlinePossible = true;
								}
								else
								{
									Log.Debug().WriteLine("No range for '{0}'", inspectorOrExplorer.Caption);
								}
							}
							else
							{
								Log.Debug().WriteLine("No selection for '{0}'", inspectorOrExplorer.Caption);
							}
						}
						else
						{
							Log.Debug().WriteLine("No HTML editor for '{0}'", inspectorOrExplorer.Caption);
						}
					}
					catch (Exception e)
					{
						// Continue with non inline image
						Log.Warn().WriteLine(e, "Error pasting HTML, most likely due to an ACCESS_DENIED as the user clicked no.");
					}
				}

				// Create the attachment (if inlined the attachment isn't visible as attachment!)
				using (var attachment = mailItem.Attachments.Add(tmpFile, OlAttachmentType.olByValue, inlinePossible ? 0 : 1, attachmentName))
				{
					if (_outlookVersion.Major >= (int) OfficeVersions.Office2007)
					{
						// Add the content id to the attachment, this only works for Outlook >= 2007
						try
						{
							var propertyAccessor = attachment.PropertyAccessor;
							propertyAccessor.SetProperty(PropTag.ATTACHMENT_CONTENT_ID, contentId);
						}
						catch
						{
							// Ignore
						}
					}
				}
			}
			catch (Exception ex)
			{
				Log.Warn().WriteLine("Problem while trying to add attachment to Item '{0}' : {1}", inspectorOrExplorer.Caption, ex);
				return false;
			}
			try
			{
				inspectorOrExplorer.Activate();
			}
			catch (Exception ex)
			{
				Log.Warn().WriteLine(ex, "Problem activating inspector/explorer: ");
				return false;
			}
			Log.Debug().WriteLine("Finished!");
			return true;
		}

		/// <summary>
		///     Export image to a new email
		/// </summary>
		/// <param name="outlookApplication"></param>
		/// <param name="emailFormat"></param>
		/// <param name="tmpFile"></param>
		/// <param name="subject"></param>
		/// <param name="attachmentName"></param>
		/// <param name="to"></param>
		/// <param name="cc"></param>
		/// <param name="bcc"></param>
		/// <param name="url"></param>
		private static void ExportToNewEmail(IOutlookApplication outlookApplication, EmailFormats emailFormat, string tmpFile, string subject, string attachmentName, string to,
			string cc, string bcc, string url)
		{
			using (var newItem = outlookApplication.CreateItem(OlItemType.olMailItem))
			{
				if (newItem == null)
				{
					return;
				}
				var newMail = (IMailItem) newItem;
				newMail.Subject = subject;
				if (!string.IsNullOrEmpty(to))
				{
					newMail.To = to;
				}
				if (!string.IsNullOrEmpty(cc))
				{
					newMail.CC = cc;
				}
				if (!string.IsNullOrEmpty(bcc))
				{
					newMail.BCC = bcc;
				}
				newMail.BodyFormat = OlBodyFormat.olFormatHTML;
				string bodyString = null;
				// Read the default signature, if nothing found use empty email
				try
				{
					bodyString = GetOutlookSignature(emailFormat);
				}
				catch (Exception e)
				{
					Log.Error().WriteLine(e, "Problem reading signature!");
				}
				switch (emailFormat)
				{
					case EmailFormats.Text:
						// Create the attachment (and dispose the COM object after using)
						using (newMail.Attachments.Add(tmpFile, OlAttachmentType.olByValue, 1, attachmentName))
						{
							newMail.BodyFormat = OlBodyFormat.olFormatPlain;
							if (bodyString == null)
							{
								bodyString = "";
							}
							newMail.Body = bodyString;
						}
						break;
					default:
						var contentId = Path.GetFileName(tmpFile);
						// Create the attachment (and dispose the COM object after using)
						using (var attachment = newMail.Attachments.Add(tmpFile, OlAttachmentType.olByValue, 0, attachmentName))
						{
							// add content ID to the attachment
							if (_outlookVersion.Major >= (int) OfficeVersions.Office2007)
							{
								try
								{
									contentId = Guid.NewGuid().ToString();
									var propertyAccessor = attachment.PropertyAccessor;
									propertyAccessor.SetProperty(PropTag.ATTACHMENT_CONTENT_ID, contentId);
								}
								catch
								{
									Log.Info().WriteLine("Error working with the PropertyAccessor, using filename as contentid");
									contentId = Path.GetFileName(tmpFile);
								}
							}
						}

						newMail.BodyFormat = OlBodyFormat.olFormatHTML;
						var href = "";
						var hrefEnd = "";
						if (!string.IsNullOrEmpty(url))
						{
							href = $"<A HREF=\"{url}\">";
							hrefEnd = "</A>";
						}
						string htmlImgEmbedded = $"<BR/>{href}<IMG border=0 hspace=0 alt=\"{attachmentName}\" align=baseline src=\"cid:{contentId}\">{hrefEnd}<BR/>";
						string fallbackBody = $"<HTML><BODY>{htmlImgEmbedded}</BODY></HTML>";
						if (bodyString == null)
						{
							bodyString = fallbackBody;
						}
						else
						{
							var bodyIndex = bodyString.IndexOf("<body", StringComparison.CurrentCultureIgnoreCase);
							if (bodyIndex >= 0)
							{
								bodyIndex = bodyString.IndexOf(">", bodyIndex, StringComparison.Ordinal) + 1;
								bodyString = bodyIndex >= 0 ? bodyString.Insert(bodyIndex, htmlImgEmbedded) : fallbackBody;
							}
							else
							{
								bodyString = fallbackBody;
							}
						}
						newMail.HTMLBody = bodyString;
						break;
				}
				// So not save, otherwise the email is always stored in Draft folder.. (newMail.Save();)
				newMail.Display(false);

				using (var inspector = newMail.GetInspector())
				{
					if (inspector == null)
					{
						return;
					}
					try
					{
						inspector.Activate();
					}
					catch
					{
						// Ignore
					}
				}
			}
		}

		/// <summary>
		///     Helper method to create an outlook mail item with attachment
		/// </summary>
		/// <param name="emailFormat"></param>
		/// <param name="tmpFile">The file to send, do not delete the file right away!</param>
		/// <param name="subject"></param>
		/// <param name="attachmentName"></param>
		/// <param name="to"></param>
		/// <param name="cc"></param>
		/// <param name="bcc"></param>
		/// <param name="url"></param>
		/// <returns>true if it worked, false if not</returns>
		public static bool ExportToOutlook(EmailFormats emailFormat, string tmpFile, string subject, string attachmentName, string to, string cc, string bcc, string url)
		{
			var exported = false;
			try
			{
				using (var outlookApplication = GetOrCreateOutlookApplication())
				{
					if (outlookApplication != null)
					{
						ExportToNewEmail(outlookApplication, emailFormat, tmpFile, subject, attachmentName, to, cc, bcc, url);
						exported = true;
					}
				}
				return exported;
			}
			catch (Exception e)
			{
				Log.Error().WriteLine(e, "Error while creating an outlook mail item: ");
			}
			return exported;
		}

		/// <summary>
		///     Helper method to get the Outlook signature
		/// </summary>
		/// <returns></returns>
		private static string GetOutlookSignature(EmailFormats emailFormat)
		{
			using (var profilesKey = Registry.CurrentUser.OpenSubKey(ProfilesKey, false))
			{
				if (profilesKey == null)
				{
					return null;
				}
				var defaultProfile = (string) profilesKey.GetValue(DefaultProfileValue);
				Log.Debug().WriteLine("defaultProfile={0}", defaultProfile);
				using (var profileKey = profilesKey.OpenSubKey(defaultProfile + @"\" + AccountKey, false))
				{
				    if (profileKey == null)
				    {
				        return null;
				    }

				    var numbers = profileKey.GetSubKeyNames();
				    foreach (var number in numbers)
				    {
				        Log.Debug().WriteLine("Found subkey {0}", number);
				        using (var numberKey = profileKey.OpenSubKey(number, false))
				        {
				            var val = (byte[]) numberKey?.GetValue(NewSignatureValue);
				            if (val == null)
				            {
				                continue;
				            }
				            var signatureName = "";
				            foreach (var b in val)
				            {
				                if (b != 0)
				                {
				                    signatureName += (char) b;
				                }
				            }
				            Log.Debug().WriteLine("Found email signature: {0}", signatureName);
				            string extension;
				            switch (emailFormat)
				            {
				                case EmailFormats.Text:
				                    extension = ".txt";
				                    break;
				                default:
				                    extension = ".htm";
				                    break;
				            }
				            var signatureFile = Path.Combine(SignaturePath, signatureName + extension);
				            if (File.Exists(signatureFile))
				            {
				                Log.Debug().WriteLine("Found email signature file: {0}", signatureFile);
				                return File.ReadAllText(signatureFile, Encoding.Default);
				            }
				        }
				    }
				}
			}
			return null;
		}

		/// <summary>
		///     Initialize static outlook variables like version and currentuser
		/// </summary>
		/// <param name="outlookApplication"></param>
		private static void InitializeVariables(IOutlookApplication outlookApplication)
		{
			if (outlookApplication == null || _outlookVersion != null)
			{
				return;
			}
			try
			{
				_outlookVersion = new Version(outlookApplication.Version);
				Log.Info().WriteLine("Using Outlook {0}", _outlookVersion);
			}
			catch (Exception exVersion)
			{
				Log.Error().WriteLine(exVersion);
				Log.Warn().WriteLine("Assuming outlook version 1997.");
				_outlookVersion = new Version((int) OfficeVersions.Office97, 0, 0, 0);
			}
			// Preventing retrieval of currentUser if Outlook is older than 2007
		    if (_outlookVersion.Major < (int) OfficeVersions.Office2007)
		    {
		        return;
		    }

		    try
		    {
		        var mapiNamespace = outlookApplication.GetNameSpace("MAPI");
		        _currentUser = mapiNamespace.CurrentUser.Name;
		        Log.Info().WriteLine("Current user: {0}", _currentUser);
		    }
		    catch (Exception exNs)
		    {
		        Log.Error().WriteLine(exNs);
		    }
		}

		/// <summary>
		///     Call this to get the running outlook application, returns null if there isn't any.
		/// </summary>
		/// <returns>IOutlookApplication or null</returns>
		private static IOutlookApplication GetOutlookApplication()
		{
			var outlookApplication = ComWrapper.GetInstance<IOutlookApplication>();
			InitializeVariables(outlookApplication);
			return outlookApplication;
		}

		/// <summary>
		///     Call this to get the running outlook application, or create a new instance
		/// </summary>
		/// <returns>IOutlookApplication</returns>
		private static IOutlookApplication GetOrCreateOutlookApplication()
		{
			var outlookApplication = ComWrapper.GetOrCreateInstance<IOutlookApplication>();
			InitializeVariables(outlookApplication);
			return outlookApplication;
		}
	}
}