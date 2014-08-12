import sys
import clr
import System
from time import gmtime, strftime

clr.AddReference("IglooCastle.CLI")
import IglooCastle.CLI
clr.ImportExtensions(IglooCastle.CLI)
from IglooCastle.CLI import FilenameProvider

def flatten(something):
	for x in something:
		if isinstance(x, list):
			for y in flatten(x):
				yield y
		else:
			yield x

def escape(str):
	return str.replace("`", "%60")

def a(href, text):
	return '<a href="%s">%s</a>' % (escape(href), text)

class HtmlTemplate:
	def __init__(self):
		self.title  = ""
		self.h1     = ""
		self.nav    = ""
		self.main   = ""
		self.footer = ""

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

	def write_to(self, file):
		print "Writing file %s" % file
		f = open(file, 'w')
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
			return IglooCastle.CLI.SystemTypes.Alias(t) or t.FullName

	def short_name(self):
		if self.type.IsGenericParameter:
			# e.g. T when found inside SomeType<T>
			return self.type.Name

		t = self.documentation.Normalize(self.type)

		if t.IsGenericType:
			return t.Name.Split('`')[0] + "&lt;" + ", ".join(subType.Name for subType in t.GetGenericArguments()) + "&gt;"
		else:
			return IglooCastle.CLI.SystemTypes.Alias(t) or t.Name

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
			link = self.filename_provider.Filename(t)
			return link
		elif t.Namespace == "System" or t.Namespace.startswith("System."):
			return "http://msdn.microsoft.com/en-us/library/%s%%28v=vs.110%%29.aspx" % t.FullName.lower()
		else:
			return None


class NavigationNode:
	def __init__(self):
		self.filename_provider = FilenameProvider()
		self.EXPANDER = '<span class="js-expander">-</span>'
		pass

	def nav_html(self):
		children_html = self.children_nav_html()
		node_html     = a(self.href(), self.text())
		if len(children_html):
			return "<li>%s %s<ol>%s</ol></li>" % (self.EXPANDER, node_html, children_html)
		else:
			return "<li>%s</li>" % node_html

	def contents_html_template(self):
		return None

	def visit(self, f):
		"""Calls f for every node in the tree."""
		f(self)
		for child in self.children():
			child.visit(f)

	def href(self):
		raise ValueError('You need to override href()')

	def text(self):
		raise ValueError('You need to override text()')

	def children(self):
		return []

	def children_nav_html(self):
		return "".join(child.nav_html() for child in self.children())

	def type_link(self, type):
		print "type_link %s" % type.Name

		if type.IsArray:
			return self.type_link(type.GetElementType()) + "[]"

		if type.IsGenericType and not type.IsGenericTypeDefinition:
			return self.type_link(type.GetGenericTypeDefinition())

		typeHelper = self.type_helper(type)
		link = typeHelper.link()
		if link:
			# print "test"
			result = a(link, typeHelper.short_name())
			# print "test 2"
		else:
			result = typeHelper.name()

		print "type_link is %s" % result
		return result

	def type_helper(self, type):
		return TypeHelper(self.documentation(), self.filename_provider, type)

	def property_link(self, property_element):
		t = property_element.DeclaringType
		if not property_element.Documentation.IsLocalType(t):
			return property_element.Name
		else:
			return a(self.filename_provider.Filename(property_element), property_element.Name)

	def method_link(self, method_element):
		t = method_element.DeclaringType
		if not method_element.Documentation.IsLocalType(t):
			return method_element.Name
		else:
			return a(self.filename_provider.Filename(method_element), method_element.Name)

	def property_type_link(self, dotnet_type):
		print "property_type_link %s" % dotnet_type.Name

		if dotnet_type.IsArray:
			return self.property_type_link(dotnet_type.GetElementType()) + "[]"

		if dotnet_type.IsGenericType and not dotnet_type.IsGenericTypeDefinition:
			result = self.property_type_link(dotnet_type.GetGenericTypeDefinition())
			result += "&lt;"
			for generic_argument in dotnet_type.GetGenericArguments():
				result += self.property_type_link(generic_argument)
				result += ", "
			result += "&gt;"

			return result


		type_helper = self.type_helper(dotnet_type)
		link = type_helper.link()
		if link:
			result = a(link, type_helper.short_name())
		else:
			result = type_helper.name()

		print "property_type_link is %s" % result
		return result


