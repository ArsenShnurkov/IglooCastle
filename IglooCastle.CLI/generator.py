import sys
import clr
import System
from time import gmtime, strftime

clr.AddReference("IglooCastle.CLI")
import IglooCastle.CLI
clr.ImportExtensions(IglooCastle.CLI)

def flatten(something):
	for x in something:
		if isinstance(x, list):
			for y in flatten(x):
				yield y
		else:
			yield x


# Some naming conventions:
# 'type_element' is a TypeElement instance
# 'dotnet_type'  is a .NET Type instance
# 'type'         is either a 'type_element' or a 'dotnet_type'

#
# generated filenames
#

class FilenameProvider:
	def __init__(self, documentation):
		self.documentation = documentation

	def type(self, dotnet_type):
		if dotnet_type.IsGenericType and not dotnet_type.IsGenericTypeDefinition:
			t = dotnet_type.GetGenericTypeDefinition()
		else:
			t = dotnet_type

		return "T_" + t.FullName + ".html"

	def property(self, dotnet_type, property):
		if dotnet_type.IsGenericType and not dotnet_type.IsGenericTypeDefinition:
			t = dotnet_type.GetGenericTypeDefinition()
		else:
			t = dotnet_type

		return "P_" + t.FullName + "." + property.Name + ".html"

	def namespace(self, str):
		return "N_" + str + ".html"


class HtmlTemplate:
	def __init__(self):
		self.title  = ""
		self.h1     = ""
		self.header = ""
		self.main   = ""
		self.footer = ""
		self.file  = ""

	def render(self):
		return """<html>
				<head>
					<title>%s</title>
				</head>
				<body>
					<h1>%s</h1>
					<!-- header -->
					<header>
					%s
					</header>
					<!-- main area -->
					<section>
					%s
					</section>
					<!-- footer -->
					<footer>
					%s
					</footer>
				</body>
			</html>
	""" % (self.title, self.h1 or self.title, self.header, self.main, self.footer)

	def write_to(self, file = None):
		f = open(file or self.file, 'w')
		f.write(self.render())
		f.close()

	@staticmethod
	def fmt_non_empty(template, contents):
		"""Formats the contents into template, if the contents are not empty."""
		if len(contents):
			return template % contents
		else:
			return ""


class TypeHelper:
	def __init__(self, documentation, filenameProvider, dotnet_type):
		self.documentation    = documentation
		self.filenameProvider = filenameProvider
		self.dotnet_type       = dotnet_type

	def name(self):
		if self.dotnet_type.IsGenericParameter:
			# e.g. T when found inside SomeType<T>
			return self.dotnet_type.Name

		t = self.documentation.Normalize(self.dotnet_type)

		if t.IsGenericType:
			return t.FullName.Split('`')[0] + "&lt;" + ", ".join(subType.Name for subType in t.GetGenericArguments()) + "&gt;"
		else:
			return self.__sysname(t) or t.FullName

	def short_name(self):
		if self.dotnet_type.IsGenericParameter:
			# e.g. T when found inside SomeType<T>
			return self.dotnet_type.Name

		t = self.documentation.Normalize(self.dotnet_type)

		if t.IsGenericType:
			return t.Name.Split('`')[0] + "&lt;" + ", ".join(subType.Name for subType in t.GetGenericArguments()) + "&gt;"
		else:
			return self.__sysname(t) or t.Name

	def type_kind(self):
		type_kind = ""
		if self.dotnet_type.IsEnum:
			type_kind = "Enumeration"
		elif self.dotnet_type.IsValueType:
			type_kind = "Struct"
		elif self.dotnet_type.IsInterface:
			type_kind = "Interface"
		elif self.dotnet_type.IsClass:
			type_kind = "Class"
		else:
			# what else?
			type_kind = "Type"

		return type_kind

	def link(self):
		t = self.dotnet_type
		if t.IsArray:
			raise ValueError("Can not link to an array")

		if t.IsGenericType and not t.IsGenericTypeDefinition:
			raise ValueError("Can not link to closed generic types")

		if self.documentation.IsLocalType(t):
			link = self.filenameProvider.type(t)
			return link
		else:
			return None

	def __sysname(self, t):
		"""Return the alias of a system type, e.g. object instead of System.Object"""
		builtins = {
			"System.Boolean" : "bool",
			"System.Object"  : "object",
			"System.Int32"   : "int",
			"System.String"  : "string"
		}

		if t.FullName in builtins:
			return builtins[t.FullName]
		else:
			return None

