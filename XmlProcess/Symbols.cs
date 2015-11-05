using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XmlProcess
{
    static class Symbols
    {
        public static string STRING = "^\".*\"$";
        public static string ANY = ".*";
        public static string IDENT = @"^[\w\d.-_:~$]+$";
        public static string L_PARA = @"^\($";
        public static string R_PARA = @"^\)$";
        public static string L_BRACE = @"^{$";
        public static string R_BRACE = @"^}$";
        public static string END_STMT = "^;$";
        public static string START_ELEMENT = "<";
        public static string END_ELEMENT = ">";
        public static string START_CLOSE_ELEMENT = "</";
        public static string END_START_ELEMENT = " />";
    }
}
