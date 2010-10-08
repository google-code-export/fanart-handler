//-----------------------------------------------------------------------
// Open Source software licensed under the GNU/GPL agreement.
// 
// Author: Cul8er
//-----------------------------------------------------------------------

namespace FanartHandler
{
    using MediaPortal.ExtensionMethods;
    using System.Drawing;
    using System.Drawing.Imaging;
    using MediaPortal.Util;
    using MediaPortal.Configuration;
    using NLog;
    using System;    
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Management;
    using System.Net;
    using System.Text;    
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Web;
    using System.Xml;
    using System.Xml.XPath;

    /// <summary>
    /// Class handling scraping of fanart from htbackdrops.com.
    /// </summary>
    class Scraper
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private Random randNumber = new Random();
        private WebProxy proxy = null;
        private ArrayList alSearchResults = null; 
        

        /// <summary>
        /// Scrapes the "new" pages on htbackdrops.com.
        /// </summary>
        public void GetNewImages(int iMax, DatabaseManager dbm)
        {
            try
            {
                Encoding enc = Encoding.GetEncoding("iso-8859-1"); 
                logger.Info("Scrape for new images is starting...");
                string dbArtist = null;
                string strResult = null;
                string path = null;
                //bool foundThumb = false;
                bool foundNewImages = false;
                string filename = null;
                string sTimestamp = dbm.GetTimeStamp("Fanart Handler Last Scrape");
                if (sTimestamp == null || sTimestamp.Length <= 0)
                {
                    sTimestamp = "2010-09-13 01:00:00";
                }

                HttpWebRequest objRequest = (HttpWebRequest)WebRequest.Create("http://htbackdrops.com/api/02274c29b2cc898a726664b96dcc0e76/searchXML?");
                if (Utils.GetUseProxy() != null && Utils.GetUseProxy().Equals("True"))
                {
                    proxy = new WebProxy(Utils.GetProxyHostname() + ":" + Utils.GetProxyPort());
                    proxy.Credentials = new NetworkCredential(Utils.GetProxyUsername(), Utils.GetProxyPassword(), Utils.GetProxyDomain());
                    objRequest.Proxy = proxy;
                }
                objRequest.ServicePoint.Expect100Continue = false;
                string values = "keywords=";
                values += "&aid=1,5";
                values += "&limit=500";
                values += "&modified_since=" + Utils.ConvertToTimestamp(DateTime.ParseExact(sTimestamp, "yyyy-MM-dd HH:mm:ss", null));
                objRequest.Method = "POST";
                objRequest.ContentType = "application/x-www-form-urlencoded"; 
                objRequest.ContentLength = values.Length;
                using (StreamWriter writer = new StreamWriter(objRequest.GetRequestStream(),enc))
                {
                    writer.Write(values);
                }

                WebResponse objResponse = objRequest.GetResponse();                
                using (StreamReader sr = new StreamReader(objResponse.GetResponseStream()))
                {
                    strResult = sr.ReadToEnd();
                    sr.Close();
                }

                objResponse.Close();
                HttpWebRequest requestPic = null;
                WebResponse responsePic = null;
                string sArtist = null;
                string sourceFilename = null;
                if (strResult != null && strResult.Length > 0)
                {                    
                    XmlDocument xmlSearchResult = new XmlDocument();
                    xmlSearchResult.LoadXml(strResult);
                    XPathNavigator nav = xmlSearchResult.CreateNavigator();
                    nav.MoveToRoot();
                    alSearchResults = new ArrayList();
                    if (nav.HasChildren)
                    {
                        nav.MoveToFirstChild();
                        GetNodeInfo(nav);
                    }
                }

                logger.Info("Found " + alSearchResults.Count + " new images on htbackdrops.com");
                int iCount = 0;                    
                int iCountThumb = 0;
                dbm.TotArtistsBeingScraped = alSearchResults.Count;                    
                dbm.CurrArtistsBeingScraped = 0;
                if (FanartHandlerSetup.MyScraperNowWorker != null)
                {
                    FanartHandlerSetup.MyScraperNowWorker.ReportProgress(0, "Ongoing");
                }
                for (int x = 0; x < alSearchResults.Count; x++)
                {
                    if (dbm.StopScraper == true)
                    {
                        break;
                    }
                    foundNewImages = true;
                    sArtist = Utils.RemoveResolutionFromArtistName(((SearchResults)alSearchResults[x]).title);
                    sArtist = sArtist.Trim();                    
                    dbArtist = Utils.GetArtist(sArtist, "MusicFanart");
                    sArtist = Utils.RemoveResolutionFromArtistName(sArtist);
                    dbArtist = Utils.RemoveResolutionFromArtistName(dbArtist);
                    iCount = dbm.GetArtistCount(sArtist, dbArtist);
                    if (iCount != 999 && iCount < iMax)
                    {
                        if (((SearchResults)alSearchResults[x]).album.Equals("1"))
                        {
                            //Artist Fanart
                            logger.Debug("Matched fanart for artist " + sArtist + ".");
                            sourceFilename = "http://htbackdrops.com/api/02274c29b2cc898a726664b96dcc0e76/download/" + ((SearchResults)alSearchResults[x]).id + "/fullsize";
                            if (dbm.SourceImageExist(dbArtist, ((SearchResults)alSearchResults[x]).id, "MusicFanart") == false)
                            {
                                if (DownloadImage(ref dbArtist, ref sourceFilename, ref path, ref filename, ref requestPic, ref responsePic, "MusicFanart"))
                                {
                                    iCount = iCount + 1;
                                    dbm.LoadMusicFanart(dbArtist, filename, ((SearchResults)alSearchResults[x]).id, "MusicFanart", 0);
                                }
                            }
                            else
                            {
                                logger.Debug("Will not download fanart image as it already exist an image in your fanart database with this source image name.");
                            }
                        }                       
                    }
                    else
                    {
                        if (iCount == 999)
                        {
                            //                  logger.Debug("Artist not in your fanart database. Will not download fanart.");
                        }
                        else
                        {
                            logger.Debug("Artist " + sArtist + " has already maximum number of images. Will not download anymore images for this artist.");
                        }
                    }
                    iCountThumb = dbm.GetArtistThumbsCount(sArtist, dbArtist);
                    if (iCountThumb == 0)
                    {
                        if (((SearchResults)alSearchResults[x]).album.Equals("5"))// && !foundThumb)
                        {
                            //Artist Thumbnail
                            logger.Debug("Found thumbnail for artist " + sArtist + ".");
                            sourceFilename = "http://htbackdrops.com/api/02274c29b2cc898a726664b96dcc0e76/download/" + ((SearchResults)alSearchResults[x]).id + "/fullsize";
                            if (DownloadImage(ref sArtist, ref sourceFilename, ref path, ref filename, ref requestPic, ref responsePic, "MusicThumbs"))
                            {
                                dbm.LoadMusicFanart(dbArtist, filename, ((SearchResults)alSearchResults[x]).id, "MusicThumbnails", 0);                               
                                dbm.SetSuccessfulScrapeThumb(dbArtist, 2);
                                //foundThumb = true;
                            }

                        }
                    }
                    else
                    {
                        if (iCountThumb == 999)
                        {
                            //                  logger.Debug("Artist not in your fanart database. Will not download fanart.");
                        }
                        else
                        {
                            logger.Debug("Artist " + sArtist + " has already a thumbnail downloaded. Will not download anymore thumbnails for this artist.");
                        }
                    }

                    dbm.CurrArtistsBeingScraped++;
                    if (dbm.TotArtistsBeingScraped > 0 && FanartHandlerSetup.MyScraperNowWorker != null)
                    {
                        FanartHandlerSetup.MyScraperNowWorker.ReportProgress(Convert.ToInt32((dbm.CurrArtistsBeingScraped / dbm.TotArtistsBeingScraped) * 100), "Ongoing");
                    }                    
                }
                if (!foundNewImages)
                {
                    logger.Info("Found no new images on htbackdrops.com");
                }
                //if (!foundThumb)
                //{
                //    dbm.SetSuccessfulScrapeThumb(dbArtist, 1);
                //}
                if (alSearchResults != null)
                {
                    alSearchResults.Clear();
                }
                alSearchResults = null;
                objRequest = null;
                Utils.GetDbm().SetTimeStamp("Fanart Handler Last Scrape", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                logger.Info("Scrape for new images is done.");
            }
            catch (Exception ex)
            {
                if (alSearchResults != null)
                {
                    alSearchResults = null;
                }
                alSearchResults.Clear();
                alSearchResults = null;
                logger.Error("GetNewImages: " + ex.ToString());  
            }
        }        
        

        /// <summary>
        /// Downloads and saves images from htbackdrops.com.
        /// </summary>
        private bool DownloadImage(ref string sArtist, ref string sourceFilename, ref string path, ref string filename, ref HttpWebRequest requestPic, ref WebResponse responsePic, string type)
        {
            int maxCount = 0;
            long position = 0;
            string status = "Resume";

            if (type.Equals("MusicThumbs"))
            {
                path = Config.GetFolder(Config.Dir.Thumbs) + @"\Music\Artists";
                filename = path + @"\" + sArtist + "L.jpg";
                logger.Debug("Downloading tumbnail for " + sArtist + " (" + filename + ").");
            }
            else
            {
                path = Config.GetFolder(Config.Dir.Thumbs) + @"\Skin FanArt\music";
                filename = path + @"\" + Utils.PatchFilename(sArtist) + " (" + randNumber.Next(10000, 99999) + ").jpg";
                logger.Debug("Downloading fanart for " + sArtist + " (" + filename + ").");
            }
            
            

            while (status.Equals("Success") == false && status.Equals("Stop") == false && maxCount < 10)
            {
                Stream webStream = null;
                FileStream fileStream = null;
                status = "Success";
                try
                {
                    if (type.Equals("MusicThumbs"))
                    {
                        if (File.Exists(filename))
                        {
                            try
                            {
                                MediaPortal.Util.Utils.FileDelete(filename);
                            }
                            catch (Exception ex)
                            {
                                logger.Error("DownloadImage: Error deleting old thumbnail - {0}", ex.Message);
                            }
                            if (File.Exists(filename)) position = new FileInfo(filename).Length;
                        }
                    }
                    else
                    {
                        if (File.Exists(filename)) position = new FileInfo(filename).Length;
                    }                    
                    maxCount++;                    
                    requestPic = (HttpWebRequest)WebRequest.Create(sourceFilename);
                    requestPic.ServicePoint.Expect100Continue = false;
                    if (Utils.GetUseProxy() != null && Utils.GetUseProxy().Equals("True"))
                    {
                        requestPic.Proxy = proxy;
                    }
                    requestPic.AddRange((int)position);
                    requestPic.Timeout = 5000 + (1000 * maxCount);
                    requestPic.ReadWriteTimeout = 20000;
                    requestPic.UserAgent = "Mozilla/5.0 (Windows; U; MSIE 7.0; Windows NT 6.0; en-US)";
                    responsePic = requestPic.GetResponse();
                    if (position == 0)
                    {
                        fileStream = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.None);
                    }
                    else
                    {
                        fileStream = new FileStream(filename, FileMode.Append, FileAccess.Write, FileShare.None);
                    }
                    webStream = responsePic.GetResponseStream();

                    // setup our tracking variables for progress
                    int bytesRead = 0;
                    long totalBytesRead = 0;
                    long totalBytes = responsePic.ContentLength + position;

                    // download the file and progressively write it to disk
                    byte[] buffer = new byte[2048];
                    bytesRead = webStream.Read(buffer, 0, buffer.Length);
                    while (bytesRead > 0)
                    {
                        // write to our file
                        fileStream.Write(buffer, 0, bytesRead);
                        totalBytesRead = fileStream.Length;
                        // read the next stretch of data
                        bytesRead = webStream.Read(buffer, 0, buffer.Length);
                    }
                    // if the downloaded ended prematurely, close the stream but save the file
                    // for resuming
                    if (fileStream.Length != totalBytes)
                    {
                        fileStream.Close();
                        fileStream = null;
                        status = "Resume";
                    }
                    else
                    {
                        if (fileStream != null)
                        {
                            fileStream.Close();
                            fileStream = null;
                        }
                    }
                    if (!IsFileValid(filename))
                    {
                        status = "Stop";
                        logger.Error("DownloadImage: Deleting downloaded file because it is corrupt.");
                    }
                    if (type.Equals("MusicThumbs"))
                    {
                        File.SetAttributes(filename, File.GetAttributes(filename) | FileAttributes.Hidden);
                        CreateThumbnail(filename);
                    }
                }
                catch (System.Runtime.InteropServices.ExternalException ex)
                {
                    status = "Stop";
                    logger.Error("DownloadImage: " + ex.ToString());
                }
                catch (UriFormatException ex)
                {
                    status = "Stop";
                    logger.Error("DownloadImage: " + ex.ToString());
                }
                catch (WebException ex)
                {                    
                    if (ex.Message.Contains("404"))
                    {
                        // file doesnt exist
                        status = "Stop";
                        logger.Error("DownloadImage: " + ex.ToString());
                    }                    
                    else
                    {
                        // timed out or other similar error
                        status = "Resume";
                        if (maxCount >= 10)
                        {
                            logger.Error("DownloadImage: " + ex.ToString());
                        }
                    }

                }
                catch (ThreadAbortException ex)
                {
                    // user is shutting down the program
                    fileStream.Close();
                    fileStream = null;
                    if (File.Exists(filename)) File.Delete(filename);
                    status = "Stop";
                    logger.Error("DownloadImage: " + ex.ToString());
                }
                catch (Exception ex)
                {
                    logger.Error("DownloadImage: " + ex.ToString());
                    status = "Stop";
                }

                // if we failed delete the file
                if (fileStream != null && status.Equals("Stop"))
                {
                    fileStream.Close();
                    fileStream = null;
                    if (File.Exists(filename)) File.Delete(filename);
                }
                if (webStream != null) webStream.Close();
                if (fileStream != null) fileStream.Close();
                if (responsePic != null) responsePic.Close();
                if (requestPic != null) requestPic.Abort();
            }
            if (status.Equals("Success") == false)
            {
                if (File.Exists(filename))
                    File.Delete(filename);
            }
            if (status.Equals("Success"))
                return true;
            else
                return false;
        }

