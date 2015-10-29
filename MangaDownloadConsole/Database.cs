/*
 * Created by SharpDevelop.
 * User: Nextop
 * Date: 29/10/15
 * Time: 2:07 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using System.IO;
using LiteDB;
using System.Linq;

namespace MangaDownloadConsole
{
	/// <summary>
	/// Description of Database.
	/// </summary>
	public class Database
	{
		public static string LITE_DB = "manga.db";
		public static string SCHEMA = "mangas";
		public Database()
		{
		}
		
		public static List<string> GetAllManga(bool isActive)
		{
			var lsActiveManga = new List<string>();
			using(var db = new LiteDatabase(Path.Combine(Environment.CurrentDirectory,LITE_DB)))
			{
				// Get customer collection
				var col = db.GetCollection<MangaLink>(SCHEMA);

				// Use Linq to query documents
				lsActiveManga = col.FindAll().Where(x=>x.IsActive==isActive).Select(x=>x.MangaName).ToList();
			}
			return lsActiveManga;
		}
		
		public static List<MangaLink> GetAllMangaLink(bool isActive)
		{
			var lsActiveManga = new List<MangaLink>();
			using(var db = new LiteDatabase(Path.Combine(Environment.CurrentDirectory,LITE_DB)))
			{
				// Get customer collection
				var col = db.GetCollection<MangaLink>(SCHEMA);

				// Use Linq to query documents
				lsActiveManga = col.FindAll().Where(x=>x.IsActive==isActive).ToList();
			}
			return lsActiveManga;
		}
		
		public static List<string> GetAllActiveManga(){
			return GetAllManga(true);
		}
		public static List<string> GetAllInActiveManga(){
			return GetAllManga(false);
		}
		
		public static void Insert(List<MangaLink> mangaLinks)
		{
			using(var db = new LiteDatabase(Path.Combine(Environment.CurrentDirectory,LITE_DB)))
			{
				// Get customer collection
				var col = db.GetCollection<MangaLink>(SCHEMA);

				// Use Linq to query documents
				col.Insert(mangaLinks);
			}
		}
		
		public static void Insert(MangaLink mangaLink){
			var mangaLinks = new List<MangaLink>();
			mangaLinks.Add(mangaLink);
			Insert(mangaLinks);
		}
		
		public static void Update(List<MangaLink> mangaLinks)
		{
			foreach (var mangaLink in mangaLinks) {
				Update(mangaLink);
			}
		}
		
		public static void Update(MangaLink mangaLink)
		{
			using(var db = new LiteDatabase(Path.Combine(Environment.CurrentDirectory,LITE_DB)))
			{
				// Get customer collection
				var col = db.GetCollection<MangaLink>(SCHEMA);

				// Use Linq to query documents
				col.Update(mangaLink);
				Utilities.WriteLine("updated " + mangaLink + " in db");
			}
		}
		
		public static void Update(string mangaLink,bool isActive)
		{
			using(var db = new LiteDatabase(Path.Combine(Environment.CurrentDirectory,LITE_DB)))
			{
				// Get customer collection
				var col = db.GetCollection<MangaLink>(SCHEMA);

				MangaLink exist = col.FindOne(x=>x.MangaName==mangaLink);
				if(exist !=null) {
					exist.IsActive = isActive;
					col.Update(exist);
				}else {
					Utilities.WriteLine("no exist " + mangaLink + " in db");
				}
			}
		}
		
		public static void InsertOrUpdate(MangaLink mangaLink)
		{
			using(var db = new LiteDatabase(Path.Combine(Environment.CurrentDirectory,LITE_DB)))
			{
				// Get customer collection
				var col = db.GetCollection<MangaLink>(SCHEMA);

				// Use Linq to query documents
				MangaLink exist = col.FindOne(x=>x.MangaName==mangaLink.MangaName);
				if(exist !=null) {
					if(exist.NumberOfChapter != mangaLink.NumberOfChapter){
						exist.NumberOfChapter = mangaLink.NumberOfChapter;
						exist.IsActive = true;
						col.Update(exist);
						Utilities.WriteLine("updated " + exist + " in db");
					}
				}else{
					col.Insert(mangaLink);
					Utilities.WriteLine("insert " + mangaLink + " in db");
				}
			}
		}
	}
}
