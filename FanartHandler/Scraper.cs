using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Text.RegularExpressions;
using System.IO;
using System.Web;
using MediaPortal.Configuration;
using System.Net;
using NLog;
using System.Management;
using System.Threading;

namespace FanartHandler
{
    class Scraper
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private Random randNumber = new Random();
        

        /// <summary>
        /// Scrapes the "new" pages on htbackdrops.com.
        /// </summary>
        public void getNewImages(int iMax, DatabaseManager dbm)
        {
            if (dbm.stopScraper == false)
            {
                try
                {
                    logger.Info("Scrape for new images is starting...");
                    System.Net.ServicePointManager.Expect100Continue = false;
                    Encoding enc = Encoding.GetEncoding("iso-8859-1"); 
                    string strResult = null;
                    string dbArtist = null;
                    string path = null;
                    string filename = null;
                    HttpWebRequest objRequest = (HttpWebRequest)WebRequest.Create("http://www.htbackdrops.com/search.php?search_terms=all&cat_id=1&search_keywords=*&search_new_images=1");
                    if (Utils.GetUseProxy() != null && Utils.GetUseProxy().Equals("True"))
                    {
                        WebProxy proxy = new WebProxy(Utils.GetProxyHostname() + ":" + Utils.GetProxyPort());
                        proxy.Credentials = new NetworkCredential(Utils.GetProxyUsername(), Utils.GetProxyPassword(), Utils.GetProxyDomain());
                        WebRequest.DefaultWebProxy = proxy;
                    }
                    objRequest.ServicePoint.Expect100Continue = false; 
                    //Found: 529 image(s) on 18 page(s). Displayed:
                    //"http://www.htbackdrops.com/search.php?search_terms=all&cat_id=1&search_keywords=*&search_new_images=1&page=1"
                    string sReg = @"Found: [0-9]+ image\(s\) on [0-9]+ page\(s\). Displayed:";

                    //The WebResponse object gets the Request's response (the HTML) 
                    WebResponse objResponse = objRequest.GetResponse();

                    using (StreamReader sr = new StreamReader(objResponse.GetResponseStream(), enc))
                    {
                        strResult = sr.ReadToEnd();
                        // Close and clean up the StreamReader
                        sr.Close();
                    }
                    objResponse.Close();
                    Match m;
                    string sPages = null;
                    Regex reg = new Regex(sReg, RegexOptions.IgnoreCase | RegexOptions.Compiled);
                    for (m = reg.Match(strResult); m.Success; m = m.NextMatch())
                    {
                        //Found: 529 image(s) on 18 page(s). Displayed:
                        sPages = m.Groups[0].ToString();
                        sPages = sPages.Substring(sPages.IndexOf("on ") + 3);
                        sPages = sPages.Substring(0, sPages.IndexOf(" page(s)."));
                    }
                    if (sPages != null && sPages.Length > 0)
                    {
                        logger.Info("Found " + sPages + " pages with new images on htbackdrops.com");
                        int iPages = Convert.ToInt32(sPages);

                        dbm.totArtistsBeingScraped = iPages;
                        dbm.currArtistsBeingScraped = 0;

                        for (int x = 0; x < iPages; x++)
                        {
                            if (dbm.stopScraper == true)
                            {
                                break;
                            }
                            logger.Debug("Scanning page " + (x + 1));
                            objRequest = (HttpWebRequest)WebRequest.Create("http://www.htbackdrops.com/search.php?search_terms=all&cat_id=1&search_keywords=*&search_new_images=1&page=" + (x + 1));
                            objRequest.ServicePoint.Expect100Continue = false; 
                            sReg = @"-->\n<a href=\""./details.php((.|\n)*?)mode=search&amp;sessionid=((.|\n)*?)img src=""./data/thumbnails/((.|\n)*?)alt=""((.|\n)*?)"" />";

                            //The WebResponse object gets the Request's response (the HTML) 
                            objResponse = objRequest.GetResponse();

                            using (StreamReader sr = new StreamReader(objResponse.GetResponseStream(), enc))
                            {
                                strResult = sr.ReadToEnd();
                                // Close and clean up the StreamReader
                                sr.Close();
                            }
                            objResponse.Close();
                            reg = new Regex(sReg, RegexOptions.IgnoreCase | RegexOptions.Compiled);

                            HttpWebRequest requestPic = null;
                            WebResponse responsePic = null;
                            string sArtist = null;
                            string picUri = null;
                            string sourceFilename = null;
                            int iCount = 0;
                            for (m = reg.Match(strResult); m.Success; m = m.NextMatch())
                            {
                                if (dbm.stopScraper == true)
                                {
                                    break;
                                }
                                picUri = m.Groups[0].ToString();
                                sArtist = picUri.Substring(picUri.IndexOf("alt=\"") + 5);
                                sArtist = sArtist.Substring(0, sArtist.LastIndexOf("\" />"));
                                dbArtist = Utils.GetArtist(sArtist, "MusicFanart");
                                sArtist = Utils.RemoveResolutionFromArtistName(sArtist);
                                dbArtist = Utils.RemoveResolutionFromArtistName(dbArtist);
                                iCount = dbm.GetArtistCount(sArtist, dbArtist);
                                if (iCount > 0 && iCount < iMax)
                                {
                                    logger.Debug("Matched fanart for artist " + sArtist + ".");
                                    picUri = picUri.Substring(picUri.IndexOf("img src=\".") + 10);
                                    picUri = picUri.Substring(0, picUri.IndexOf("\" border="));
                                    picUri = picUri.Replace("data/thumbnails", "data/media");
                                    sourceFilename = "http://www.htbackdrops.com/" + picUri;
                                    if (dbm.SourceImageExist(dbArtist, sourceFilename, "MusicFanart") == false)
                                    {
                                        if (DownloadImage(ref sArtist, ref sourceFilename, ref path, ref filename, ref requestPic, ref responsePic))
                                        {
                                            iCount = iCount + 1;
                                            dbm.loadMusicFanart(dbArtist, filename, sourceFilename, "MusicFanart");
                                        }
                                    }
                                    else
                                    {
                                        logger.Debug("Will not download fanart image as it already exist an image in your fanart database with this source image name.");
                                    }
                                }
                                else
                                {
                                    if (iCount == 0 && iCount < iMax)
                                    {
                                        //                  logger.Debug("Artist not in your fanart database. Will not download fanart.");
                                    }
                                    else
                                    {
                                        logger.Debug("Artist " + sArtist + " has already maximum number of images. Will not download anymore images for this artist.");
                                    }
                                }
                            }
                            dbm.currArtistsBeingScraped++;
                        }
                    }
                    else
                    {
                        logger.Info("Found no new images on htbackdrops.com");
                    }
                    logger.Info("Scrape for new images is done.");
                }
                catch (Exception ex)
                {
                    logger.Error("getNewImages: " + ex.ToString());
                }
            }
        }
        

