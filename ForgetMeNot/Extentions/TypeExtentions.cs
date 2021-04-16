using System;
using System.Collections.Generic;
using System.Linq;

namespace ForgetMeNot.Extentions
{
    public static class TypeExtentions
    {
        public static IEnumerable<Type> GetAllInAssembly(this Type type)
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(x => x.GetTypes())
                .Where(x => type.IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract);
        }
    }
}