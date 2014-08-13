using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IglooCastle.CLI
{
    public class TypePrinter
    {
        private readonly Documentation _documentation;
        private readonly FilenameProvider _filenameProvider;

        public TypePrinter(Documentation documentation, FilenameProvider filenameProvider)
        {
            _documentation = documentation;
            _filenameProvider = filenameProvider;
        }

        public string Print(Type type)
        {
            string result;
            if (type.IsGenericType && !type.IsGenericTypeDefinition)
            {
                // concrete generic type
                Type genericTypeDefinition = type.GetGenericTypeDefinition();
                result = BeginPrint(genericTypeDefinition);
                result += string.Join(", ", type.GetGenericArguments().Select(Print));
                result += EndPrint(genericTypeDefinition);
            }
            else
            {
                result = BeginPrint(type);
                result += EndPrint(type);
            }

            return result;
        }

        public virtual string BeginPrint(Type type)
        {
            if (type.IsGenericTypeDefinition)
            {
                return type.FullName.Split('`')[0] + "&lt;";
            }

            if (_documentation.IsLocalType(type))
            {
                return string.Format("<a href=\"{0}\">{1}</a>", _filenameProvider.Filename(type), type.Name);
            }

            return type.FullName;
        }

        public virtual string EndPrint(Type type)
        {
            if (type.IsGenericTypeDefinition)
            {
                return "&gt;";
            }

            return string.Empty;
        }
    }
}
