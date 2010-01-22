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
            try
            {
                logger.Debug("Scrape for new images is starting...");
                string strResult = "";
                string dbArtist;
                string path;
                string filename;
                WebRequest objRequest = WebRequest.Create("http://www.htbackdrops.com/search.php?search_terms=all&cat_id=1&search_keywords=*&search_new_images=1");
                //ADD PROXY CODE HERE IF NEEDED!!!

                //Found: 529 image(s) on 18 page(s). Displayed:
                //"http://www.htbackdrops.com/search.php?search_terms=all&cat_id=1&search_keywords=*&search_new_images=1&page=1"
                string sReg = @"Found: [0-9]+ image\(s\) on [0-9]+ page\(s\). Displayed:";
                
                //The WebResponse object gets the Request's response (the HTML) 
                WebResponse objResponse = objRequest.GetResponse();

                using (StreamReader sr = new StreamReader(objResponse.GetResponseStream()))
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
                    logger.Debug("Found " + sPages + " pages with new images on htbackdrops.com");
                    int iPages = Convert.ToInt32(sPages);

                    for (int x = 0; x < iPages; x++)
                    {
                        if (dbm.stopScraper == true)
                        {
                            break;
                        } 
                        logger.Debug("Scanning page " + (x+1));
                        objRequest = WebRequest.Create("http://www.htbackdrops.com/search.php?search_terms=all&cat_id=1&search_keywords=*&search_new_images=1&page=" + (x + 1));
                        sReg = @"-->\n<a href=\""./details.php((.|\n)*?)mode=search&amp;sessionid=((.|\n)*?)img src=""./data/thumbnails/((.|\n)*?)alt=""((.|\n)*?)"" />";

                        //The WebResponse object gets the Request's response (the HTML) 
                        objResponse = objRequest.GetResponse();

                        using (StreamReader sr = new StreamReader(objResponse.GetResponseStream()))
                        {
                            strResult = sr.ReadToEnd();
                            // Close and clean up the StreamReader
                            sr.Close();
                        }
                        objResponse.Close();
                        reg = new Regex(sReg, RegexOptions.IgnoreCase | RegexOptions.Compiled);

                        WebRequest requestPic;
                        WebResponse responsePic;
                        Image webImage;
                        string sArtist;
                        string picUri = "";
                        string sourceFilename = "";
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
                            //sql to get artist and images
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
                                    requestPic = WebRequest.Create(sourceFilename);
                                    responsePic = requestPic.GetResponse();
                                    webImage = Image.FromStream(responsePic.GetResponseStream());
                                    path = Config.GetFolder(Config.Dir.Thumbs) + @"\Skin FanArt\music";
                                    filename = path + @"\" + Utils.PatchFilename(sArtist) + " ("+randNumber.Next(0, 99999) + ").jpg";
                                    logger.Debug("Downloading fanart for " + sArtist + " (" + filename + ").");
                                    webImage.Save(filename, System.Drawing.Imaging.ImageFormat.Jpeg);
                                    responsePic.Close();
                                    iCount = iCount + 1;
                                    dbm.loadMusicFanart(dbArtist, filename, sourceFilename, "MusicFanart");
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
                    }
                }
                else
                {
                    logger.Debug("Found no new images on htbackdrops.com");
                }
                logger.Debug("Scrape for new images is done.");
            }
            catch (Exception ex)
            {
                logger.Error("getNewImages: " + ex.ToString());
            }
        }

        /// <summary>
        /// Scrapes image for a specific artist on htbackdrops.com.
        /// </summary>
        public int getImages(string artist, int iMax, DatabaseManager dbm, FanartHandler.FanartHandlerSetup.ScraperWorkerNowPlaying swnp)
        {
            try
            {
                string dbArtist;
                string strResult = "";
                string path;
                string filename;
                //HttpWebRequest objRequest = (HttpWebRequest)WebRequest.Create("http://www.htbackdrops.com/search.php?search_terms=all&cat_id=1&search_keywords=" + HttpUtility.UrlEncode(artist));
                HttpWebRequest objRequest = (HttpWebRequest)WebRequest.Create("http://www.htbackdrops.com/search.php?");                
                //ADD PROXY CODE HERE IF NEEDED!!!

                string sReg = @"-->\n<a href=\""./details.php((.|\n)*?)mode=search&amp;sessionid=((.|\n)*?)img src=""./data/thumbnails/((.|\n)*?)alt=""((.|\n)*?)"" />";
                string values = "search_terms=all";
                values += "&cat_id=1";
                values += "&search_keywords=" + Utils.ReplaceDiacritics(artist);
                objRequest.Method = "POST";
                objRequest.ContentType = "application/x-www-form-urlencoded";
                objRequest.ContentLength = values.Length;
                using (StreamWriter writer = new StreamWriter(objRequest.GetRequestStream(), System.Text.Encoding.ASCII))
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
                Regex reg;
                Match m;
                WebRequest requestPic;
                WebResponse responsePic;
                Image webImage;
                string sArtist;
                string picUri = "";
                string sourceFilename = "";
                reg = new Regex(sReg, RegexOptions.IgnoreCase | RegexOptions.Compiled);
                int iCount = 0;
                logger.Debug("Trying to find fanart for artist " + artist + ".");
                for (m = reg.Match(strResult); m.Success && iCount < iMax; m = m.NextMatch())
                {
                    picUri = m.Groups[0].ToString();
                    sArtist = picUri.Substring(picUri.IndexOf("alt=\"") + 5);
                    sArtist = sArtist.Substring(0, sArtist.LastIndexOf("\" />"));
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
                            requestPic = WebRequest.Create(sourceFilename);
                            responsePic = requestPic.GetResponse();
                            webImage = Image.FromStream(responsePic.GetResponseStream());
                            path = Config.GetFolder(Config.Dir.Thumbs) + @"\Skin FanArt\music";
                            filename = path + @"\" + Utils.PatchFilename(artist) + " (" + randNumber.Next(0, 99999) + ").jpg";
                            logger.Debug("Downloading fanart for " + artist + " (" + filename + ").");
                            webImage.Save(filename, System.Drawing.Imaging.ImageFormat.Jpeg);
                            responsePic.Close();
                            iCount = iCount + 1;
                            dbm.loadMusicFanart(dbArtist, filename, sourceFilename, "MusicFanart");
                            if (swnp != null)
                            {
                                swnp.setRefreshFlag(true);
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
            return 0;
        }
    }
}
