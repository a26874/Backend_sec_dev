/*
*	<copyright file="NullChecks">
*	</copyright>
* 	<author>Marco Macedo</author>
*	<contact>a26874@alunos.ipca.pt</contact>
*   <date>2025 12/13/2025 11:04:33 PM</date>
*	<description></description>
**/

namespace Backend_sec_dev.Shared.Helpers
{
    public static class NullChecks
    {
        public static bool StringNullOrEmpty(string t)
        {
            return t == null && t == string.Empty && t.ToCharArray().Length  == 0;
        }

        public static bool ArrayNullOrEmpty<T>(Array array)
        {
            return array != null && array.Length == 0;
        }

        public static bool ListNullOrEmpty<T>(List<T> list)
        {
            return list != null && list.Count == 0; 
        }
    }
}