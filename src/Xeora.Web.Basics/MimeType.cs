﻿using System;
using System.Collections.Generic;

namespace Xeora.Web.Basics
{
    public static class MimeType
    {
        private enum MimeLookups
        {
            Type,
            Extension
        }

        private static Dictionary<string, string> _extensionMap;
        private static Dictionary<string, string> MimeMapping
        {
            get
            {
                if (MimeType._extensionMap != null) 
                    return MimeType._extensionMap;
                
                MimeType._extensionMap = new Dictionary<string, string>
                {
                    { ".323", "text/h323" },
                    { ".asx", "video/x-ms-asf" },
                    { ".acx", "application/internet-property-stream" },
                    { ".ai", "application/postscript" },
                    { ".aif", "audio/x-aiff" },
                    { ".aiff", "audio/aiff" },
                    { ".axs", "application/olescript" },
                    { ".aifc", "audio/aiff" },
                    { ".asr", "video/x-ms-asf" },
                    { ".avi", "video/x-msvideo" },
                    { ".asf", "video/x-ms-asf" },
                    { ".au", "audio/basic" },
                    { ".application", "application/x-ms-application" },
                    { ".bin", "application/octet-stream" },
                    { ".bas", "text/plain" },
                    { ".bcpio", "application/x-bcpio" },
                    { ".bmp", "image/bmp" },
                    { ".cdf", "application/x-cdf" },
                    { ".cat", "application/vndms-pkiseccat" },
                    { ".crt", "application/x-x509-ca-cert" },
                    { ".c", "text/plain" },
                    { ".css", "text/css" },
                    { ".cer", "application/x-x509-ca-cert" },
                    { ".crl", "application/pkix-crl" },
                    { ".cmx", "image/x-cmx" },
                    { ".csh", "application/x-csh" },
                    { ".cod", "image/cis-cod" },
                    { ".cpio", "application/x-cpio" },
                    { ".clp", "application/x-msclip" },
                    { ".crd", "application/x-mscardfile" },
                    { ".deploy", "application/octet-stream" },
                    { ".dll", "application/x-msdownload" },
                    { ".dot", "application/msword" },
                    { ".doc", "application/msword" },
                    { ".dvi", "application/x-dvi" },
                    { ".dir", "application/x-director" },
                    { ".dxr", "application/x-director" },
                    { ".der", "application/x-x509-ca-cert" },
                    { ".dib", "image/bmp" },
                    { ".dcr", "application/x-director" },
                    { ".disco", "text/xml" },
                    { ".exe", "application/octet-stream" },
                    { ".etx", "text/x-setext" },
                    { ".evy", "application/envoy" },
                    { ".eml", "message/rfc822" },
                    { ".eps", "application/postscript" },
                    { ".flr", "x-world/x-vrml" },
                    { ".fif", "application/fractals" },
                    { ".gtar", "application/x-gtar" },
                    { ".gif", "image/gif" },
                    { ".gz", "application/x-gzip" },
                    { ".hta", "application/hta" },
                    { ".htc", "text/x-component" },
                    { ".htt", "text/webviewhtml" },
                    { ".h", "text/plain" },
                    { ".hdf", "application/x-hdf" },
                    { ".hlp", "application/winhlp" },
                    { ".html", "text/html" },
                    { ".htm", "text/html" },
                    { ".hqx", "application/mac-binhex40" },
                    { ".isp", "application/x-internet-signup" },
                    { ".iii", "application/x-iphone" },
                    { ".ief", "image/ief" },
                    { ".ivf", "video/x-ivf" },
                    { ".ins", "application/x-internet-signup" },
                    { ".ico", "image/x-icon" },
                    { ".jpg", "image/jpeg" },
                    { ".jfif", "image/pjpeg" },
                    { ".jpe", "image/jpeg" },
                    { ".jpeg", "image/jpeg" },
                    { ".js", "application/x-javascript" },
                    { ".lsx", "video/x-la-asf" },
                    { ".latex", "application/x-latex" },
                    { ".lsf", "video/x-la-asf" },
                    { ".manifest", "application/x-ms-manifest" },
                    { ".mhtml", "message/rfc822" },
                    { ".mny", "application/x-msmoney" },
                    { ".mht", "message/rfc822" },
                    { ".mid", "audio/mid" },
                    { ".mpv2", "video/mpeg" },
                    { ".man", "application/x-troff-man" },
                    { ".mvb", "application/x-msmediaview" },
                    { ".mpeg", "video/mpeg" },
                    { ".m3u", "audio/x-mpegurl" },
                    { ".mdb", "application/x-msaccess" },
                    { ".mpp", "application/vnd.ms-project" },
                    { ".m1v", "video/mpeg" },
                    { ".mpa", "video/mpeg" },
                    { ".me", "application/x-troff-me" },
                    { ".m13", "application/x-msmediaview" },
                    { ".movie", "video/x-sgi-movie" },
                    { ".m14", "application/x-msmediaview" },
                    { ".mpe", "video/mpeg" },
                    { ".mp2", "video/mpeg" },
                    { ".mov", "video/quicktime" },
                    { ".mp3", "audio/mpeg" },
                    { ".mpg", "video/mpeg" },
                    { ".ms", "application/x-troff-ms" },
                    { ".nc", "application/x-netcdf" },
                    { ".nws", "message/rfc822" },
                    { ".oda", "application/oda" },
                    { ".ods", "application/oleobject" },
                    { ".pmc", "application/x-perfmon" },
                    { ".p7r", "application/x-pkcs7-certreqresp" },
                    { ".p7b", "application/x-pkcs7-certificates" },
                    { ".p7s", "application/pkcs7-signature" },
                    { ".pmw", "application/x-perfmon" },
                    { ".ps", "application/postscript" },
                    { ".p7c", "application/pkcs7-mime" },
                    { ".pbm", "image/x-portable-bitmap" },
                    { ".ppm", "image/x-portable-pixmap" },
                    { ".pub", "application/x-mspublisher" },
                    { ".png", "image/png" },
                    { ".pnm", "image/x-portable-anymap" },
                    { ".pml", "application/x-perfmon" },
                    { ".p10", "application/pkcs10" },
                    { ".pfx", "application/x-pkcs12" },
                    { ".p12", "application/x-pkcs12" },
                    { ".pdf", "application/pdf" },
                    { ".pps", "application/vnd.ms-powerpoint" },
                    { ".p7m", "application/pkcs7-mime" },
                    { ".pko", "application/vndms-pkipko" },
                    { ".ppt", "application/vnd.ms-powerpoint" },
                    { ".pmr", "application/x-perfmon" },
                    { ".pma", "application/x-perfmon" },
                    { ".pot", "application/vnd.ms-powerpoint" },
                    { ".prf", "application/pics-rules" },
                    { ".pgm", "image/x-portable-graymap" },
                    { ".qt", "video/quicktime" },
                    { ".ra", "audio/x-pn-realaudio" },
                    { ".rgb", "image/x-rgb" },
                    { ".ram", "audio/x-pn-realaudio" },
                    { ".rmi", "audio/mid" },
                    { ".ras", "image/x-cmu-raster" },
                    { ".roff", "application/x-troff" },
                    { ".rtf", "application/rtf" },
                    { ".rtx", "text/richtext" },
                    { ".sv4crc", "application/x-sv4crc" },
                    { ".spc", "application/x-pkcs7-certificates" },
                    { ".setreg", "application/set-registration-initiation" },
                    { ".snd", "audio/basic" },
                    { ".stl", "application/vndms-pkistl" },
                    { ".setpay", "application/set-payment-initiation" },
                    { ".stm", "text/html" },
                    { ".shar", "application/x-shar" },
                    { ".sh", "application/x-sh" },
                    { ".sit", "application/x-stuffit" },
                    { ".spl", "application/futuresplash" },
                    { ".sct", "text/scriptlet" },
                    { ".scd", "application/x-msschedule" },
                    { ".sst", "application/vndms-pkicertstore" },
                    { ".src", "application/x-wais-source" },
                    { ".sv4cpio", "application/x-sv4cpio" },
                    { ".svg", "image/svg+xml"},
                    { ".tex", "application/x-tex" },
                    { ".tgz", "application/x-compressed" },
                    { ".t", "application/x-troff" },
                    { ".tar", "application/x-tar" },
                    { ".tr", "application/x-troff" },
                    { ".tif", "image/tiff" },
                    { ".txt", "text/plain" },
                    { ".texinfo", "application/x-texinfo" },
                    { ".trm", "application/x-msterminal" },
                    { ".tiff", "image/tiff" },
                    { ".tcl", "application/x-tcl" },
                    { ".texi", "application/x-texinfo" },
                    { ".tsv", "text/tab-separated-values" },
                    { ".ustar", "application/x-ustar" },
                    { ".uls", "text/iuls" },
                    { ".vcf", "text/x-vcard" },
                    { ".wps", "application/vnd.ms-works" },
                    { ".wav", "audio/wav" },
                    { ".wrz", "x-world/x-vrml" },
                    { ".wri", "application/x-mswrite" },
                    { ".wks", "application/vnd.ms-works" },
                    { ".wmf", "application/x-msmetafile" },
                    { ".wcm", "application/vnd.ms-works" },
                    { ".wrl", "x-world/x-vrml" },
                    { ".wdb", "application/vnd.ms-works" },
                    { ".wsdl", "text/xml" },
                    { ".xml", "text/xml" },
                    { ".xlm", "application/vnd.ms-excel" },
                    { ".xaf", "x-world/x-vrml" },
                    { ".xla", "application/vnd.ms-excel" },
                    { ".xls", "application/vnd.ms-excel" },
                    { ".xof", "x-world/x-vrml" },
                    { ".xlt", "application/vnd.ms-excel" },
                    { ".xlc", "application/vnd.ms-excel" },
                    { ".xsl", "text/xml" },
                    { ".xbm", "image/x-xbitmap" },
                    { ".xlw", "application/vnd.ms-excel" },
                    { ".xpm", "image/x-xpixmap" },
                    { ".xwd", "image/x-xwindowdump" },
                    { ".xsd", "text/xml" },
                    { ".z", "application/x-compress" },
                    { ".zip", "application/x-zip-compressed" }
                };

                // Load Custom Mime Definitions
                foreach (Configuration.IMimeItem item in Configurations.Xeora.Application.CustomMimes)
                    MimeType._extensionMap[item.Extension] = item.Type;

                return MimeType._extensionMap;
            }
        }

