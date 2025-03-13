namespace Xeora.Web.Directives
{
    internal static class PropertyConstants
    {
        public const char DIRECTIVE = '.';
        public const char DATA_FIELD = '#';
        public const char SESSION = '-';
        public const char APPLICATION = '&';
        public const char FORM = '~';
        public const char QUERY = '^';
        public const char COOKIE = '+';
        public const char CONSTANT = '=';
        public const char OBJECT = '@';
        // Search order should be :
        // [Directive, VariablePool, Session, Application, Cookie, Form, Query]
        public const char WILDCARD = '*'; 
    }
}