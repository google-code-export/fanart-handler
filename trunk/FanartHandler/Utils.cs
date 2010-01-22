using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Globalization;
using NLog;


namespace FanartHandler
{
    static class Utils
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private const string rxMatchNonWordCharacters = @"[^\w]";

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

        public static string ReplaceDiacritics(this String self)
        {
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

        public static bool isMatch(string key, string filename)
        {
            int i = 0;
            if (key.Length > filename.Length)
            {
                i = key.Length - filename.Length;
            }
            else if (filename.Length > key.Length)
            {
                i = key.Length - filename.Length;
            }
            if (Utils.IsInteger(key))
            {
                if (filename.Contains(key) && i <= 2)
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
                filename = Utils.RemoveTrailingDigits(filename);
                key = Utils.RemoveTrailingDigits(key);
                if (filename.Equals(key))
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
            return Regex.Replace(self, @"\s{2,}", " ").Trim();
        }

        /// <summary>
        /// Remove _ from string.
        /// </summary>
        public static string RemoveUnderline(string key)
        {
            return Regex.Replace(key, @"_", "");
        }

        /// <summary>
        /// Get Artist.
        /// </summary>
        public static string GetArtist(string key, string type)
        {
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
            return key;
        }

        /// <summary>
        /// Get Artist.
        /// </summary>
        public static string GetArtistLeftOfMinusSign(string key)
        {
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
            return Regex.Replace(key, @"\d", "");
            
        }

        public static string PatchSQL(string s)
        {
            return s.Replace("'", "''");
        }

        public static string PatchFilename(string s)
        {
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
            if (Utils.IsInteger(s))
            {
                return s;
            }
            else
            {
                return Regex.Replace(s, "[0-9]*$", "").Trim();
            }
        }
    }    
}
