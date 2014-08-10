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

def escape(str):
	return str.Replace('`', '_')

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

	def namespace(self, str):
		return "N_" + str + ".html"

	def type(self, type):
		return "T_" + self.__type_filename(type) + ".html"

	def properties(self, type):
		return "Properties_" + self.__type_filename(type) + ".html"

	def property(self, type, property):
		return "P_" + self.__type_filename(type) + "." + property.Name + ".html"

	def methods(self, type):
		return "Methods_" + self.__type_filename(type) + ".html"

	def method(self, type, method):
		def parameters():
			parameters = method.GetParameters()
			if not len(parameters):
				return ""

			return "-" + "_".join(p.ParameterType.FullName for p in parameters)

		return "M_" + self.__type_filename(type) + "." + method.Name + parameters() + ".html"

	def __type_filename(self, type):
		if type.IsGenericType and not type.IsGenericTypeDefinition:
			type = type.GetGenericTypeDefinition()

		return escape(type.FullName)


class HtmlTemplate:
	def __init__(self):
		self.title  = ""
		self.h1     = ""
		self.nav    = ""
		self.main   = ""
		self.footer = ""
		self.file   = ""

	def render(self):
		return """<html>
				<head>
					<title>%s</title>
				</head>
				<body>
					<!-- left side navigation -->
					<nav>
						%s
					</nav>
					<!-- main area -->
					<section>
						<h1>%s</h1>
						%s
					</section>
					<!-- footer -->
					<footer>
						%s
					</footer>
				</body>
			</html>
	""" % (self.title, self.nav, self.h1 or self.title, self.main, self.footer)

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
		html_template.nav    = self.nav()
		html_template.footer = self.__footer()
		html_template.file   = self.filenameProvider.namespace(namespace)
		return html_template

	def generate_namespace_pages(self):
		print "Namespaces:"
		return [self.generate_namespace_page(n) for n in self.documentation.Namespaces]

	def generate_properties_page(self, type_element, type_helper):
		html_template        = HtmlTemplate()
		html_template.title  = "%s Properties" % type_helper.name()
		html_template.h1     = "%s Properties" % type_helper.short_name()
		html_template.nav    = self.nav()
		html_template.main   = "<ol>%s</ol>" % "".join("<li>" + p.Name + "</li>" for p in type_element.Properties)
		html_template.footer = self.__footer()
		html_template.file   = self.filenameProvider.properties(type_element.Type)
		return html_template

	def generate_property_page(self, type_element, type_helper, property_element):
		property = property_element.Property
		property_name = property.Name

		def syntax():
			getter = property_element.Property.GetGetMethod(True) if property_element.CanRead else None
			setter = property_element.Property.GetSetMethod(True) if property_element.CanWrite else None
			getter_attr = getter.Attributes if getter else None
			setter_attr = setter.Attributes if setter else None

			getter_str = ""
			if getter_attr:
				getter_str = "get;"

			setter_str = setter_attr.ToString() + " set;" if setter_attr else ""

			access = ""
			if getter_attr and not type_element.IsInterface: # all interface members are public
				access = getter_attr.ToString()

			return "%s %s %s { %s %s }" % (access, self.type_link(property_element.PropertyType), property_element.Name, getter_str, setter_str)

		html_template        = HtmlTemplate()
		html_template.title  = "%s %s" % (property_name, "Property")
		html_template.h1     = "%s.%s %s" % (type_helper.short_name(), property_name, "Property")
		html_template.nav    = self.nav()
		html_template.main   = HtmlTemplate.fmt_non_empty("""
				<h2>Summary</h2>
				<p>%s</p>
				""", property_element.XmlComment.Summary()) + \
				"""
				<h2>Syntax</h2>
				<code>
				%s
				</code>
				""" % syntax()


		html_template.footer = self.__footer()
		html_template.file   = self.filenameProvider.property(type_element.Type, property_element)
		return html_template

	def generate_methods_page(self, type_element, type_helper):
		html_template        = HtmlTemplate()
		html_template.title  = "%s Methods" % type_helper.name()
		html_template.h1     = "%s Methods" % type_helper.short_name()
		html_template.nav    = self.nav()
		html_template.main   = "<ol>%s</ol>" % "".join("<li>" + m.Name + "</li>" for m in type_element.Methods)
		html_template.footer = self.__footer()
		html_template.file   = self.filenameProvider.methods(type_element.Type)
		return html_template

	def generate_method_page(self, type_element, type_helper, method_element):
		method = method_element.Method
		method_name = method.Name

		def syntax():
			method_attr = method.Attributes
			access = ""
			if method_attr and not type_element.IsInterface: # all interface members are public
				access = method_attr.ToString()

			parameters = ", ".join( self.type_link(p.ParameterType) + " " + p.Name for p in method_element.GetParameters() )

			return "%s %s %s (%s)" % (access, self.type_link(method_element.ReturnType), method_element.Name, parameters)

		html_template        = HtmlTemplate()
		html_template.title  = "%s %s" % (method_name, "Method")
		html_template.h1     = "%s.%s %s" % (type_helper.short_name(), method_name, "Method")
		html_template.nav    = self.nav()
		html_template.main   = HtmlTemplate.fmt_non_empty("""
				<h2>Summary</h2>
				<p>%s</p>
				""", method_element.XmlComment.Summary()) + \
				"""
				<h2>Syntax</h2>
				<code>
				%s
				</code>
				""" % syntax()


		html_template.footer = self.__footer()
		html_template.file   = self.filenameProvider.method(type_element.Type, method_element)
		return html_template

	def generate_type_page(self, type_element):
		dotnet_type = type_element.Type
		print "Generating page for type %s" % dotnet_type.ToString()
		type_helper = self.__type_helper(dotnet_type)
		fullName = type_helper.name()
		type_kind = type_helper.type_kind()

		html_template        = HtmlTemplate()
		html_template.title  = "%s %s" % (fullName, type_kind)
		html_template.nav    = self.nav()
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
		result.append(self.generate_properties_page(type_element, type_helper))
		result.extend(self.generate_property_page(type_element, type_helper, p) for p in type_element.Properties)
		result.append(self.generate_methods_page(type_element, type_helper))
		result.extend(self.generate_method_page(type_element, type_helper, m) for m in type_element.Methods)
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
			# print "test"
			result = "<a href=\"%s\">%s</a>" % (link, typeHelper.short_name())
			# print "test 2"
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
		return "<ol>%s</ol>" % "".join(self.nav_namespace(n) for n in self.documentation.Namespaces)

	def nav_namespace(self, n):
		result = "<li><span class=\"js-expander\">-</span><a href=\"%s\">%s</a>" % ( self.filenameProvider.namespace(n.Namespace), n.Namespace + " Namespace" )
		result += ( "<ol>%s</ol>" % "".join(self.nav_type(n, t) for t in n.Types) )
		result += "</ol>" # /types
		result += "</li>" # /namespace
		return result

	def nav_type(self, n, t):
		typeHelper = self.__type_helper(t.Type)
		result = "<li><span class=\"js-expander\">-</span>"
		result += ( "<a href=\"%s\">%s</a>" % (typeHelper.link(), typeHelper.short_name() + " " + typeHelper.type_kind()) )
		result += "<ol>"

		if len(t.Properties):
			result += "<li><span class=\"js-expander\">-</span><a href=\"%s\">Properties</a>" % self.filenameProvider.properties(t.Type)
			result += "<ol>"
			for p in t.Properties:
				result += "<li>"
				result += ( "<a href=\"%s\">%s</a>" % (self.filenameProvider.property(t.Type, p), p.Name) )
				result += "</li>" # /type property
			result += "</ol>" # /type properties
			result += "</li>" #/type properties group

		if (len(t.Methods)):
			result += "<li><span class=\"js-expander\">-</span><a href=\"%s\">Methods</a>" % self.filenameProvider.methods(t.Type)
			result += "<ol>"
			for m in t.Methods:
				result += "<li>"
				result += ( "<a href=\"%s\">%s</a>" % (self.filenameProvider.method(t.Type, m), m.Name) )
				result += "</li>" # /type method
			result += "</ol>" # /type methods
			result += "</li>" #/type methods group

		result += "</ol>" # /type member groups
		result += "</li>" # /type
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
