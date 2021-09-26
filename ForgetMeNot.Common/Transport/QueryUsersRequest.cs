using System.Collections.Generic;

namespace ForgetMeNot.Common.Transport
{
    public class QueryUsersRequest
    {
        public IEnumerable<ulong> UserIds { get; set; }
    }
}
