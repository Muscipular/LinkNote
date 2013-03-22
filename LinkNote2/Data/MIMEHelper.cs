namespace LinkNote2.Data
{
    public static class MIMEHelper
    {
        public static string GetMIME(string ext)
        {
            ext = ext ?? "";
            ext = ext.TrimStart('.');
            switch (ext)
            {
                case "txt":
                case "cs":
                case "sql":
                case "xaml":
                    return "text/plain";
                case "rtf":
                    return "text/richtext";
                case "js":
                    return "text/javascript";
                case "xml":
                case "xsl":
                case "htm":
                case "html":
                case "css":
                    return "text/" + ext;
                case "png":
                case "jpg":
                case "bmp":
                case "tif":
                case "tiff":
                case "gif":
                case "jpeg":
                    return "image/" + ext;
                default:
                    return "application/octet-stream";
            }
        }
    }
}