        public static bool CreateThumbnail(string aInputFilename)
        {
            Bitmap OrigImg = null;
            Bitmap NewImg = null;
            string aThumbTargetPath = aInputFilename.Substring(0, aInputFilename.IndexOf("L.jpg")) + ".jpg";
            try
            {
                int iWidth = 75;
                int iHeight = 75;

                try
                {
                    MediaPortal.Util.Utils.FileDelete(aThumbTargetPath);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("CreateThumbnail: Error deleting old thumbnail - {0}", ex.Message);
                }

                OrigImg = (System.Drawing.Bitmap)Image.FromFile(aInputFilename).Clone();
                NewImg = new Bitmap(iWidth, iHeight);

                using (Graphics g = Graphics.FromImage((Image)NewImg))
                {
                    g.CompositingQuality = Thumbs.Compositing;
                    g.InterpolationMode = Thumbs.Interpolation;
                    g.SmoothingMode = Thumbs.Smoothing;
                    g.DrawImage(OrigImg, new Rectangle(0, 0, iWidth, iHeight));
                }

                return SaveThumbnail(aThumbTargetPath, NewImg);
            }
            catch (Exception ex)
            {
                logger.Debug("CreateThumbnail: " + ex.ToString());
                return false;
            }
            finally
            {
                if (OrigImg != null)
                    OrigImg.SafeDispose();
                if (NewImg != null)
                    NewImg.SafeDispose();
            }

        }