class NavigationDocumentationNode(NavigationNode):
	def __init__(self, documentation):
		NavigationNode.__init__(self)
		self.documentation = documentation

	def href(self):
		return None

	def text(self):
		return None

	def children(self):
		return [ NavigationNamespaceNode(n) for n in self.documentation.Namespaces ]

	def nav_html(self):
		return "<ol>%s</ol>" % self.children_nav_html()

	def documentation(self):
		return self.documentation


class NavigationNamespaceNode(NavigationNode):
	def __init__(self, namespace_element):
		NavigationNode.__init__(self)
		self.namespace_element = namespace_element

	def href(self):
		return self.filename_provider.Filename(self.namespace_element)

	def text(self):
		return self.namespace_element.Namespace + " Namespace"

	def children(self):
		return [ NavigationTypeNode(t) for t in self.namespace_element.Types ]

	def contents_html_template(self):
		print "Generating page for namespace %s" % self.namespace_element.Namespace
		html_template = HtmlTemplate()
		html_template.title = self.text()
		html_template.main  = \
			HtmlTemplate.fmt_non_empty("""
				<h2>Classes</h2>
				<ol>
					%s
				</ol>
				""", "".join("<li>" + self.type_link(t) + "</li>" for t in self.namespace_element.Types if t.IsClass)) + \
			HtmlTemplate.fmt_non_empty("""
				<h2>Interfaces</h2>
				<ol>
					%s
				</ol>
				""", "".join("<li>" + self.type_link(t) + "</li>" for t in self.namespace_element.Types if t.IsInterface)) + \
			HtmlTemplate.fmt_non_empty("""
				<h2>Enumerations</h2>
				<ol>
					%s
				</ol>
				""", "".join("<li>" + self.type_link(t) + "</li>" for t in self.namespace_element.Types if t.IsEnum))

		return html_template

	def documentation(self):
		return self.namespace_element.Documentation


class NavigationTypeNode(NavigationNode):
	def __init__(self, type_element):
		NavigationNode.__init__(self)
		self.type_element = type_element

	def href(self):
		return self.filename_provider.Filename(self.type_element)

	def text(self):
		type_helper = self.type_helper(self.type_element)
		return type_helper.short_name() + " " + type_helper.type_kind()

	def children(self):
		result = []
		if len(self.type_element.DeclaredProperties):
			result.append(NavigationPropertiesNode(self.type_element))

		if len(self.type_element.DeclaredMethods):
			result.append(NavigationMethodsNode(self.type_element))

		return result

	def contents_html_template(self):
		print "Generating page for type %s" % self.type_element.FullName
		type_helper = self.type_helper(self.type_element)
		fullName = type_helper.name()
		type_kind = type_helper.type_kind()

		html_template        = HtmlTemplate()
		html_template.title  = "%s %s" % (fullName, type_kind)
		html_template.h1     = "%s %s" % (type_helper.short_name(), type_kind)
		html_template.main   = HtmlTemplate.fmt_non_empty("""
				<h2>Summary</h2>
				<p>%s</p>""", self.type_element.XmlComment.Summary()) + \
			self.base_type_section() + \
			self.interfaces_section() + \
			self.derived_types_section() + \
			self.constructors_section(type_helper) + \
			self.properties_section() + \
			self.methods_section() + \
			self.extension_methods_section()

		return html_template

	def constructors_section(self, type_helper):

		def constructor_list_item(constructor):
			return "<li>%s(%s)</li>" % ( type_helper.short_name(), self.format_parameters(constructor) )

		return HtmlTemplate.fmt_non_empty(
			"<h2>Constructors</h2><ul>%s</ul>",
			"".join(constructor_list_item(c) for c in self.type_element.Constructors))

	def base_type_section(self):
		base_type = self.type_element.BaseType
		if not base_type or base_type.FullName == "System.Object":
			return ""

		return "<p>Inherits from %s</p>" % self.type_link(base_type)

	def interfaces_section(self):
		interfaces = self.type_element.GetInterfaces()
		if not interfaces:
			return ""

		return "<p>Implements interfaces: %s</p>" % ", ".join(self.type_link(t) for t in interfaces)

	def derived_types_section(self):
		derived_types = self.type_element.GetDerivedTypes()
		if not derived_types:
			return ""

		return "<p>Known derived types: %s</p>" % ", ".join(self.type_link(t) for t in derived_types)

	def inherited_from(self, member_element):
		if member_element.IsDeclaredIn(member_element.OwnerType):
			inherited_link = ""
		else:
			inherited_link = "(inherited from %s)" % self.type_link(member_element.DeclaringType)
		return inherited_link

	def properties_section(self):
		def property_list_item(property):
			name        = property.Name
			ptype       = self.property_type_link(property.PropertyType)
			description = property.XmlComment.Summary() + " " + self.inherited_from(property)
			link        = self.filename_provider.Filename(property)
			return """<tr>
			<td>%s</td>
			<td>%s</td>
			<td>%s</td>
			</tr>""" % (self.property_link(property), ptype, description)

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
			"".join(property_list_item(p) for p in self.type_element.Properties))

	def is_extension_method(self, method):
		attributes = [x
			for x in method.GetCustomAttributes(False)
			if isinstance(x, System.Runtime.CompilerServices.ExtensionAttribute)]
		return len(attributes) > 0

	def methods_section(self):
		def method_list_item(method_element):
			inheritedLink = self.inherited_from(method_element)
			is_extension_method = self.is_extension_method(method_element)
			parameters_string = ",".join(self.format_parameter(p) for p in method_element.GetParameters())
			if is_extension_method:
				parameters_string = "this " + parameters_string

			name = self.type_link(method_element.ReturnType) + " " + \
				method_element.Name + "(" + \
				parameters_string + \
				")"

			name = self.format_attributes(method_element.GetCustomAttributes(False)) + name
			result = """<li>
				<dl>
					<dt>%s</dt>
					<dd>%s %s</dd>
				</dl>
			</li>""" % (self.method_link(method_element), method_element.XmlComment.Summary(), inheritedLink)

			return result
		return HtmlTemplate.fmt_non_empty(
			"<h2>Methods</h2><ul>%s</ul>",
			"".join(method_list_item(m) for m in self.type_element.Methods))

	def extension_methods_section(self):
		return "<h2>Extension methods</h2><p>todo</p>"

	def documentation(self):
		return self.type_element.Documentation

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


