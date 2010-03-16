using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Globalization;
using NLog;
using MediaPortal.GUI.Library;
using System.Drawing;
using System.IO;


namespace FanartHandler
{
    static class Utils
    {
        #region declarations
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private const string rxMatchNonWordCharacters = @"[^\w]";
        public const string GetMajorMinorVersionNumber = "1.6";  //Holds current pluginversion.
        private static string useProxy = null;  // Holds info read from fanarthandler.xml settings file
        private static string proxyHostname = null;  // Holds info read from fanarthandler.xml settings file
        private static string proxyPort = null;  // Holds info read from fanarthandler.xml settings file
        private static string proxyUsername = null;  // Holds info read from fanarthandler.xml settings file
        private static string proxyPassword = null;  // Holds info read from fanarthandler.xml settings file
        private static string proxyDomain = null;  // Holds info read from fanarthandler.xml settings file
        private static bool isStopping = false;  //is the plugin about to stop, then this will be true
        private static DatabaseManager dbm;  //database handle
        private static string scraperMaxImages = null;  //Max scraper images allowed
        #endregion

        /// <summary>
        /// Return value.
        /// </summary>
        public static DatabaseManager GetDbm()
        {
            return dbm;
        }

        /// <summary>
        /// Set value.
        /// </summary>
        public static void InitiateDbm()
        {
            dbm = new DatabaseManager();
            dbm.initDB();
        }

        /// <summary>
        /// Return value.
        /// </summary>
        public static bool GetIsStopping()
        {
            return isStopping;
        }

        /// <summary>
        /// Set value.
        /// </summary>
        public static void SetIsStopping(bool b)
        {
            isStopping = b;
        }

        /// <summary>
        /// Return value.
        /// </summary>
        public static string GetScraperMaxImages()
        {
            return scraperMaxImages;
        }

        /// <summary>
        /// Set value.
        /// </summary>
        public static void SetScraperMaxImages(string s)
        {
            scraperMaxImages = s;
        }

        /// <summary>
        /// Return value.
        /// </summary>
        public static string GetUseProxy()
        {            
            return useProxy;
        }

        /// <summary>
        /// Set value.
        /// </summary>
        public static void SetUseProxy(string s)
        {
            useProxy = s;
        }

        /// <summary>
        /// Return value.
        /// </summary>
        public static string GetProxyHostname()
        {
            return proxyHostname;
        }

        /// <summary>
        /// Set value.
        /// </summary>
        public static void SetProxyHostname(string s)
        {
            proxyHostname = s;
        }

        /// <summary>
        /// Return value.
        /// </summary>
        public static string GetProxyPort()
        {
            return proxyPort;
        }

        /// <summary>
        /// Set value.
        /// </summary>
        public static void SetProxyPort(string s)
        {
            proxyPort = s;
        }

        /// <summary>
        /// Return value.
        /// </summary>
        public static string GetProxyUsername()
        {
            return proxyUsername;
        }

        /// <summary>
        /// Set value.
        /// </summary>
        public static void SetProxyUsername(string s)
        {
            proxyUsername = s;
        }

        /// <summary>
        /// Return value.
        /// </summary>
        public static string GetProxyPassword()
        {
            return proxyPassword;
        }

        /// <summary>
        /// Set value.
        /// </summary>
        public static void SetProxyPassword(string s)
        {
            proxyPassword = s;
        }

        /// <summary>
        /// Return value.
        /// </summary>
        public static string GetProxyDomain()
        {
            return proxyDomain;
        }

        /// <summary>
        /// Set value.
        /// </summary>
        public static void SetProxyDomain(string s)
        {
            proxyDomain = s;
        }

