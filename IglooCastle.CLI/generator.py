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

#
# generated filenames
#

class FilenameProvider:
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
	def __init__(self, documentation, filename_provider, type):
		self.documentation     = documentation
		self.filename_provider = filename_provider
		self.type              = type

	def name(self):
		if self.type.IsGenericParameter:
			# e.g. T when found inside SomeType<T>
			return self.type.Name

		t = self.documentation.Normalize(self.type)

		if t.IsGenericType:
			return t.FullName.Split('`')[0] + "&lt;" + ", ".join(subType.Name for subType in t.GetGenericArguments()) + "&gt;"
		else:
			return self.__sysname(t) or t.FullName

	def short_name(self):
		if self.type.IsGenericParameter:
			# e.g. T when found inside SomeType<T>
			return self.type.Name

		t = self.documentation.Normalize(self.type)

		if t.IsGenericType:
			return t.Name.Split('`')[0] + "&lt;" + ", ".join(subType.Name for subType in t.GetGenericArguments()) + "&gt;"
		else:
			return self.__sysname(t) or t.Name

	def type_kind(self):
		type_kind = ""
		if self.type.IsEnum:
			type_kind = "Enumeration"
		elif self.type.IsValueType:
			type_kind = "Struct"
		elif self.type.IsInterface:
			type_kind = "Interface"
		elif self.type.IsClass:
			type_kind = "Class"
		else:
			# what else?
			type_kind = "Type"

		return type_kind

	def link(self):
		t = self.type
		if t.IsArray:
			raise ValueError("Can not link to an array")

		if t.IsGenericType and not t.IsGenericTypeDefinition:
			raise ValueError("Can not link to closed generic types")

		if self.documentation.IsLocalType(t):
			link = self.filename_provider.type(t)
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

class NavigationNode:
	def __init__(self):
		self.filename_provider = FilenameProvider()
		self.EXPANDER = "<span class=\"js-expander\">-</span>"
		pass

	def nav_html(self):
		children_html = "".join(child.nav_html() for child in self.children())
		node_html     = "<a href=\"%s\">%s</a>" % (self.href(), self.text())
		if len(children_html):
			return "<li>%s %s<ol>%s</ol></li>" % (self.EXPANDER, node_html, children_html)
		else:
			return "<li>%s</li>" % node_html

	def href(self):
		raise ValueError('You need to override href()')

	def text(self):
		raise ValueError('You need to override text()')

	def children(self):
		return []


class NavigationNamespaceNode(NavigationNode):
	def __init__(self, namespace_element):
		NavigationNode.__init__(self)
		self.namespace_element = namespace_element

	def href(self):
		return self.filename_provider.namespace(self.namespace_element.Namespace)

	def text(self):
		return self.namespace_element.Namespace + " Namespace"

	def children(self):
		return [ NavigationTypeNode(t) for t in self.namespace_element.Types ]


class NavigationTypeNode(NavigationNode):
	def __init__(self, type_element):
		NavigationNode.__init__(self)
		self.type_element = type_element

	def href(self):
		return self.filename_provider.type(self.type_element)

	def text(self):
		type_helper = TypeHelper(self.type_element.Documentation, self.filename_provider, self.type_element)
		return type_helper.short_name() + " " + type_helper.type_kind()

	def children(self):
		result = []
		if len(self.type_element.DeclaredProperties):
			result.append(NavigationPropertiesNode(self.type_element))

		if len(self.type_element.DeclaredMethods):
			result.append(NavigationMethodsNode(self.type_element))

		return result


class NavigationPropertiesNode(NavigationNode):
	def __init__(self, type_element):
		NavigationNode.__init__(self)
		self.type_element = type_element

	def href(self):
		return self.filename_provider.properties(self.type_element)

	def text(self):
		return "Properties"

	def children(self):
		return [ NavigationPropertyNode(p) for p in self.type_element.DeclaredProperties ]


class NavigationMethodsNode(NavigationNode):
	def __init__(self, type_element):
		NavigationNode.__init__(self)
		self.type_element = type_element

	def href(self):
		return self.filename_provider.methods(self.type_element)

	def text(self):
		return "Methods"

	def children(self):
		return [ NavigationMethodNode(m) for m in self.type_element.DeclaredMethods ]


class NavigationPropertyNode(NavigationNode):
	def __init__(self, property_element):
		NavigationNode.__init__(self)
		self.property_element = property_element

	def href(self):
		return self.filename_provider.property(self.property_element.OwnerType, self.property_element)

	def text(self):
		return self.property_element.Name


