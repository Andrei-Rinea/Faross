using Faross.Util;

namespace Faross.Models
{
    public class ConditionResultDetail
    {
        public ConditionResultDetail(string conditionName, bool success, string info)
        {
            ConditionName = conditionName;
            Success = success;
            Info = info;
        }

        public string ConditionName { get; }
        public bool Success { get; }
        public string Info { get; }

        public override bool Equals(object obj)
        {
            var other = obj as ConditionResultDetail;
            return other != null &&
                   other.Success == Success &&
                   other.Info == Info &&
                   other.ConditionName == ConditionName;
        }

        public override int GetHashCode()
        {
            return HashCodeUtil.GetCombinedHash(ConditionName, Success, Info);
        }
    }
}