        /// <summary>
        /// Returns and converts the string into a common format. Thanks to Moving Picture developers for this
        /// function (http://code.google.com/p/moving-pictures/).
        /// </summary>
        /// 
        /// <param name="self"></param>
        /// <returns></returns>
        public static string Equalize(this String self)
        {
            if (self == null) return string.Empty;

            // Convert title to lowercase culture invariant
            string newTitle = self.ToLowerInvariant();

            // Replace non-descriptive characters with spaces
            newTitle = Regex.Replace(newTitle, rxMatchNonWordCharacters, " ");

            // Equalize: Convert to base character string
            newTitle = newTitle.RemoveDiacritics();

            // Equalize: Common characters with words of the same meaning
            newTitle = Regex.Replace(newTitle, @"\b(and|und|en|et|y)\b", " & ");

            // Remove the number 1 from the end of a title string
            newTitle = Regex.Replace(newTitle, @"\s(1)$", "");


            // Remove double spaces and return the cleaned title
            return newTitle.TrimWhiteSpace();
        }


        /// <summary>
        /// Translates characters to their base form. ( ë/é/è -> e)
        /// </summary>
        /// <example>
        /// characters: ë, é, è
        /// result: e
        /// </example>
        /// <remarks>
        /// source: http://blogs.msdn.com/michkap/archive/2007/05/14/2629747.aspx
        /// </remarks>
        /// <param name="self"></param>
        /// <returns></returns>
        public static string RemoveDiacritics(this String self)
        {
            if (self == null) return string.Empty;
            string stFormD = self.Normalize(NormalizationForm.FormD);
            StringBuilder sb = new StringBuilder();
            for (int ich = 0; ich < stFormD.Length; ich++)
            {
                UnicodeCategory uc = CharUnicodeInfo.GetUnicodeCategory(stFormD[ich]);
                if (uc != UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(stFormD[ich]);
                }
            }

            return (sb.ToString().Normalize(NormalizationForm.FormC));
        }

