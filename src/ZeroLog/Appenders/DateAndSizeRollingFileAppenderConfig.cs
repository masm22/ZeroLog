namespace ZeroLog.Appenders
{
    public class DateAndSizeRollingFileAppenderConfig
    {
        public string FilePathRoot { get; set; }
        public string Extension { get; set; } = DateAndSizeRollingFileAppender.DefaultExtension;
        public int MaxFileSizeInBytes { get; set; } = DateAndSizeRollingFileAppender.DefaultMaxSize;
        public string PrefixPattern { get; set; } = DateAndSizeRollingFileAppender.DefaultPrefixPattern;
    }
}
