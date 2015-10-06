/*
 * Created by SharpDevelop.
 * User: Admin15
 * Date: 01/11/2013
 * Time: 4:49 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;

namespace MangaDownloadConsole
{
	/// <summary>
	/// Description of MangaVolume.
	/// </summary>
	public class MangaVolume
	{
		public MangaVolume()
		{
		}
		
		public MangaVolume(String mangaName,int chapterPerVol)
		{
			ChapterPerVol = chapterPerVol;
			MangaName = mangaName;
		}
		public List<MangaChapter> ListChapter = new List<MangaChapter>();
		
		public int Volume { get; set; }
		public string MangaName { get; set; }
		public int ChapterPerVol { get; set; }
		public bool IsFull()
		{
			return ListChapter.Count == ChapterPerVol;
		}
		public void Add(MangaChapter chapter)
		{
			ListChapter.Add(chapter);
		}
		
		public override string ToString()
		{
			string output = "MangaVolume["+"Volume="+Volume+",MangaName="+MangaName+",ChapterPerVol="+ChapterPerVol
				+",ListChapter={"+string.Join("\r\n",ListChapter)+"}]\r\n\r\n";
			return output;
		}
	}
}