        /// <summary>
        /// Downloads and saves images from htbackdrops.com.
        /// </summary>
        private bool DownloadImage(ref string sArtist, ref string sourceFilename, ref string path, ref string filename, ref HttpWebRequest requestPic, ref WebResponse responsePic)
        {
            int maxCount = 0;
            long position = 0;
            string status = "Resume";

            path = Config.GetFolder(Config.Dir.Thumbs) + @"\Skin FanArt\music";
            filename = path + @"\" + Utils.PatchFilename(sArtist) + " (" + randNumber.Next(10000, 99999) + ").jpg";
            logger.Debug("Downloading fanart for " + sArtist + " (" + filename + ").");

            while (status.Equals("Success") == false && status.Equals("Stop") == false && maxCount < 10)
            {
                Stream webStream = null;
                FileStream fileStream = null;
                status = "Success";
                try
                {
                    if (File.Exists(filename)) position = new FileInfo(filename).Length;
                    maxCount++;                    
                    requestPic = (HttpWebRequest)WebRequest.Create(sourceFilename);
                    requestPic.ServicePoint.Expect100Continue = false; 
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

        /// <summary>
        /// Scrapes image for a specific artist on htbackdrops.com.
        /// </summary>
        public int getImages(string artist, int iMax, DatabaseManager dbm, FanartHandler.FanartHandlerSetup.ScraperWorkerNowPlaying swnp)
        {
            try
            {
                Encoding enc = Encoding.GetEncoding("iso-8859-1");
                System.Net.ServicePointManager.Expect100Continue = false;
                string dbArtist = null;
                string strResult = null;
                string path = null;
                string filename = null;
                HttpWebRequest objRequest = (HttpWebRequest)WebRequest.Create("http://www.htbackdrops.com/search.php?");
                if (Utils.GetUseProxy() != null && Utils.GetUseProxy().Equals("True"))
                {
                    WebProxy proxy = new WebProxy(Utils.GetProxyHostname() + ":" + Utils.GetProxyPort());
                    proxy.Credentials = new NetworkCredential(Utils.GetProxyUsername(), Utils.GetProxyPassword(), Utils.GetProxyDomain());
                    WebRequest.DefaultWebProxy = proxy;
                }
                objRequest.ServicePoint.Expect100Continue = false; 
                string sReg = @"-->\n<a href=\""./details.php((.|\n)*?)mode=search&amp;sessionid=((.|\n)*?)img src=""./data/thumbnails/((.|\n)*?)alt=""((.|\n)*?)"" />";
                string values = "search_terms=all";
                values += "&cat_id=1";
                values += "&search_keywords=" + artist;
                objRequest.Method = "POST";
                objRequest.ContentType = "application/x-www-form-urlencoded";
                objRequest.ContentLength = values.Length;               
                using (StreamWriter writer = new StreamWriter(objRequest.GetRequestStream(), enc))
                {
                    writer.Write(values);
                }

                WebResponse objResponse = objRequest.GetResponse();                

                using (StreamReader sr = new StreamReader(objResponse.GetResponseStream(), enc))
                {
                    strResult = sr.ReadToEnd();
                    sr.Close();
                }
                objResponse.Close();
                Regex reg;
                Match m;
                HttpWebRequest requestPic = null;
                WebResponse responsePic = null;
                string sArtist = null;
                string picUri = null;
                string sourceFilename = null;
                reg = new Regex(sReg, RegexOptions.IgnoreCase | RegexOptions.Compiled);
                int iCount = 0;
                logger.Debug("Trying to find fanart for artist " + artist + ".");
                for (m = reg.Match(strResult); m.Success && iCount < iMax; m = m.NextMatch())
                {
                    picUri = m.Groups[0].ToString();
                    sArtist = picUri.Substring(picUri.IndexOf("alt=\"") + 5);
                    sArtist = sArtist.Substring(0, sArtist.LastIndexOf("\" />"));
                    sArtist = Utils.RemoveResolutionFromArtistName(sArtist);
                    dbArtist = Utils.GetArtist(artist, "MusicFanart");
                    sArtist = Utils.GetArtist(sArtist, "MusicFanart");
                    if (Utils.isMatch(dbArtist, sArtist))
                    {
                        logger.Debug("Found fanart for artist " + artist + ".");
                        picUri = picUri.Substring(picUri.IndexOf("img src=\".") + 10);
                        picUri = picUri.Substring(0, picUri.IndexOf("\" border="));
                        picUri = picUri.Replace("data/thumbnails", "data/media");
                        sourceFilename = "http://www.htbackdrops.com/" + picUri;
                        if (dbm.SourceImageExist(dbArtist, sourceFilename, "MusicFanart") == false)
                        {
                            if (DownloadImage(ref sArtist, ref sourceFilename, ref path, ref filename, ref requestPic, ref responsePic))
                            {
                                iCount = iCount + 1;
                                dbm.loadMusicFanart(dbArtist, filename, sourceFilename, "MusicFanart");
                                if (swnp != null)
                                {
                                    swnp.setRefreshFlag(true);
                                }
                            }
                        }
                        else
                        { 
                            logger.Debug("Will not download fanart image as it already exist an image in your fanart database with this source image name.");
                        }
                    }
                }
                objRequest = null;
                return iCount;
            }
            catch (Exception ex)
            {
                logger.Error("getImages: " + ex.ToString());  
            }
            return 9999;
        }
    }
}
