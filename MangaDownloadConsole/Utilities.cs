/*
 * Created by SharpDevelop.
 * User: Admin15
 * Date: 02/11/2013
 * Time: 1:00 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using HtmlAgilityPack;
using System.Linq;
using iTextSharp.text;
using iTextSharp.text.pdf;
using log4net;

namespace MangaDownloadConsole
{
	/// <summary>
	/// Description of MangaLinkFilter.
	/// </summary>
	public static class Utilities
	{
		public static int CHAPTER_PER_VOL = 10;
		public static int MAX_TASK = 5;
		public static int RETRY = 5;
		public static string NEWLINE="\r\n";
		public const String MANGA24 = "http://manga24h.com/";
		static string TTT="http://truyentranhtuan.com";
		static string COMICVN="http://comicvn.net";
		public readonly static ILog log = log4net.LogManager.GetLogger(typeof(Utilities));
		public static MangaSite site =MangaSite.None;
		/// <summary>
		/// get all chapter link in page
		/// </summary>
		/// <param name="MangaLink"></param>
		/// <param name="reverse"></param>
		/// <returns></returns>
		public static List<string> GetAllChapterLinkInManga24h(string MangaLink,bool reverse = true)
		{
			try{
				string html = DownloadString(MangaLink);
				var doc = new HtmlDocument();
				doc.LoadHtml(html);
				var list = doc.DocumentNode.Descendants("div")
					.Where(tr => tr.GetAttributeValue("class", "").Contains("table_view table_chapter"))
					.SelectMany(tr => tr.Descendants("a")).Select(a => a.Attributes["href"].Value)
					.ToList();
				list = list.Select(o=>MANGA24+o).ToList();
				if(reverse)
				{
					list.Reverse();
				}
				return list;
			}catch(Exception ex)
			{
				WriteLine(ex.Message);
				return new List<string>();
			}
		}
		
		/// <summary>
		/// validate link
		/// </summary>
		/// <param name="link"></param>
		/// <returns></returns>
		public static bool ValidateLink(string link)
		{
			Uri uriResult;
			bool returnResult = Uri.TryCreate(link, UriKind.Absolute, out uriResult) && uriResult.Scheme == Uri.UriSchemeHttp;
			return returnResult;
		}
		
		/// <summary>
		/// download html string from address
		/// </summary>
		/// <param name="address"></param>
		/// <returns></returns>
		public static string DownloadString (string address)
		{
			using(WebClient client = new WebClient ())
			{
				client.Encoding = Encoding.UTF8;
				string reply = client.DownloadString (address);
				return UnescapeXml(reply);
			}
		}
		
		/// <summary>
		/// get all image link in chapter
		/// </summary>
		/// <param name="ChapterLink"></param>
		/// <param name="volume"></param>
		/// <param name="rootPath"></param>
		/// <returns>dic[imageurl]=savepath</returns>
		public static Dictionary<string,string> GetAllImageLinksInChapterManga24(MangaChapter ChapterLink,int volume,string rootPath,int maxVolume,int maxChapter)
		{
			try{
				string html = DownloadString(ChapterLink.ChapterLink);
				var doc = new HtmlDocument();
				doc.LoadHtml(html);
				var list = doc.DocumentNode.Descendants("div")
					.Where(tr => tr.GetAttributeValue("class", "").Contains("view2"))
					.SelectMany(tr => tr.Descendants("img")).Select(a => a.Attributes["src"].Value)
					.ToList();
				
				//put image link and save path to dic
				//dic[imageurl] = savepath
				Dictionary<string,string> dic = new Dictionary<string,string>();
				for (int i = 0; i < list.Count; i++) {
					string tempRootPath = Path.Combine(rootPath,ChapterLink.MangaName);
					string path = Path.Combine(tempRootPath,ChapterLink.MangaName+"_Vol"+GetNumber(volume,maxVolume));
					dic[list[i]] = Path.Combine(path,ChapterLink.MangaName+"_Volume"+GetNumber(volume,maxVolume)+"_Chapter"+GetNumber(ChapterLink.ChapterNumber,maxChapter)+"_"+GetNumber((i+1),list.Count)+".jpg");
				}
				return dic;
			}catch(Exception ex)
			{
				WriteLine(ex.Message);
				return new Dictionary<string,string>();
			}
		}
		
		/// <summary>
		/// fill string to list object
		/// </summary>
		/// <param name="lsLink"></param>
		/// <param name="mangaName"></param>
		/// <param name="chapterPerVol"></param>
		/// <param name="rootPath"></param>
		/// <returns></returns>
		public static  List<MangaVolume> GetMangaVolumes(List<string> lsLink,string mangaName,int chapterPerVol,string rootPath)
		{
			List<MangaVolume> lsManga = new List<MangaVolume>();
			MangaVolume mangaVol = new MangaVolume(mangaName,chapterPerVol);
			int maxChapter = lsLink.Count;
			int maxVolume = ((int)maxChapter/chapterPerVol) +1;
			
			try{
				for (int i = 0; i < lsLink.Count; i++) {
					mangaVol.Volume = ((int)i/mangaVol.ChapterPerVol)+1;
					MangaChapter chapter = new MangaChapter();
					chapter.MangaName = mangaVol.MangaName;
					chapter.ChapterLink = lsLink[i];
					chapter.ChapterNumber =(i+1);
					var allImageLinks = GetAllImageLinks(chapter,mangaVol.Volume,rootPath,maxVolume,maxChapter);
					chapter.ListImageLink = allImageLinks;
					
//					string outText = string.Format("Chapter {0} has image links:{1}",chapter.ChapterNumber,
//					                               string.Join(NEWLINE,allImageLinks.Keys)+"-----"+string.Join(NEWLINE,allImageLinks.Values));
					
					string outText = string.Format("Chapter {0} has {1} image links",chapter.ChapterNumber,
					                               allImageLinks.Keys.Count);
					WriteLine(outText);
					mangaVol.Add(chapter);
					if(mangaVol.IsFull() || i==(lsLink.Count-1))
					{
//						outText = string.Format("Volume {0} has chapter links:{1}",mangaVol.Volume,string.Join(NEWLINE,mangaVol.ListChapter));
						int noChapter = mangaVol.ListChapter.Count;
						int noImage = mangaVol.ListChapter.Sum(o=>o.ListImageLink.Count);
						float average = noImage/noChapter;
						outText = string.Format("Volume {0} has {1} chapter links",mangaVol.Volume,mangaVol.ListChapter.Count);
						WriteLine(outText);
						lsManga.Add(mangaVol);
						mangaVol = new MangaVolume(mangaName,chapterPerVol);
					}
				}
			}catch(Exception e)
			{
				WriteLine(e.Message);
			}
			return lsManga;
		}
		
		/// <summary>
		/// get safe mange name from url
		/// </summary>
		/// <param name="url"></param>
		/// <returns></returns>
		public static string GetSafeMangaNameFromUrl(string url)
		{
			if(site== MangaSite.Manga24h || site== MangaSite.TruyenTranhTuan)
			{
				try{
					Uri uri = new Uri(url);
					string title = uri.Segments.LastOrDefault();
					title = title.Replace(".html","").Replace("/","").Replace("\\"," ");
					return title;
				}catch(Exception ex)
				{
					WriteLine(ex.Message);
					return "";
				}
			}else{
				try{
					Uri uri = new Uri(url);
					string title = string.Join("_",uri.Segments);
					title = title.Replace("/","").Replace("\\"," ");
					return title;
				}catch(Exception ex)
				{
					WriteLine(ex.Message);
					return "";
				}
			}
		}
		
		/// <summary>
		/// do the main job
		/// </summary>
//		public static void Do()
//		{
//			string mangaLink = "http://manga24h.com/3776/Claymore-Mat-Bac.html";
//			string rootPath = "D:\\Manga\\";
//
//			string defaultConfig = string.Format("\r\n\r\nwe will get manga from link : {0} \r\nSave path : {1}\r\nChapter per vol :{2}\r\n",mangaLink,rootPath,CHAPTER_PER_VOL);
//			WriteLine(defaultConfig);
//			WriteLine("Sleep 3s before process,please check params");
//			Thread.Sleep(3000);
//
//			string mangaName = GetSafeMangaNameFromUrl(mangaLink);
//
//
//			WriteLine("manga name : "+mangaName);
//
//			List<string> lsMangaChap = GetAllChapterLinkInManga24h(mangaLink);
//			WriteLine("All chapter link:"+ string.Join(NEWLINE,lsMangaChap)+NEWLINE);
//			var lsVol = GetMangaVolumes(lsMangaChap,mangaName,CHAPTER_PER_VOL,rootPath);
//
//			foreach (var vol in lsVol)
//			{
//				string folder = Path.Combine(rootPath,vol.MangaName+"_Vol"+vol.Volume);
//				if(!Directory.Exists(folder))
//				{
//					Directory.CreateDirectory(folder);
//				}
//			}
//
//			//get all image link and path
//			var allChapter = lsVol.Select(o=>o.ListChapter).SelectMany(x=>x).ToList();
//			var allImageList = allChapter.Select(o=>o.ListImageLink).ToList();
//
//			Parallel.ForEach(allImageList,(element)=>{
//			                 	Parallel.ForEach(element.Keys,(key)=>
//			                 	                 {
//			                 	                 	DownloadData(key,element[key]);
//			                 	                 });
//			                 });
//
//			WriteLine("ALL DONE");
//		}
		
		/// <summary>
		/// do the main job with params
		/// </summary>
		/// <param name="mangaLink"></param>
		/// <param name="rootPath"></param>
		/// <param name="chapterPerVol"></param>
		public static void Do(string mangaLink,string rootPath,int chapterPerVol=5,int maxTask=5,string pdfPath=null)
		{
			CHAPTER_PER_VOL = chapterPerVol;
			MAX_TASK = maxTask;
			site = GetSite(mangaLink);
			string defaultConfig = string.Format("\r\n\r\nwe will get manga from link : {0} \r\nSave path : {1}\r\nChapter per vol :{2}\r\nMake pdf : {3}",mangaLink,rootPath,chapterPerVol,pdfPath!=null?"true":"false");
			WriteLine(defaultConfig);
			WriteLine("Sleep 5s before process,please check params");
			Thread.Sleep(5000);
			string mangaName = GetSafeMangaNameFromUrl(mangaLink);
			mangaName = mangaName.Replace(".html","");
			WriteLine("Manga name : "+mangaName);
			
			List<string> lsMangaChap = GetAllChapterLink(mangaLink);
//			WriteLine("All chapter link:"+ string.Join(NEWLINE,lsMangaChap)+NEWLINE);
			WriteLine(string.Format("Found {0} chapters",lsMangaChap.Count));
			var lsVol = GetMangaVolumes(lsMangaChap,mangaName,CHAPTER_PER_VOL,rootPath);
			
			foreach (var vol in lsVol)
			{
				string tempRootPath = Path.Combine(rootPath,vol.MangaName);
				string folder = Path.Combine(tempRootPath,vol.MangaName+"_Vol"+GetNumber(vol.Volume,lsVol.Count));
				if(!Directory.Exists(folder))
				{
					Directory.CreateDirectory(folder);
				}
			}
			
			//get all image link and path
			var allChapter = lsVol.Select(o=>o.ListChapter).SelectMany(x=>x).ToList();
			var allImageList = allChapter.Select(o=>o.ListImageLink).ToList();
			
			ForEachAsync(allImageList,MAX_TASK,DoWork).Wait();
			
			WriteLine("ALL DONE:"+mangaLink);
			if(pdfPath!=null)
			{
				Task.Run(()=>{
				         	DirectoryInfo directory = new DirectoryInfo(Path.Combine(rootPath,mangaName));
				         	DirectoryInfo[] directories = directory.GetDirectories();
				         	var allFolders = directories.Select(o=>o.FullName).ToList();
				         	Parallel.ForEach(allFolders,(folder)=>{
				         	                 	CreatePdfFromImages(folder+".pdf",folder);
				         	                 });
				         }).ContinueWith(result=>
				                {
				                	WriteLine("Pdf creator done");
				                });
			}
		}
		
		public static List<string>  GetAllChapterLink(string mangaLink)
		{
			if(site==MangaSite.Manga24h)
			{
				return  GetAllChapterLinkInManga24h(mangaLink);
			}else if(site==MangaSite.Comicvn)
			{
				return GetChapterLinkFromComicVN(mangaLink);
			}else if(site==MangaSite.TruyenTranhTuan)
			{
				return GetChapterLinkFromTTT
					(mangaLink);
			}else{
				return new List<string>();
			}
		}
		
		public static Dictionary<string,string>  GetAllImageLinks(MangaChapter ChapterLink,int volume,string rootPath,int maxVolume,int maxChapter)
		{
			if(site==MangaSite.Manga24h)
			{
				return  GetAllImageLinksInChapterManga24(ChapterLink,volume,rootPath,maxVolume,maxChapter);
			}else if(site==MangaSite.Comicvn)
			{
				return GetImageLinkFromChapterComicVN(ChapterLink,volume,rootPath,maxVolume,maxChapter);
			}else if(site==MangaSite.TruyenTranhTuan)
			{
				return GetImageLinkFromChapterTTT(ChapterLink,volume,rootPath,maxVolume,maxChapter);
			}else{
				return new Dictionary<string,string>();
			}
		}
		
		public static MangaSite GetSite(string mangaLink)
		{
			if(mangaLink.Contains(MANGA24))
				return MangaSite.Manga24h;
			
			if(mangaLink.Contains(TTT))
				return MangaSite.TruyenTranhTuan;
			
			if(mangaLink.Contains(COMICVN))
				return MangaSite.Comicvn;
			else
				return MangaSite.None;
		}
		
		public static Task DoWork(Dictionary<string,string> element)
		{
			return ForEachAsync(element.Keys,MAX_TASK,element,DownloadData);
		}
		
		/// <summary>
		/// download images
		/// </summary>
		/// <param name="url"></param>
		/// <param name="fileName"></param>
		public static Task DownloadData(string url,string fileName)
		{
			Task downloadTask =  Task.Run(()=>{
			                              	if(!File.Exists(fileName)){
			                              		for (int i = 0; i <= RETRY;i++) {
			                              			try{
			                              				var request = WebRequest.Create(url);
			                              				request.Timeout = 15000;
			                              				using(var response = request.GetResponse())
			                              					using(var responseStream = response.GetResponseStream())
			                              						using(var result = new MemoryStream())
			                              				{
			                              					responseStream.CopyTo(result);
			                              					System.IO.File.WriteAllBytes(fileName, result.ToArray());
			                              				}
			                              				WriteLine("Downloaded "+url+" ===========>"+fileName);
			                              				break;
			                              			}catch(Exception e)
			                              			{
			                              				WriteLine("got exception when download url :"+url+" Error:"+e.Message);
			                              				if(i==RETRY)
			                              				{
			                              					WriteError(url+"===="+fileName);
			                              				}
			                              				
			                              				if(i!=0)
			                              				{
			                              					WriteLine("retry "+i+" to download "+url+"===="+fileName);
			                              				}
			                              				Thread.Sleep(1000*(i+1));
			                              			}
			                              		}
			                              	}else{
			                              		WriteLine("-----------------EXIST file " + Path.GetFileName(fileName) + " url DOWNLOADED " + url+"-------------");
			                              	}
			                              });
			return downloadTask;
		}
		
		public static void WriteLine(string input)
		{
			Console.WriteLine(input);
			log.Info(input);
		}
		
		public static void WriteError(string input)
		{
			log.Error(input);
		}
		
		public static Task ForEachAsync<T>(this IEnumerable<T> source, int dop, Func<T, Task> body)
		{
			return Task.WhenAll(
				from partition in Partitioner.Create(source).GetPartitions(dop)
				select Task.Run(async delegate {
				                	using (partition)
				                		while (partition.MoveNext())
				                			await body(partition.Current);
				                }));
		}
		
		public static Task ForEachAsync<T>(this IEnumerable<T> source, int dop,Dictionary<T,T> dic, Func<T,T,Task> body)
		{
			return Task.WhenAll(
				from partition in Partitioner.Create(source).GetPartitions(dop)
				select Task.Run(async delegate {
				                	using (partition)
				                		while (partition.MoveNext())
				                			await body(partition.Current,dic[partition.Current]);
				                }));
		}
		
		
		public static void CreatePdfFromImages(string pdfName,List<string> images)
		{
			Document doc = new Document(PageSize.A4);
			PdfWriter writer = PdfWriter.GetInstance(doc, new FileStream(pdfName, FileMode.Create));
			doc.Open();
			doc.Add(new Paragraph(pdfName.Replace(".pdf","")));
			try
			{
				foreach (var image in images) {
					if(!File.Exists(image)) continue;
					iTextSharp.text.Image jpg = iTextSharp.text.Image.GetInstance(image);
					//Resize image depend upon your need
					jpg.ScaleToFit(560f, 790f);
					//Give space before image
					jpg.SpacingBefore = 5f;
					//Give some space after the image
					jpg.SpacingAfter = 5f;
					jpg.Alignment = Element.ALIGN_CENTER;
					doc.Add(jpg);
				}
			}
			catch (Exception ex)
			{
				WriteLine(ex.Message);
			}
			finally
			{
				try{
					doc.Close();
				}catch(Exception ex)
				{
					WriteLine(ex.Message);
				}
			}
		}
		
		
		public static void CreatePdfFromImages(string pdfName,string[] images)
		{
			CreatePdfFromImages(pdfName,images.ToList());
		}
		
		
		public static void CreatePdfFromImages(string pdfName,string imageFolder)
		{
			Document doc = null;
			try
			{
				DirectoryInfo dirInfo = new DirectoryInfo(imageFolder);
				List<string> extensionArray = new List<string>(){".jpg",".bmp",".gif",".ico",".jpeg",".png",".tif",".tiff",".wmf"};
				HashSet<string> allowedExtensions = new HashSet<string>(extensionArray, StringComparer.OrdinalIgnoreCase);
				FileInfo[] files = Array.FindAll(dirInfo.GetFiles(), f => allowedExtensions.Contains(f.Extension));
				List<string> images = files.Select(o=>o.FullName).OrderBy(o=>o).ToList();
				if(images.Count<= 0)
				{
					WriteLine("Can not found any image in "+imageFolder);
					return;
				}
				//if pdfName is directory,add name to it
				if(Directory.Exists(pdfName) && !pdfName.EndsWith(".pdf"))
				{
					pdfName+=(dirInfo.Name+".pdf");
				}
				
				//if pdfName is exist,backup it
				if(File.Exists(pdfName)){
					string fileName = Path.GetFileNameWithoutExtension(pdfName);
					string extension = Path.GetExtension(pdfName);
					fileName += String.Format("{0:s}", DateTime.Now).Replace(":","-");
					string backup = Path.Combine(Path.GetDirectoryName(pdfName),fileName)+ extension;
					File.Move(pdfName,backup);
					WriteLine("file " + pdfName + " exist,move to backup " + backup);
				}
				
				doc = new Document(PageSize.A4);
				PdfWriter writer = PdfWriter.GetInstance(doc, new FileStream(pdfName, FileMode.Create));
				doc.Open();
				doc.Add(new Paragraph(pdfName.Replace(".pdf","")));
				
				foreach (var image in images) {
					if(!File.Exists(image)) continue;
					iTextSharp.text.Image jpg = iTextSharp.text.Image.GetInstance(image);
					//Resize image depend upon your need
					jpg.ScaleToFit(560f, 790f);
					//Give space before image
					jpg.SpacingBefore = 5f;
					//Give some space after the image
					jpg.SpacingAfter = 5f;
					jpg.Alignment = Element.ALIGN_CENTER;
					doc.Add(jpg);
				}
				WriteLine(string.Format("Add {0} images to {1}",images.Count,pdfName));
			}
			catch (Exception ex)
			{
				WriteLine(ex.Message);
			}
			finally
			{
				try{
					if(doc!=null){
						doc.Close();
					}
					WriteLine("finish create pdf :"+pdfName);
				}catch(Exception ex)
				{
					WriteLine(ex.Message);
				}
			}
		}
		
		public static string GetNumber(int input,int reference)
		{
			string zero="";
			for (int i = 0; i < reference.ToString().Length; i++) {
				zero+="0";
			}
			return input.ToString(zero);
		}
		
		public static void MakePdfFromFolders(string parentFolder,string savePath)
		{
			DirectoryInfo directory = new DirectoryInfo(parentFolder);
			DirectoryInfo[] directories = directory.GetDirectories();
			var allFolders = directories.Select(o=>o.FullName).ToList();
			WriteLine("Starting create pdfs");
			WriteLine("All folders:");
			WriteLine(string.Join(NEWLINE,allFolders));
			Task.Run(()=>{
			         	
			         	Parallel.ForEach(allFolders,(folder)=>{
			         	                 	CreatePdfFromImages(folder+".pdf",folder);
			         	                 });
			         }).ContinueWith(result=>
			                {
			                	WriteLine("Pdf creator done,please check "+parentFolder);
			                });
		}
		
		
		
		
		public static List<string> GetChapterLinkFromTTT(string MangaLink,bool reverse=true)
		{
			try{
				string html = DownloadString(MangaLink);
				var doc = new HtmlDocument();
				doc.LoadHtml(html);
				var list = doc.DocumentNode.Descendants("div")
					.Where(tr => tr.GetAttributeValue("id", "").Contains("manga-chapter"))
					.SelectMany(tr => tr.Descendants("a")).Select(a => a.Attributes["href"].Value)
					.ToList();
				if(reverse)
				{
					list.Reverse();
				}
//				WriteLine(string.Join("\r\n",list));
				return list;
			}catch(Exception ex)
			{
				WriteLine(ex.Message);
				return new List<string>();
			}
		}
		
		
		public static Dictionary<string,string> GetImageLinkFromChapterTTT(MangaChapter ChapterLink,int volume,string rootPath,int maxVolume,int maxChapter)
		{
			string html = DownloadString(ChapterLink.ChapterLink);
			string SLIDEPAGE= "var slides_page";
			string LENGTH_CHAPTER ="length_chapter =";
			HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
			doc.LoadHtml(html);
			var a =doc.DocumentNode.Descendants()
				.Where(n => n.Name == "script");
			var b = a.Select(o=>o.OuterHtml).Where(o=>o.Contains(SLIDEPAGE)).ToList();
			var c = b.FirstOrDefault();
			if(c!=null)
			{
				var d = c.IndexOf(SLIDEPAGE);
				var e = c.IndexOf(LENGTH_CHAPTER);
				var f = c.Substring(d+SLIDEPAGE.Length,(e-d-LENGTH_CHAPTER.Length-10));
				var g = f.Replace("=","").Replace("[","").Replace("]","").Replace("\"","").Replace(" ","").Replace(";","");
				var list = g.Split(new char[]{','},StringSplitOptions.RemoveEmptyEntries).OrderBy(o=>o).Select(o=> o.Replace("varslides_page_path","")).ToList();
//				WriteLine(string.Join("\r\n",h));
//				return h;
				
				Dictionary<string,string> dic = new Dictionary<string,string>();
				for (int i = 0; i < list.Count; i++) {
					string tempRootPath = Path.Combine(rootPath,ChapterLink.MangaName);
					string path = Path.Combine(tempRootPath,ChapterLink.MangaName+"_Vol"+GetNumber(volume,maxVolume));
					dic[list[i]] = Path.Combine(path,ChapterLink.MangaName+"_Volume"+GetNumber(volume,maxVolume)+"_Chapter"+GetNumber(ChapterLink.ChapterNumber,maxChapter)+"_"+GetNumber((i+1),list.Count)+".jpg");
				}
				return dic;
			}
			return new  Dictionary<string,string>();
		}
		
		
		public static List<string> GetChapterLinkFromComicVN(string MangaLink,bool reverse=true)
		{
			try{
				string html = DownloadString(MangaLink);
				var doc = new HtmlDocument();
				doc.LoadHtml(html);
				var list = doc.DocumentNode.Descendants("div")
					.Where(tr => tr.GetAttributeValue("class", "").Contains("warp_box_ch"))
					.SelectMany(tr => tr.Descendants("a")).Select(a => a.Attributes["href"].Value)
					.ToList();
				if(reverse)
				{
					list.Reverse();
				}
				list = list.Select(o=>COMICVN+o).ToList();
//				WriteLine(string.Join("\r\n",list));
				return list;
			}catch(Exception ex)
			{
				WriteLine(ex.Message);
				return new List<string>();
			}
		}
		
		
		public static Dictionary<string,string> GetImageLinkFromChapterComicVN(MangaChapter ChapterLink,int volume,string rootPath,int maxVolume,int maxChapter)
		{
			string html = DownloadString(ChapterLink.ChapterLink);
			var doc = new HtmlDocument();
			doc.LoadHtml(html);
			var list = doc.DocumentNode.Descendants("textarea")
				.Where(tr => tr.GetAttributeValue("id", "").Contains("txtarea"))
				.SelectMany(tr => tr.Descendants("img")).Select(a => a.Attributes["src"].Value)
				.ToList();
			if(list.Count <10)
			{
				Console.WriteLine(ChapterLink+":"+list.Count);
				string t = HttpUtility.HtmlDecode(html);
			}
//			WriteLine(string.Join("\r\n",list));
//			return list;
			//put image link and save path to dic
			//dic[imageurl] = savepath
			Dictionary<string,string> dic = new Dictionary<string,string>();
			for (int i = 0; i < list.Count; i++) {
				string tempRootPath = Path.Combine(rootPath,ChapterLink.MangaName);
				string path = Path.Combine(tempRootPath,ChapterLink.MangaName+"_Vol"+GetNumber(volume,maxVolume));
				dic[list[i]] = Path.Combine(path,ChapterLink.MangaName+"_Volume"+GetNumber(volume,maxVolume)+"_Chapter"+GetNumber(ChapterLink.ChapterNumber,maxChapter)+"_"+GetNumber((i+1),list.Count)+".jpg");
			}
			return dic;
			
		}
		
		public static void Test(string ChapterLink)
		{
			string html = DownloadString(ChapterLink);
			var doc = new HtmlDocument();
			doc.LoadHtml(html);
			var list = doc.DocumentNode.Descendants("textarea")
				.Where(tr => tr.GetAttributeValue("id", "").Contains("txtarea"))
				.SelectMany(tr => tr.Descendants("img")).Select(a => a.Attributes["src"].Value)
				.ToList();
			if(list.Count <10)
			{
				Console.WriteLine(ChapterLink+":"+list.Count);
				string t = UnescapeXml(html);
				doc.LoadHtml(t);
				list = doc.DocumentNode.Descendants("textarea")
					.Where(tr => tr.GetAttributeValue("id", "").Contains("txtarea"))
					.SelectMany(tr => tr.Descendants("img")).Select(a => a.Attributes["src"].Value)
					.ToList();
				if(list.Count<10)
				{
					Console.WriteLine();
				}
			}
		}
		
		public static string UnescapeXml(string s)
		{
			string unxml = WebUtility.HtmlDecode(s);
			if ( !string.IsNullOrEmpty( unxml ) )
			{
				// replace entities with literal values
				unxml = unxml.Replace( "&apos;", "'" );
				unxml = unxml.Replace( "&quot;", "\"" );
				unxml = unxml.Replace( "&gt;", ">;" );
				unxml = unxml.Replace( "&lt;", "<" );
				unxml = unxml.Replace( "&amp;", "&" );
			}
			return unxml;
		}
		
		public enum MangaSite
		{
			None,
			Manga24h,
			TruyenTranhTuan,
			Comicvn
		}
	}
	
}