class HtmlGenerator:
	def __init__(self, documentation):
		self.documentation = documentation
		self.filenameProvider    = FilenameProvider(documentation)

	def generate_index_page(self):
		pass

	def generate_namespace_page(self, namespaceElement):
		namespace = namespaceElement.Namespace
		print "Generating page for namespace %s" % namespace
		html_template        = HtmlTemplate()
		html_template.title  = namespace + " Namespace"
		html_template.header = self.nav()
		html_template.footer = self.__footer()
		html_template.file   = self.filenameProvider.namespace(namespace)
		return html_template

	def generate_namespace_pages(self):
		print "Namespaces:"
		return [self.generate_namespace_page(n) for n in self.documentation.Namespaces]

	def generate_property_page(self, type_element, property_element):
		property = property_element.Property
		property_name = property.Name

		html_template        = HtmlTemplate()
		html_template.title  = "%s %s" % (property_name, "Property")
		html_template.header = self.nav()
		html_template.main   = HtmlTemplate.fmt_non_empty("""
				<h2>Summary</h2>
				<p>%s</p>""", property_element.XmlComment.Summary())
		html_template.footer = self.__footer()
		html_template.file   = self.filenameProvider.property(type_element.Type, property)
		return html_template

	def generate_type_page(self, type_element):
		dotnet_type = type_element.Type
		print "Generating page for type %s" % dotnet_type.ToString()
		type_helper = self.__type_helper(dotnet_type)
		fullName = type_helper.name()
		type_kind = type_helper.type_kind()

		html_template        = HtmlTemplate()
		html_template.title  = "%s %s" % (fullName, type_kind)
		html_template.header = self.nav()
		html_template.main   = HtmlTemplate.fmt_non_empty("""
				<h2>Summary</h2>
				<p>%s</p>""", type_element.XmlComment.Summary()) + \
			self.base_type_section(type_element) + \
			self.interfaces_section(type_element) + \
			self.derived_types_section(type_element) + \
			self.constructors_section(type_element, type_helper) + \
			self.properties_section(type_element) + self.methods_section(type_element)
		html_template.footer = self.__footer()
		html_template.file   = self.filenameProvider.type(type_element.Type)

		result = [ html_template ]
		result.extend(self.generate_property_page(type_element, p) for p in type_element.Properties)
		return result

	def generate_type_pages(self):
		print "Types:"
		return [self.generate_type_page(type_element) for type_element in self.documentation.Types]

	def generate_nant_task_pages(self):
		print "NAnt tasks:"
		for type_element in self.documentation.Types:
			if type_element.HasAttribute('NAnt.Core.Attributes.TaskName'):
				taskName = type_element.GetAttribute('NAnt.Core.Attributes.TaskName').Name
				print "todo: generate page for nant task " + taskName

	def type_link(self, dotnet_type):
		print "type_link %s" % dotnet_type.Name

		if dotnet_type.IsArray:
			return self.type_link(dotnet_type.GetElementType()) + "[]"

		if dotnet_type.IsGenericType and not dotnet_type.IsGenericTypeDefinition:
			return self.type_link(dotnet_type.GetGenericTypeDefinition())

		typeHelper = self.__type_helper(dotnet_type)
		link = typeHelper.link()
		if link:
			print "test"
			result = "<a href=\"%s\">%s</a>" % (link, typeHelper.short_name())
			print "test 2"
		else:
			result = typeHelper.name()

		print "type_link is %s" % result
		return result

	def exclude_attribute(self, attribute):
		if (attribute.GetType().Name == "__DynamicallyInvokableAttribute"):
			return True
		return isinstance(attribute, (
			System.Runtime.CompilerServices.ExtensionAttribute,
			System.Runtime.TargetedPatchingOptOutAttribute,
			System.Security.SecuritySafeCriticalAttribute))

	def format_attribute(self, attribute):
		return "[" + self.type_link(attribute.GetType()) + "]"

	def format_attributes(self, attributes):
		if not attributes:
			return ""

		return "".join(self.format_attribute(a) for a in attributes if not self.exclude_attribute(a))

	def format_parameter(self, parameterInfo):
		attributes = parameterInfo.GetCustomAttributes(False)

		return self.format_attributes(attributes) + \
			self.type_link(parameterInfo.ParameterType) + \
			" " + parameterInfo.Name

	def format_parameters(self, something):
		return ", ".join(self.format_parameter(p) for p in something.GetParameters())

	def inherited_from(self, type_element, memberElement):
		if memberElement.DeclaringType != type_element.Type:
			inheritedLink = "(inherited from %s)" % self.type_link(memberElement.DeclaringType)
		else:
			inheritedLink = ""
		return inheritedLink

	def constructors_section(self, type_element, type_helper):
		print "constructors_section"

		def constructor_list_item(constructor_element):
			return "<li>%s(%s)</li>" % ( type_helper.short_name(), self.format_parameters(constructor_element.Constructor) )

		return HtmlTemplate.fmt_non_empty(
			"<h2>Constructors</h2><ul>%s</ul>",
			"".join(constructor_list_item(c) for c in type_element.Constructors))

	def base_type_section(self, type):
		base_type = type.BaseType
		if not base_type or base_type.FullName == "System.Object":
			return ""
		return "<p>Inherits from %s</p>" % self.type_link(base_type)

	def interfaces_section(self, type):
		interfaces = type.GetInterfaces()
		if not interfaces:
			return ""

		return "<p>Implements interfaces: %s</p>" % ", ".join(self.type_link(t) for t in interfaces)

	def derived_types_section(self, type_element):
		print "derived_types_section"

		derived_types = type_element.GetDerivedTypes()
		if not derived_types:
			return ""

		print "derived_types_section"

		return "<p>Known derived types: %s</p>" % ", ".join(self.type_link(t) for t in derived_types)

	def properties_section(self, type_element):
		def property_list_item(property_element):
			name        = property_element.Name
			ptype       = self.type_link(property_element.PropertyType)
			description = property_element.XmlComment.Summary() + " " + self.inherited_from(type_element, property_element)
			link        = self.filenameProvider.property(type_element.Type, property_element)
			return """<tr>
			<td><a href=\"%s\">%s</a></td>
			<td>%s</td>
			<td>%s</td>
			</tr>""" % (link, name, ptype, description)

		return HtmlTemplate.fmt_non_empty(
			"""<h2>Properties</h2>
			<table>
				<thead>
				<tr>
					<th>Name</th>
					<th>Type</th>
					<th>Description</th>
				</tr>
				</thead>
				<tbody>
				%s
				</tbody>
			</table>""",
			"".join(property_list_item(p) for p in type_element.Properties))

	def is_extension_method(self, method_element):
		attributes = [x
			for x in method_element.Method.GetCustomAttributes(False)
			if isinstance(x, System.Runtime.CompilerServices.ExtensionAttribute)]
		return len(attributes) > 0

	def methods_section(self, type_element):
		def method_list_item(type_element, memberElement):
			inheritedLink = self.inherited_from(type_element, memberElement)
			is_extension_method = self.is_extension_method(memberElement)
			parameters_string = ",".join(self.format_parameter(p) for p in memberElement.Method.GetParameters())
			if is_extension_method:
				parameters_string = "this " + parameters_string

			name = self.type_link(memberElement.ReturnType) + " " + \
				memberElement.Name + "(" + \
				parameters_string + \
				")"

			name = self.format_attributes(memberElement.GetCustomAttributes(False)) + name
			result = """<li>
				<dl>
					<dt>%s</dt>
					<dd>%s %s</dd>
				</dl>
			</li>""" % (name, memberElement.XmlComment.Summary(), inheritedLink)

			return result
		return HtmlTemplate.fmt_non_empty(
			"<h2>Methods</h2><ul>%s</ul>",
			"".join(method_list_item(type_element, m) for m in type_element.Methods))

	def nav(self):
		result = "<ol>"
		for n in self.documentation.Namespaces:
			result = result + "<li><span class=\"js-expander\">-</span>" + n.Namespace + " Namespace"
			result = result + "<ol>"
			for t in n.Types:
				typeHelper = self.__type_helper(t.Type)
				result = result + "<li><span class=\"js-expander\">-</span>"
				result = result + ( "<a href=\"%s\">%s</a>" % (typeHelper.link(), typeHelper.short_name() + " " + typeHelper.type_kind()) )
				result = result + "<ol>"
				if len(t.Properties):
					result = result + "<li><span class=\"js-expander\">-</span>Properties"
					result = result + "<ol>"
					for p in t.Properties:
						result = result + "<li>"
						result = result + ( "<a href=\"%s\">%s</a>" % (self.filenameProvider.property(t.Type, p), p.Property.Name) )
						result = result + "</li>" # /type property
					result = result + "</ol>" # /type properties
					result = result + "</li>" #/type properties group
				result = result + "</ol>" # /type member groups
				result = result + "</li>" # /type
			result = result + "</ol>" # /types
			result = result + "</li>" # /namespace
		result = result + "</ol>" # /namespaces

		return result

	def __type_helper(self, dotNetType):
		return TypeHelper(self.documentation, self.filenameProvider, dotNetType)

	def __footer(self):
		return """Generated by IglooCastle at
			""" + strftime("%Y-%m-%d %H:%M:%S", gmtime()) + """
			<link type=\"text/css\" rel=\"stylesheet\" href=\"style.css\" />
			<script src="jquery-1.11.1.min.js"></script>
			<script src="app.js"></script>
			</footer>"""


def Generate(documentation):
	"""Entry point for IglooCastle"""
	print "Hello from python!"
	htmlGenerator = HtmlGenerator(documentation)
	htmlGenerator.generate_index_page()

	pages = []
	pages.extend(flatten(htmlGenerator.generate_namespace_pages()))
	pages.extend(flatten(htmlGenerator.generate_type_pages()))

	for page in pages:
		page.write_to()

	htmlGenerator.generate_nant_task_pages()