class NavigationMethodNode(NavigationNode):
	def __init__(self, method_element):
		NavigationNode.__init__(self)
		self.method_element = method_element

	def href(self):
		return self.filename_provider.method(self.method_element.OwnerType, self.method_element)

	def text(self):
		return self.method_element.Name


class HtmlGenerator:
	def __init__(self, documentation):
		self.documentation      = documentation
		self.filename_provider  = FilenameProvider()

		# The HTML of the left-side navigation tree
		self.__nav              = None

		# The top-level navigation nodes that comprise the left-side navigation tree
		self.__navigation_nodes = None

	def generate_index_page(self):
		pass

	def generate_namespace_page(self, namespaceElement):
		namespace = namespaceElement.Namespace
		print "Generating page for namespace %s" % namespace
		html_template        = HtmlTemplate()
		html_template.title  = namespace + " Namespace"
		html_template.nav    = self.nav()
		html_template.footer = self.__footer()
		html_template.file   = self.filename_provider.namespace(namespace)
		return html_template

	def generate_namespace_pages(self):
		print "Namespaces:"
		return [self.generate_namespace_page(n) for n in self.documentation.Namespaces]

	def generate_properties_page(self, type, type_helper):
		html_template        = HtmlTemplate()
		html_template.title  = "%s Properties" % type_helper.name()
		html_template.h1     = "%s Properties" % type_helper.short_name()
		html_template.nav    = self.nav()
		html_template.main   = "<ol>%s</ol>" % "".join("<li>" + p.Name + "</li>" for p in type.Properties)
		html_template.footer = self.__footer()
		html_template.file   = self.filename_provider.properties(type)
		return html_template

	def generate_property_page(self, type, type_helper, property):
		property_name = property.Name

		def syntax():
			getter = property.GetGetMethod(True) if property.CanRead else None
			setter = property.GetSetMethod(True) if property.CanWrite else None
			getter_attr = getter.Attributes if getter else None
			setter_attr = setter.Attributes if setter else None

			getter_str = ""
			if getter_attr:
				getter_str = "get;"

			setter_str = setter_attr.ToString() + " set;" if setter_attr else ""

			access = ""
			if getter_attr and not type.IsInterface: # all interface members are public
				access = getter_attr.ToString()

			return "%s %s %s { %s %s }" % (access, self.type_link(property.PropertyType), property.Name, getter_str, setter_str)

		html_template        = HtmlTemplate()
		html_template.title  = "%s %s" % (property_name, "Property")
		html_template.h1     = "%s.%s %s" % (type_helper.short_name(), property_name, "Property")
		html_template.nav    = self.nav()
		html_template.main   = HtmlTemplate.fmt_non_empty("""
				<h2>Summary</h2>
				<p>%s</p>
				""", property.XmlComment.Summary()) + \
				"""
				<h2>Syntax</h2>
				<code>
				%s
				</code>
				""" % syntax()


		html_template.footer = self.__footer()
		html_template.file   = self.filename_provider.property(type, property)
		return html_template

	def generate_methods_page(self, type, type_helper):
		html_template        = HtmlTemplate()
		html_template.title  = "%s Methods" % type_helper.name()
		html_template.h1     = "%s Methods" % type_helper.short_name()
		html_template.nav    = self.nav()
		html_template.main   = "<ol>%s</ol>" % "".join("<li>" + m.Name + "</li>" for m in type.Methods)
		html_template.footer = self.__footer()
		html_template.file   = self.filename_provider.methods(type)
		return html_template

	def generate_method_page(self, type, type_helper, method):
		method_name = method.Name

		def syntax():
			method_attr = method.Attributes
			access = ""
			if method_attr and not type.IsInterface: # all interface members are public
				access = method_attr.ToString()

			parameters = ", ".join( self.type_link(p.ParameterType) + " " + p.Name for p in method.GetParameters() )

			return "%s %s %s (%s)" % (access, self.type_link(method.ReturnType), method.Name, parameters)

		html_template        = HtmlTemplate()
		html_template.title  = "%s %s" % (method_name, "Method")
		html_template.h1     = "%s.%s %s" % (type_helper.short_name(), method_name, "Method")
		html_template.nav    = self.nav()
		html_template.main   = HtmlTemplate.fmt_non_empty("""
				<h2>Summary</h2>
				<p>%s</p>
				""", method.XmlComment.Summary()) + \
				"""
				<h2>Syntax</h2>
				<code>
				%s
				</code>
				""" % syntax()


		html_template.footer = self.__footer()
		html_template.file   = self.filename_provider.method(type, method)
		return html_template

	def generate_type_page(self, type):
		print "Generating page for type %s" % type.ToString()
		type_helper = self.__type_helper(type)
		fullName = type_helper.name()
		type_kind = type_helper.type_kind()

		html_template        = HtmlTemplate()
		html_template.title  = "%s %s" % (fullName, type_kind)
		html_template.h1     = "%s %s" % (type_helper.short_name(), type_kind)
		html_template.nav    = self.nav()
		html_template.main   = HtmlTemplate.fmt_non_empty("""
				<h2>Summary</h2>
				<p>%s</p>""", type.XmlComment.Summary()) + \
			self.base_type_section(type) + \
			self.interfaces_section(type) + \
			self.derived_types_section(type) + \
			self.constructors_section(type, type_helper) + \
			self.properties_section(type) + self.methods_section(type)
		html_template.footer = self.__footer()
		html_template.file   = self.filename_provider.type(type)

		result = [ html_template ]
		result.append(self.generate_properties_page(type, type_helper))
		result.extend(self.generate_property_page(type, type_helper, p) for p in type.DeclaredProperties)
		result.append(self.generate_methods_page(type, type_helper))
		result.extend(self.generate_method_page(type, type_helper, m) for m in type.DeclaredMethods)
		return result

	def generate_type_pages(self):
		print "Types:"
		return [self.generate_type_page(type) for type in self.documentation.Types]

	def generate_nant_task_pages(self):
		print "NAnt tasks:"
		for type in self.documentation.Types:
			if type.HasAttribute('NAnt.Core.Attributes.TaskName'):
				taskName = type.GetAttribute('NAnt.Core.Attributes.TaskName').Name
				print "todo: generate page for nant task " + taskName

	def type_link(self, type):
		print "type_link %s" % type.Name

		if type.IsArray:
			return self.type_link(type.GetElementType()) + "[]"

		if type.IsGenericType and not type.IsGenericTypeDefinition:
			return self.type_link(type.GetGenericTypeDefinition())

		typeHelper = self.__type_helper(type)
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
		if attribute.GetType().Name == "__DynamicallyInvokableAttribute":
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

	def inherited_from(self, type, memberElement):
		if memberElement.IsDeclaredIn(type):
			inheritedLink = "(inherited from %s)" % self.type_link(memberElement.DeclaringType)
		else:
			inheritedLink = ""
		return inheritedLink

	def constructors_section(self, type, type_helper):

		def constructor_list_item(constructor):
			return "<li>%s(%s)</li>" % ( type_helper.short_name(), self.format_parameters(constructor) )

		return HtmlTemplate.fmt_non_empty(
			"<h2>Constructors</h2><ul>%s</ul>",
			"".join(constructor_list_item(c) for c in type.Constructors))

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

	def derived_types_section(self, type):
		derived_types = type.GetDerivedTypes()
		if not derived_types:
			return ""

		return "<p>Known derived types: %s</p>" % ", ".join(self.type_link(t) for t in derived_types)

	def properties_section(self, type):
		def property_list_item(property):
			name        = property.Name
			ptype       = self.type_link(property.PropertyType)
			description = property.XmlComment.Summary() + " " + self.inherited_from(type, property)
			link        = self.filename_provider.property(type, property)
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
			"".join(property_list_item(p) for p in type.Properties))

	def is_extension_method(self, method):
		attributes = [x
			for x in method.GetCustomAttributes(False)
			if isinstance(x, System.Runtime.CompilerServices.ExtensionAttribute)]
		return len(attributes) > 0

	def methods_section(self, type):
		def method_list_item(type, memberElement):
			inheritedLink = self.inherited_from(type, memberElement)
			is_extension_method = self.is_extension_method(memberElement)
			parameters_string = ",".join(self.format_parameter(p) for p in memberElement.GetParameters())
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
			"".join(method_list_item(type, m) for m in type.Methods))

	def nav(self):
		if not self.__nav:
			print "Generating navigation"
			self.__navigation_nodes = [ NavigationNamespaceNode(n) for n in self.documentation.Namespaces ]
			self.__nav = "<ol>%s</ol>" % "".join(n.nav_html() for n in self.__navigation_nodes)

		return self.__nav

	def __type_helper(self, type):
		return TypeHelper(self.documentation, self.filename_provider, type)

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
