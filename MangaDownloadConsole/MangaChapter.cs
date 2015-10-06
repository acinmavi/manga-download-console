/*
 * Created by SharpDevelop.
 * User: Admin15
 * Date: 02/11/2013
 * Time: 10:24 AM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;

namespace MangaDownloadConsole
{
	/// <summary>
	/// Description of MangaChapter.
	/// </summary>
	public class MangaChapter
	{
		public MangaChapter()
		{
		}
		public String ChapterLink { get; set; }
		public int ChapterNumber { get; set; }
		public String MangaName { get; set; }
		
		public  Dictionary<string,string> ListImageLink { get; set; }
		
		public override String ToString()
		{
			return "[MangaName="+MangaName+",ChapterNumber="+ChapterNumber+",ChapterLink="
				+ChapterLink+",ImageLinks:"+string.Join("\r\n",ListImageLink)+"\r\n]";
		}
	}
}