        private static string ResolveMime(MimeLookups mimeLookup, string searchValue)
        {
            switch (mimeLookup)
            {
                case MimeLookups.Type:
                    if (string.IsNullOrEmpty(searchValue))
                        return "application/octet-stream";

                    return MimeType.MimeMapping.ContainsKey(searchValue)
                        ? MimeType.MimeMapping[searchValue]
                        : "application/octet-stream";
                case MimeLookups.Extension:
                    if (string.IsNullOrEmpty(searchValue))
                        return ".dat";

                    foreach (var (key, value) in MimeType.MimeMapping)
                    {
                        if (string.CompareOrdinal(value, searchValue) == 0)
                            return key;
                    }

                    return ".dat";
                default:
                    throw new Exception("ResolveMime should never reach here!");
            }
        }

        /// <summary>
        /// Gets the MIME according to the file extension
        /// </summary>
        /// <returns>MIME type</returns>
        /// <param name="fileExtension">File extension</param>
        public static string GetMime(string fileExtension)
        {
            if (!string.IsNullOrEmpty(fileExtension) && !fileExtension.StartsWith("."))
                fileExtension = $".{fileExtension}";

            return MimeType.ResolveMime(MimeLookups.Type, fileExtension);
        }

        /// <summary>
        /// Gets the file extension according to the MIME type
        /// </summary>
        /// <returns>The file extension</returns>
        /// <param name="mimeType">MIME type</param>
        public static string GetExtension(string mimeType) =>
            MimeType.ResolveMime(MimeLookups.Extension, mimeType);
    }
}
