/*
 * Created by SharpDevelop.
 * User: Admin15
 * Date: 27/11/2013
 * Time: 1:24 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Diagnostics;

namespace MangaDownloadConsole
{
	class Program
	{
		public static void Main(string[] args)
		{
//			log4net.Config.BasicConfigurator.Configure();
			Utilities.WriteLine("Hello user,USAGE :");
			Utilities.WriteLine("MangaDownloadConsole.exe -link=\"(Manga_link Manga_link)\" -path=(Save_Path) -vol=[ChapterPerVol] -max=[max task run an instance] -pdf=[true or false]");
			Utilities.WriteLine("when () means require,[] means options.Example:");
			string example = "MangaDownloadConsole.exe -link=\"http://manga24h.com/1648/Puppy-Lovers.html http://manga24h.com/3621/A-bout.html\" -path=D:\\Manga\\ -vol=10 -max=10 -pdf=true";
			Utilities.WriteLine(example);
			// Create new stopwatch
			Stopwatch stopwatch = new Stopwatch();
			// Begin timing
			stopwatch.Start();
			Arguments CommandLine=new Arguments(args);
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
			if(cmdLink != null){
				Utilities.WriteLine("link value: " +
				                    cmdLink);
				linkDefine = true;
			}
			else
				Utilities.WriteLine("link not defined !");
			
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
			
			string[] listLink = null;
			if(linkDefine)
			{
				listLink = cmdLink.Split(new char[]{' '},StringSplitOptions.RemoveEmptyEntries);
			}
			
			if(linkDefine && pathDefine && ChapinVolDefine && MaxTaskDefine && listLink.Length>0)
			{
				foreach (var link in listLink) {
					Utilities.Do(link,cmdPath,int.Parse(cmdVol),int.Parse(cmdMaxTask),cmdPdf);
				}
			}else if(linkDefine && pathDefine && ChapinVolDefine && listLink.Length>0)
			{
				foreach (var link in listLink) {
					Utilities.Do(link,cmdPath,int.Parse(cmdVol),5,cmdPdf);
				}
			}else if(linkDefine && pathDefine && listLink.Length>0)
			{
				foreach (var link in listLink) {
					Utilities.Do(link,cmdPath,10,5,cmdPdf);
				}
				
			}else if(pathDefine && pdfDefine&&(!linkDefine))
			{
				Utilities.MakePdfFromFolders(cmdPath,cmdPdf);
			}else{
				Utilities.WriteLine("Invalid params,see usage");
			}
			// Stop timing
			stopwatch.Stop();

			// Write result
			Utilities.WriteLine( string.Format("Time elapsed: {0}",
			                                   stopwatch.Elapsed));
			Console.Write("Press any key to continue . . . ");
			Console.ReadKey(true);
		}
	}
}