        public static bool SaveThumbnail(string aThumbTargetPath, Image myImage)
        {
            try
            {
                using (FileStream fs = new FileStream(aThumbTargetPath, FileMode.Create, FileAccess.ReadWrite, FileShare.None))
                {
                    using (Bitmap bmp = new Bitmap(myImage))
                    {
                        bmp.Save(fs, Thumbs.ThumbCodecInfo, Thumbs.ThumbEncoderParams);
                    }
                    fs.Flush();
                }

                File.SetAttributes(aThumbTargetPath, File.GetAttributes(aThumbTargetPath) | FileAttributes.Hidden);
                // even if run in background thread wait a little so the main process does not starve on IO
                if (MediaPortal.Player.g_Player.Playing)
                    Thread.Sleep(100);
                else
                    Thread.Sleep(1);
                return true;
            }
            catch (Exception ex)
            {
                logger.Debug("SaveThumbnail: Error saving new thumbnail {0} - {1}", aThumbTargetPath, ex.Message);
                return false;
            }
        }


        private bool IsFileValid(string filename)
        {
            if (filename == null) return false;
            Image checkImage = null;
            try
            {
                checkImage = Image.FromFile(filename);
                if (checkImage != null && checkImage.Width > 0)
                {
                    checkImage.Dispose();
                    checkImage = null;
                    return true;
                }

            }
            catch
            {
                checkImage = null;
            }
            return false;
        }