class NavigationPropertiesNode(NavigationNode):
	def __init__(self, type_element):
		NavigationNode.__init__(self)
		self.type_element = type_element

	def href(self):
		return self.filename_provider.Filename(self.type_element, "Properties")

	def text(self):
		return "Properties"

	def children(self):
		return [ NavigationPropertyNode(p) for p in self.type_element.DeclaredProperties ]

	def documentation(self):
		return self.type_element.Documentation

	def contents_html_template(self):
		type_helper = self.type_helper(self.type_element)
		html_template        = HtmlTemplate()
		html_template.title  = "%s Properties" % type_helper.name()
		html_template.h1     = "%s Properties" % type_helper.short_name()
		html_template.main   = "<ol>%s</ol>" % "".join("<li>" + self.property_link(p) + "</li>" for p in self.type_element.Properties)
		return html_template


class NavigationMethodsNode(NavigationNode):
	def __init__(self, type_element):
		NavigationNode.__init__(self)
		self.type_element = type_element

	def href(self):
		return self.filename_provider.Filename(self.type_element, "Methods")

	def text(self):
		return "Methods"

	def children(self):
		return [ NavigationMethodNode(m) for m in self.type_element.DeclaredMethods ]

	def documentation(self):
		return self.type_element.Documentation

	def contents_html_template(self):
		type_helper = self.type_helper(self.type_element)
		html_template        = HtmlTemplate()
		html_template.title  = "%s Methods" % type_helper.name()
		html_template.h1     = "%s Methods" % type_helper.short_name()
		html_template.main   = "<ol>%s</ol>" % "".join("<li>" + self.method_link(m) + "</li>" for m in self.type_element.Methods)
		return html_template


