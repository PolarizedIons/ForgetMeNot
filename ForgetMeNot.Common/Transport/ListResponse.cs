using System.Collections.Generic;

namespace ForgetMeNot.Common.Transport
{
    public class ListResponse<T>
    {
        public ListResponse()
        {
        }

        public ListResponse(IEnumerable<T> list)
        {
            List = list;
        }

        public IEnumerable<T> List { get; set; }
    }
}
