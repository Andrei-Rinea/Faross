namespace Faross.Models
{
    public class CheckResultDetail
    {
        public CheckResultDetail(bool success, string info)
        {
            Success = success;
            Info = info;
        }

        public bool Success { get; }
        public string Info { get; }
    }
}