class NavigationPropertyNode(NavigationNode):
	def __init__(self, property_element):
		NavigationNode.__init__(self)
		self.property_element = property_element

	def href(self):
		return self.filename_provider.Filename(self.property_element)

	def text(self):
		return self.property_element.Name

	def documentation(self):
		return self.property_element.Documentation

	def contents_html_template(self):
		property_name = self.property_element.Name
		type_helper = self.type_helper(self.property_element.OwnerType)
		def syntax():
			getter = self.property_element.GetGetMethod(True) if self.property_element.CanRead else None
			setter = self.property_element.GetSetMethod(True) if self.property_element.CanWrite else None
			getter_attr = getter.Attributes if getter else None
			setter_attr = setter.Attributes if setter else None

			getter_str = ""
			if getter_attr:
				getter_str = "get;"

			setter_str = setter_attr.ToString() + " set;" if setter_attr else ""

			access = ""
			if getter_attr and not self.property_element.OwnerType.IsInterface: # all interface members are public
				access = getter_attr.ToString()

			return "%s %s %s { %s %s }" % (access, self.type_link(self.property_element.PropertyType), self.property_element.Name, getter_str, setter_str)

		html_template        = HtmlTemplate()
		html_template.title  = "%s %s" % (property_name, "Property")
		html_template.h1     = "%s.%s %s" % (type_helper.short_name(), property_name, "Property")
		html_template.main   = HtmlTemplate.fmt_non_empty("""
				<h2>Summary</h2>
				<p>%s</p>
				""", self.property_element.XmlComment.Summary()) + \
				"""
				<h2>Syntax</h2>
				<code>
				%s
				</code>
				""" % syntax()
		return html_template


class NavigationMethodNode(NavigationNode):
	def __init__(self, method_element):
		NavigationNode.__init__(self)
		self.method_element = method_element

	def href(self):
		return self.filename_provider.Filename(self.method_element)

	def text(self):
		return self.method_element.Name

	def documentation(self):
		return self.method_element.Documentation

	def contents_html_template(self):
		method_name = self.method_element.Name
		type_helper = self.type_helper(self.method_element.OwnerType)
		html_template        = HtmlTemplate()
		html_template.title  = "%s %s" % (method_name, "Method")
		html_template.h1     = "%s.%s %s" % (type_helper.short_name(), method_name, "Method")
		html_template.main   = self.__summary_section() + \
			self.__syntax_section() + \
			self.__parameters_section() + \
			self.__return_value()

		return html_template

	def __summary_section(self):
		return HtmlTemplate.fmt_non_empty("""
			<h2>Summary</h2>
			<p>%s</p>
			""", self.method_element.XmlComment.Summary())

	def __syntax_section(self):
		method_attr = self.method_element.Attributes
		access = ""
		if method_attr and not self.method_element.OwnerType.IsInterface: # all interface members are public
			access = method_attr.ToString()

		parameters = ", ".join( self.type_link(p.ParameterType) + " " + p.Name for p in self.method_element.GetParameters() )

		syntax = "%s %s %s (%s)" % (access, self.type_link(self.method_element.ReturnType), self.method_element.Name, parameters)

		return """
			<h2>Syntax</h2>
			<code>
			%s
			</code>
			""" % syntax

	def __parameter(self, parameter):
		return """<li>
		%s
		<br />
		Type: %s
		%s
		</li>""" % (parameter.Name, self.type_link(parameter.ParameterType), "todo param doc")

	def __parameters_section(self):
		return HtmlTemplate.fmt_non_empty("""
			<h2>Parameters</h2>
			<ol>
			%s
			</ol>
			""", "".join(self.__parameter(p) for p in self.method_element.GetParameters()))

	def __return_value(self):
		return """
		<h2>Return Value</h2>
		%s %s
		""" % (self.type_link(self.method_element.ReturnType), self.method_element.XmlComment.Section("returns"))

def make_visitor(nav, footer):
	def visitor(navigation_node):
		print "visting %s" % navigation_node
		html_template = navigation_node.contents_html_template()
		if not html_template:
			return

		html_template.nav    = nav
		html_template.footer = footer
		html_template.write_to(navigation_node.href())

	return visitor

#	def generate_nant_task_pages(self):
#		print "NAnt tasks:"
#		for type in self.documentation.Types:
#			if type.HasAttribute('NAnt.Core.Attributes.TaskName'):
#				taskName = type.GetAttribute('NAnt.Core.Attributes.TaskName').Name
#				print "todo: generate page for nant task " + taskName

def Generate(documentation):
	"""Entry point for IglooCastle"""
	print "Hello from python!"

	root_nav_node = NavigationDocumentationNode(documentation)
	nav           = root_nav_node.nav_html()
	footer        = """Generated by IglooCastle at
			""" + strftime("%Y-%m-%d %H:%M:%S", gmtime()) + """
			<link type=\"text/css\" rel=\"stylesheet\" href=\"style.css\" />
			<script src="jquery-1.11.1.min.js"></script>
			<script src="app.js"></script>
			</footer>"""
	visitor = make_visitor(nav, footer)
	root_nav_node.visit(visitor)
	print "Python out!"