        /// <summary>
        /// Replaces diacritics in a string
        /// </summary>    
        public static string ReplaceDiacritics(this String self)
        {
            if (self == null) return string.Empty;
            string s1 = self;
            string s2 = Utils.RemoveDiacritics(self);
            StringBuilder sb = new StringBuilder();
            for (int ich = 0; ich < s1.Length; ich++)
            {
                if (s1[ich].Equals(s2[ich]) == false)
                {
                    sb.Append("*");
                }
                else
                {
                    sb.Append(s1[ich]);
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// Matches two strings (artists or titles)
        /// </summary>        
        public static bool isMatch(string s1, string s2)
        {
            if (s1 == null) return false;
            int i = 0;
            if (s1.Length > s2.Length)
            {
                i = s1.Length - s2.Length;
            }
            else if (s2.Length > s1.Length)
            {
                i = s1.Length - s2.Length;
            }
            if (Utils.IsInteger(s1))
            {
                if (s2.Contains(s1) && i <= 2)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                s2 = Utils.RemoveTrailingDigits(s2);
                s1 = Utils.RemoveTrailingDigits(s1);
                if (s2.Equals(s1))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Check if sting is numeric
        /// </summary>        
        public static bool IsInteger(string theValue)
        {
            if (theValue == null) return false;
            Regex _isNumber = new Regex(@"^\d+$");
            Match m = _isNumber.Match(theValue);
            return m.Success;
        } 

        /// <summary>
        /// Replaces multiple white-spaces with one space
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static string TrimWhiteSpace(this String self)
        {
            if (self == null) return string.Empty;
            return Regex.Replace(self, @"\s{2,}", " ").Trim();
        }

        /// <summary>
        /// Remove _ from string.
        /// </summary>
        public static string RemoveUnderline(string key)
        {
            if (key == null) return string.Empty;
            return Regex.Replace(key, @"_", "");
        }

        /// <summary>
        /// Get Artist.
        /// </summary>
        public static string GetArtist(string key, string type)
        {
            if (key == null) return string.Empty;
            key = GetFilenameNoPath(key);
            key = Utils.RemoveExtension(key);
            key = Regex.Replace(key, @"\(\d{5}\)", "").Trim();
            if (type.Equals("MusicArtist"))
            {
                key = Regex.Replace(key, "[L]$", "").Trim();
            }
            key = Utils.RemoveUnderline(key);
            if (type.Equals("MusicAlbum"))
            {
                if (key.IndexOf("-") >= 0)
                {
                    key = key.Substring(0, key.IndexOf("-"));
                }
            }
            key = RemoveTrailingDigits(key);
            key = key.Equalize();
            key = key.MovePrefixToFront();
            return key;
        }

        /// <summary>
        /// Removes MP artist pipe (| artist |) in artist name
        /// </summary>    
        public static string RemoveMPArtistPipes(string s)
        {
            if (s == null) return string.Empty;
            s = s.Replace("|","");
            s = s.Trim();
            return s;
        }

        /// <summary>
        /// Get Artist.
        /// </summary>
        public static string GetArtistLeftOfMinusSign(string key)
        {
            if (key == null) return string.Empty;
            if (key.IndexOf("-") >= 0)
            {
                key = key.Substring(0, key.IndexOf("-"));
            }
            return key;
        }

        /// <summary>
        /// Get filename string.
        /// </summary>
        public static string GetFilenameNoPath(string key)
        {
            if (key == null) return string.Empty;
            key = key.Replace("/", "\\");
            if (key.LastIndexOf("\\") >= 0)
            {
                return key.Substring(key.LastIndexOf("\\") + 1);
            }
            return key;
        }

        /// <summary>
        /// Remove file extension.
        /// </summary>
        public static string RemoveExtension(string key)
        {
            if (key == null) return string.Empty;
            //key = key.ToLowerInvariant();
            key = Regex.Replace(key, @".jpg", "");
            key = Regex.Replace(key, @".JPG", "");
            key = Regex.Replace(key, @".png", "");
            key = Regex.Replace(key, @".PNG", "");
            key = Regex.Replace(key, @".bmp", "");
            key = Regex.Replace(key, @".BMP", "");
            key = Regex.Replace(key, @".tif", "");
            key = Regex.Replace(key, @".TIF", "");
            key = Regex.Replace(key, @".gif", "");
            key = Regex.Replace(key, @".GIF", "");
            return key;
        }

        /// <summary>
        /// Remove digits from string.
        /// </summary>
        public static string RemoveDigits(string key)
        {
            if (key == null) return string.Empty;
            return Regex.Replace(key, @"\d", "");            
        }

        /// <summary>
        /// Patch SQL statements by replaceing single quotes with two
        /// </summary>    
        public static string PatchSQL(string s)
        {
            if (s == null) return string.Empty;
            return s.Replace("'", "''");
        }

        /// <summary>
        /// Remove resolution information from artist names. Due to some
        /// images at htbackdrops.com containing for example trailing _720P...
        /// </summary>    
        public static string RemoveResolutionFromArtistName(string s)
        {
            if (s == null) return string.Empty;
            s = s.Replace("-(1080P)", "");
            s = s.Replace("-(720P)", "");
            s = s.Replace("-[1080P]", "");
            s = s.Replace("-[720P]", "");
            s = s.Replace("_(1080P)", "");
            s = s.Replace("_(720P)", "");
            s = s.Replace("_[1080P]", "");
            s = s.Replace("_[720P]", "");
            s = s.Replace(" (1080P)", "");
            s = s.Replace(" (720P)", "");
            s = s.Replace(" [1080P]", "");
            s = s.Replace(" [720P]", "");
            s = s.Replace("(1080P)", "");
            s = s.Replace("(720P)", "");
            s = s.Replace("[1080P]", "");
            s = s.Replace("[720P]", "");
            s = s.Replace("-1080P", "");
            s = s.Replace("-720P", "");
            s = s.Replace("-1080", "");
            s = s.Replace("-720", "");
            s = s.Replace("_1080P", "");
            s = s.Replace("_720P", "");
            s = s.Replace("_1080", "");
            s = s.Replace("_720", "");
            s = s.Replace(" 1080P", "");
            s = s.Replace(" 720P", "");
            s = s.Replace(" 1080", "");
            s = s.Replace(" 720", "");
            s = s.Replace("1080P", "");
            s = s.Replace("720P", "");
            s = s.Replace("1080", "");
            s = s.Replace("720", "");
            s = s.Replace("1920x1080", "");
            s = s.Replace("_1920", "");
            return s;
        }

        /// <summary>
        /// Remove illegal characters from filename.
        /// </summary>
        public static string PatchFilename(string s)
        {
            if (s == null) return string.Empty;
            s = s.Replace("?", "");
            s = s.Replace("[", "");
            s = s.Replace("]", "");
            s = s.Replace("/", "");
            s = s.Replace("\\", "");
            s = s.Replace("=", "");
            s = s.Replace("+", "");
            s = s.Replace("<", "");
            s = s.Replace(">", "");
            s = s.Replace(":", "");
            s = s.Replace(";", "");
            s = s.Replace("\"", "");
            s = s.Replace(",", "");
            s = s.Replace("*", "");
            s = s.Replace("|", "");
            return s.Replace("^", "");                          
        }

        /// <summary>
        /// Remove trailing digits.
        /// </summary>
        public static string RemoveTrailingDigits(string s)
        {
            if (s == null) return string.Empty;
            if (Utils.IsInteger(s))
            {
                return s;
            }
            else
            {
                return Regex.Replace(s, "[0-9]*$", "").Trim();
            }
        }

        /// <summary>
        /// Returns the string as "String, The -> The String". 
        /// Thanks to Moving Picture developers for this function (http://code.google.com/p/moving-pictures/).
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static string MovePrefixToFront(this String self)
        {
            if (self == null) return string.Empty;
            Regex expr = new Regex(@"(.+?)(?: (" + "the|a|an|ein|das|die|der|les|la|le|el|une|de|het" + @"))?\s*$", RegexOptions.IgnoreCase);
            return expr.Replace(self, "$2 $1").Trim();
        }

        /// <summary>
        /// Returns the string as "The String String, The". 
        /// Thanks to Moving Picture developers for this function (http://code.google.com/p/moving-pictures/).
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static string MovePrefixToBack(this String self)
        {
            if (self == null) return string.Empty;
            Regex expr = new Regex(@"^(" + "the|a|an|ein|das|die|der|les|la|le|el|une|de|het" + @")\s(.+)", RegexOptions.IgnoreCase);
            return expr.Replace(self, "$2, $1").Trim();
        }        

        /// <summary>
        /// Returns plugin version.
        /// </summary>
        public static string GetAllVersionNumber()
        {
            return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }


        /// <summary>
        /// Returns a shuffled list. 
        /// </summary>
        public static void Shuffle(ref Hashtable filenames)
        {
            Random r = new Random();
            if (filenames != null && r != null)
            {
                for (int n = filenames.Count - 1; n > 0; --n)
                {
                    int k = r.Next(n + 1);
                    object temp = filenames[n];
                    filenames[n] = filenames[k];
                    filenames[k] = temp;
                }
            }
        }

        /// <summary>
        /// Load image
        /// </summary>
        public static void LoadImage(string filename, string type)
        {
            if (isStopping == false)
            {
                try
                {
                    if (filename != null && filename.Length > 0)
                    {                        
                        GUITextureManager.Load(filename, 0, 0, 0, false);
                    }
                }
                catch (Exception ex)
                {
                    if (isStopping == false)
                    {
                        logger.Error("LoadImage: " + ex.ToString());
/*                        if (!IsFileValid(filename))
                        {
                            if (File.Exists(filename)) File.Delete(filename);
                            dbm.DeleteFanart(filename, type);
                            logger.Error("LoadImage: Deleting downloaded file because it is corrupt.");
                        }
 */ 
                    }

                }
            }
        }

        /// <summary>
        /// Decide if image is corropt or not
        /// </summary>
        private static bool IsFileValid(string filename)
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
            catch //(Exception ex)
            {
                checkImage = null;
            }
            return false;
        }

      

    }    
}