        private void GetNodeInfo(XPathNavigator nav1)
        {
            if (nav1 != null && nav1.Name != null && nav1.Name.ToString().Equals("images"))
            {                
                XmlReader reader = nav1.ReadSubtree();
                SearchResults sr = new SearchResults();
                while (reader.Read())
                {                    
                    if (reader.NodeType == XmlNodeType.Element)
                    {
                        switch (reader.Name)
                        {
                            case "id":
                                sr = new SearchResults();
                                sr.id = reader.ReadString();
                                break;
                            case "album":
                                sr.album = reader.ReadString();
                                break;
                            case "title":
                                sr.title = reader.ReadString();
                                break;
                            case "votes":
                                alSearchResults.Add(sr);
                                break;
                        }
                    }
                }                
                reader.Close();
            }

            if (nav1.HasChildren)
            {
                nav1.MoveToFirstChild();
                while (nav1.MoveToNext())
                {
                    GetNodeInfo(nav1);
                    nav1.MoveToParent();
                }
            }
            else
            {
                if (nav1.MoveToNext())
                {
                    GetNodeInfo(nav1);
                }
            }
        }

        /// <summary>
        /// Scrapes image for a specific artist on htbackdrops.com.
        /// </summary>
        public int GetImages(string artist, int iMax, DatabaseManager dbm, bool reportProgress)
        {
            try
            {
                Encoding enc = Encoding.GetEncoding("iso-8859-1"); 
                string dbArtist = null;
                string strResult = null;
                string path = null;
                bool foundThumb = false;
                string filename = null;
                HttpWebRequest objRequest = (HttpWebRequest)WebRequest.Create("http://htbackdrops.com/api/02274c29b2cc898a726664b96dcc0e76/searchXML?");
                if (Utils.GetUseProxy() != null && Utils.GetUseProxy().Equals("True"))
                {
                    proxy = new WebProxy(Utils.GetProxyHostname() + ":" + Utils.GetProxyPort());
                    proxy.Credentials = new NetworkCredential(Utils.GetProxyUsername(), Utils.GetProxyPassword(), Utils.GetProxyDomain());
                    objRequest.Proxy = proxy;
                }
                objRequest.ServicePoint.Expect100Continue = false;                                 
                string values = "keywords=" + artist;
                values += "&aid=1,5";
                values += "&default_operator=and";
                objRequest.Method = "POST";
                objRequest.ContentType = "application/x-www-form-urlencoded";
                objRequest.ContentLength = values.Length;
                using (StreamWriter writer = new StreamWriter(objRequest.GetRequestStream(),enc))
                {
                    writer.Write(values);
                }
                WebResponse objResponse = objRequest.GetResponse();
                using (StreamReader sr = new StreamReader(objResponse.GetResponseStream()))
                {
                    strResult = sr.ReadToEnd();
                    sr.Close();
                }
                objResponse.Close();
                HttpWebRequest requestPic = null;
                WebResponse responsePic = null;
                string sArtist = null;
                string sourceFilename = null;                
                if (strResult != null && strResult.Length > 0)
                {
                    XmlDocument xmlSearchResult = new XmlDocument();
                    xmlSearchResult.LoadXml(strResult);
                    XPathNavigator nav = xmlSearchResult.CreateNavigator();
                    nav.MoveToRoot();
                    alSearchResults = new ArrayList();
                    if (nav.HasChildren)
                    {
                        nav.MoveToFirstChild();
                        GetNodeInfo(nav);
                    }
                }                
                int iCount = 0;
                logger.Debug("Trying to find fanart for artist " + artist + ".");
                if (!reportProgress)
                {
                    if (alSearchResults.Count > iMax)
                    {
                        dbm.TotArtistsBeingScraped = iMax;
                    }
                    else
                    {
                        dbm.TotArtistsBeingScraped = alSearchResults.Count;
                    }
                    dbm.CurrArtistsBeingScraped = 0;
                    if (FanartHandlerSetup.MyScraperNowWorker != null)
                    {
                        FanartHandlerSetup.MyScraperNowWorker.ReportProgress(0, "Ongoing");
                    }
                }
                dbArtist = Utils.GetArtist(artist, "MusicFanart");
                for (int x = 0; x < alSearchResults.Count; x++)
                {
                    if (dbm.StopScraper == true)
                    {
                        break;
                    }
                    sArtist = Utils.RemoveResolutionFromArtistName(((SearchResults)alSearchResults[x]).title);
                    sArtist = sArtist.Trim();                    
                    sArtist = Utils.GetArtist(sArtist, "MusicFanart");                    
                    if (Utils.IsMatch(dbArtist, sArtist))
                    {
                        if (iCount < iMax)
                        {
                            if (((SearchResults)alSearchResults[x]).album.Equals("1"))
                            {
                                //Artist Fanart
                                logger.Debug("Found fanart for artist " + artist + ".");
                                sourceFilename = "http://htbackdrops.com/api/02274c29b2cc898a726664b96dcc0e76/download/" + ((SearchResults)alSearchResults[x]).id + "/fullsize";
                                if (dbm.SourceImageExist(dbArtist, ((SearchResults)alSearchResults[x]).id, "MusicFanart") == false)
                                {
                                    if (DownloadImage(ref dbArtist, ref sourceFilename, ref path, ref filename, ref requestPic, ref responsePic, "MusicFanart"))
                                    {
                                        iCount = iCount + 1;
                                        dbm.LoadMusicFanart(dbArtist, filename, ((SearchResults)alSearchResults[x]).id, "MusicFanart", 0);
                                        if (FanartHandlerSetup.MyScraperNowWorker != null)
                                        {
                                            FanartHandlerSetup.MyScraperNowWorker.TriggerRefresh = true;
                                        }
                                    }
                                }
                                else
                                {
                                    logger.Debug("Will not download fanart image as it already exist an image in your fanart database with this source image name.");
                                }
                            }
                        }
                        if (((SearchResults)alSearchResults[x]).album.Equals("5") && !foundThumb)
                        {
                            //Artist Thumbnail
                            logger.Debug("Found thumbnail for artist " + artist + ".");
                            sourceFilename = "http://htbackdrops.com/api/02274c29b2cc898a726664b96dcc0e76/download/" + ((SearchResults)alSearchResults[x]).id + "/fullsize";
                            if (DownloadImage(ref artist, ref sourceFilename, ref path, ref filename, ref requestPic, ref responsePic, "MusicThumbs"))
                            {
                                dbm.LoadMusicFanart(dbArtist, filename, ((SearchResults)alSearchResults[x]).id, "MusicThumbnails", 0);
                                dbm.SetSuccessfulScrapeThumb(dbArtist, 2);
                                foundThumb = true; 
                            }                                                        
                        }
                    }
                    if (!reportProgress)
                    {
                        dbm.CurrArtistsBeingScraped++;
                        if (dbm.TotArtistsBeingScraped > 0 && FanartHandlerSetup.MyScraperNowWorker != null)
                        {
                            FanartHandlerSetup.MyScraperNowWorker.ReportProgress(Convert.ToInt32((dbm.CurrArtistsBeingScraped / dbm.TotArtistsBeingScraped) * 100), "Ongoing");
                        }
                    }
                }
                if (!foundThumb)
                {
                    dbm.SetSuccessfulScrapeThumb(dbArtist, 1);
                }
                if (alSearchResults != null)
                {
                    alSearchResults.Clear();
                }
                alSearchResults = null;
                objRequest = null;
                return iCount;
            }
            catch (Exception ex)
            {
                if (alSearchResults != null)
                {
                    alSearchResults = null;
                }
                alSearchResults.Clear();
                alSearchResults = null;
                logger.Error("getImages: " + ex.ToString());  
            }
            return 9999;
        }
        
    }

    class SearchResults
    {
        public string id = string.Empty;
        public string album = string.Empty;
        public string title = string.Empty;

        public SearchResults()
        {
        }

        public SearchResults(string id, string album, string title)
        {
            this.id = id;
            this.album = album;
            this.title = title;
        }
    }
}
