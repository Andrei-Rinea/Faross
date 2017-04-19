using Faross.Util;

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

        public override bool Equals(object obj)
        {
            var other = obj as CheckResultDetail;
            return other != null &&
                   other.Success == Success &&
                   other.Info == Info;
        }

        public override int GetHashCode()
        {
            return HashCodeUtil.GetCombinedHash(Success, Info);
        }
    }
}