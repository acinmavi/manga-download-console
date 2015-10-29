/*
 * Created by SharpDevelop.
 * User: Admin15
 * Date: 27/11/2013
 * Time: 1:24 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Timers;

namespace MangaDownloadConsole
{
	class Program
	{
		public static void Main(string[] args)
		{
			//add timer
			System.Timers.Timer aTimer = new System.Timers.Timer();
			aTimer.Elapsed+=new ElapsedEventHandler(OnTimedEvent);
			aTimer.Interval=60*60*1000;
			aTimer.Enabled=true;
			
			
//			log4net.Config.BasicConfigurator.Configure();
			Utilities.WriteLine("Hello user,USAGE :");
			Utilities.WriteLine("MangaDownloadConsole.exe -link=\"(Manga_link Manga_link)\" -path=(Save_Path) -vol=[ChapterPerVol] -max=[max task run an instance] -pdf=[true or false]");
			Utilities.WriteLine("when () means require,[] means options.Example:");
			string example = "MangaDownloadConsole.exe -link=\"http://manga24h.com/1648/Puppy-Lovers.html http://manga24h.com/3621/A-bout.html\" -path=D:\\Manga\\ -vol=10 -max=10 -pdf=true -startChap=10 -endChap=20" +
				"OR -add=\"http://manga24h.com/1648/Puppy-Lovers.html http://manga24h.com/3621/A-bout.html\" to update or add to database";
			Utilities.WriteLine(example);
			Arguments CommandLine=new Arguments(args);
			while(true){
				bool linkDefine = false;
				bool pathDefine  = false;
				bool ChapinVolDefine = false;
				bool MaxTaskDefine = false;
				bool pdfDefine = false;
				string cmdLink = CommandLine["link"];
				string cmdPath = CommandLine["path"];
				string cmdVol = CommandLine["vol"];
				string cmdMaxTask = CommandLine["max"];
				string cmdPdf = CommandLine["pdf"];
				string startChap = CommandLine["startChap"];
				string endChap = CommandLine["endChap"];
				string addLinks = CommandLine["add"];
				bool manualLink = false;
				// Create new stopwatch
				Stopwatch stopwatch = new Stopwatch();
				// Begin timing
				stopwatch.Start();
				if(addLinks!=null){
					List<string> listLink = addLinks.Split(new char[]{' '},StringSplitOptions.RemoveEmptyEntries).ToList();
					if(listLink.Count > 0){
						Database.ManualInsert(listLink);
					}
					manualLink = true;
				}else{
					
					if(cmdLink != null){
						Utilities.WriteLine("link value: " +
						                    cmdLink);
						linkDefine = true;
						manualLink = true;
					}
					else
						Utilities.WriteLine("link not defined,will get from db!");
					
					if(cmdPath != null){
						Utilities.WriteLine("path value: " +
						                    cmdPath);
					}
					else {
						cmdPath = "D:\\Manga\\";
					}
					pathDefine = true;
					
					if(cmdVol != null){
						Utilities.WriteLine("chapter per vol value: " +
						                    cmdVol);
					}
					else {
						cmdVol = "20";
					}
					ChapinVolDefine = true;
					
					if(cmdMaxTask != null){
						Utilities.WriteLine("max task value: " +
						                    cmdMaxTask);
						
					}else {
						cmdMaxTask = "2";
					}
					MaxTaskDefine = true;
					
					if(cmdPdf != null){
						Utilities.WriteLine("make pdf : " +
						                    cmdPdf);
						
					}
					else{
						cmdPdf="true";
					}
					pdfDefine = true;
					
					List<string> listLink = new List<string>();
					if(linkDefine)
					{
						listLink = cmdLink.Split(new char[]{' '},StringSplitOptions.RemoveEmptyEntries).ToList();
					}
					bool getFromDb = false;
					if(!linkDefine || listLink.Count == 0){
						listLink=Database.GetAllActiveManga();
						linkDefine = true;
						getFromDb = true;
					}
					
					if(linkDefine && pathDefine && ChapinVolDefine && MaxTaskDefine && listLink.Count>0)
					{
						foreach (var link in listLink) {
							Utilities.Do(link,cmdPath,int.Parse(cmdVol),int.Parse(cmdMaxTask),cmdPdf,startChap,endChap,getFromDb);
						}
					}else if(linkDefine && pathDefine && ChapinVolDefine && listLink.Count>0)
					{
						foreach (var link in listLink) {
							Utilities.Do(link,cmdPath,int.Parse(cmdVol),5,cmdPdf,startChap,endChap,getFromDb);
						}
					}else if(linkDefine && pathDefine && listLink.Count>0)
					{
						foreach (var link in listLink) {
							Utilities.Do(link,cmdPath,10,5,cmdPdf,startChap,endChap,getFromDb);
						}
						
					}else if(pathDefine && pdfDefine&&(!linkDefine))
					{
						Utilities.MakePdfFromFolders(cmdPath,cmdPdf);
					}else{
						if(listLink.Count <= 0){
							Utilities.WriteLine("no more active manga on database");
						}else{
							Utilities.WriteLine("Invalid params,see usage");
						}
					}
				}
				// Stop timing
				stopwatch.Stop();

				// Write result
				Utilities.WriteLine( string.Format("Time elapsed: {0}",
				                                   stopwatch.Elapsed));
				
				if(manualLink){
					break;
				}
				
				Thread.Sleep(5000);
				
			}
			Console.ReadLine();
		}
		
		private static void OnTimedEvent(object source, ElapsedEventArgs e)
		{
			//find all manga inactive and update number of chapter and activeFlg if needed
			List<MangaLink> allInActiveManga  = Database.GetAllMangaLink(false);
			if(allInActiveManga.Count <=0) return;
			foreach (var mangaLink in allInActiveManga) {
				List<string> lsMangaChap = Utilities.GetAllChapterLink(mangaLink.MangaName);
				if(mangaLink.NumberOfChapter != lsMangaChap.Count){
					mangaLink.NumberOfChapter = lsMangaChap.Count;
					mangaLink.IsActive = true;
					Database.Update(mangaLink);
				}
			}
		}
	}
}