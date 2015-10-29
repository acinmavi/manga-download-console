/*
 * Created by SharpDevelop.
 * User: Nextop
 * Date: 29/10/15
 * Time: 2:04 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;

namespace MangaDownloadConsole
{
	/// <summary>
	/// Description of MangaLink.
	/// </summary>
	public class MangaLink
	{
		public MangaLink()
		{
		}
		public int Id { get; set; }
		public string MangaName { get; set; }
		public int NumberOfChapter { get; set; }
		public bool IsActive { get; set; }
		
		public override string ToString()
		{
			return string.Format("[MangaLink Id={0}, MangaName={1}, NumberOfChapter={2}, IsActive={3}]", Id, MangaName, NumberOfChapter, IsActive);
		}

		
	}
}
